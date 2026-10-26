#nullable enable
using System;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Aero.Gen;
using Aero.Gen.Attributes;
using Aero.Protocol;
using GameServer.Extensions;
using Serilog;
using Serilog.Events;
using Shared.Udp;

namespace GameServer.Protocol;

public class Channel
{
    private const int _protocolHeaderSize = 80; // UDP + IP
    private const int _gameSocketHeaderSize = 4;
    private const int _totalHeaderSize = _protocolHeaderSize + _gameSocketHeaderSize;
    private const int _maxPacketSize = PacketServer.MTU - _totalHeaderSize;

    // Sub-packet framing: 2 byte header, plus a 2 byte sequence number on sequenced channels.
    private const int _channelHeaderSize = GamePacketHeader.Size;
    private const int _sequenceNumberSize = 2;

    // Message headers written by this channel. The GSS header is a typecode, the entity id with its
    // low byte replaced by the typecode (7 bytes of the 8 byte little endian value) and a message id.
    private const int _gssHeaderSize = GamePacket.GssHeaderSize;
    private const int _messageIdSize = 1;
    private const int _playerIdSize = 8;
    private const int _checksumSize = 4;

    // Number of received sequence numbers remembered for duplicate detection on reliable channels
    private const int _receivedSequenceWindow = 64;

    // GSS message ids, relative to the typecode of the view or controller the message belongs to
    private const byte _gssMessageUpdate = 1;
    private const byte _gssMessageChecksum = 2;
    private const byte _gssMessageViewKeyframe = 3;
    private const byte _gssMessageControllerKeyframe = 4;
    private const byte _gssMessageControllerRemove = 5;
    private const byte _gssMessageScopeOut = 6;

    private static readonly byte[] _xorByte = [0xFF, 0xAA, 0xCC];

    private readonly ILogger _logger;

    private readonly INetworkClient _client;
    private readonly ConcurrentQueue<GamePacket> _incomingPackets;
    private readonly ConcurrentQueue<Memory<byte>> _outgoingPackets;
    private readonly SortedDictionary<ushort, GamePacket> _incomingSplitMessagePackets;
    private readonly int _channelFramingSize;
    private readonly int _maxMessageSize;
    private readonly ReliableSender? _reliable;

    private int _sendSequenceNumber;
    private ushort _lastAck;
    private ushort _lastReceivedSequenceNumber;
    private ulong _receivedSequenceMask;
    private bool _hasReceivedSequence;
    private bool _inSplitMode;

    private Channel(ChannelType channelType, bool isSequenced, bool isReliable, bool isGSS, INetworkClient networkClient, ILogger logger, GssVersion gssProtocolVersion, MatrixVersion matrixProtocolVersion)
    {
        Type = channelType;
        IsSequenced = isSequenced;
        IsReliable = isReliable;
        IsGSS = isGSS;
        _client = networkClient;
        _logger = logger;
        GssProtocolVersion = gssProtocolVersion;
        MatrixProtocolVersion = matrixProtocolVersion;

        _channelFramingSize = isSequenced ? _channelHeaderSize + _sequenceNumberSize : _channelHeaderSize;
        _maxMessageSize = _maxPacketSize - _channelFramingSize;

        _incomingPackets = new ConcurrentQueue<GamePacket>();
        _outgoingPackets = new ConcurrentQueue<Memory<byte>>();
        _incomingSplitMessagePackets = [];
        _reliable = isReliable ? new ReliableSender() : null;
    }

    public delegate void PacketAvailableDelegate(GamePacket packet);

    public event PacketAvailableDelegate? PacketAvailable;

    private ChannelType Type { get; }
    private bool IsSequenced { get; }
    private bool IsReliable { get; }
    private bool IsGSS { get; }
    private GssVersion GssProtocolVersion { get; }
    private MatrixVersion MatrixProtocolVersion { get; }

    public static Dictionary<ChannelType, Channel> GetChannels(INetworkClient client, ILogger logger, GssVersion gssProtocolVersion, MatrixVersion matrixProtocolVersion)
    {
        return new Dictionary<ChannelType, Channel>
               {
                   { ChannelType.Control, new Channel(ChannelType.Control, false, false, false, client, logger, gssProtocolVersion, matrixProtocolVersion) },
                   { ChannelType.Matrix, new Channel(ChannelType.Matrix, true, true, false, client, logger, gssProtocolVersion, matrixProtocolVersion) },
                   { ChannelType.ReliableGss, new Channel(ChannelType.ReliableGss, true, true, true, client, logger, gssProtocolVersion, matrixProtocolVersion) },
                   { ChannelType.UnreliableGss, new Channel(ChannelType.UnreliableGss, true, false, true, client, logger, gssProtocolVersion, matrixProtocolVersion) }
               };
    }

    public void HandlePacket(GamePacket packet)
    {
        _incomingPackets.Enqueue(packet);
    }

    public void Process(CancellationToken ct)
    {
        while (_outgoingPackets.TryDequeue(out var qi))
        {
            _client.Send(qi);
        }

        if (_reliable?.Pump(DateTime.UtcNow, _client.Send) is { } failure)
        {
            _client.Close($"{failure} on {Type}, {_reliable.InFlightCount} frames unacked");
            return;
        }

        while (_incomingPackets.TryDequeue(out var packet))
        {
            var sequenceNumber = (ushort)0;
            if (IsSequenced && !packet.TryReadUInt16BigEndian(out sequenceNumber))
            {
                _logger.Warning("---> Truncated {Channel} packet: {PacketBytes} bytes", Type, packet.TotalBytes);
                continue;
            }

            if (packet.Header.ResendCount > 0)
            {
                packet = DecodeResend(packet);
                if (_logger.IsEnabled(LogEventLevel.Debug))
                {
                    _logger.Debug("---> Resent packet!!! C:{Channel}: {PacketBytes} bytes", Type, packet.TotalBytes);
                }
            }

            // Reliable channels remember the sequence numbers they have already handled. A resend of an
            // already handled message is acknowledged again, but never handed to the game a second time.
            var isDuplicate = IsReliable && IsDuplicateSequence(sequenceNumber);
            if (IsReliable)
            {
                TrackReceivedSequence(sequenceNumber);
            }

            if (isDuplicate)
            {
                _client.SendAck(Type, sequenceNumber, packet.Received);
                if (_logger.IsEnabled(LogEventLevel.Debug))
                {
                    _logger.Debug("---> Duplicate {Channel} packet SeqNum = {SeqNum}, not handled again", Type, sequenceNumber);
                }

                continue;
            }

            if (_inSplitMode)
            {
                _incomingSplitMessagePackets[sequenceNumber] = packet;
                if (!packet.Header.IsSplit)
                {
                    // Finish split mode
                    _inSplitMode = false;

                    var combinedPacket = new GamePacket(packet.Header, CombineSplitMessagePackets(), packet.Received);

                    _client.SendAck(Type, sequenceNumber, packet.Received);
                    _lastAck = sequenceNumber;
                    PacketAvailable?.Invoke(combinedPacket);
                }
            }
            else if (packet.Header.IsSplit)
            {
                // Enter split mode
                _inSplitMode = true;
                _incomingSplitMessagePackets[sequenceNumber] = packet;
                _client.SendAck(Type, sequenceNumber, packet.Received);
                _lastAck = sequenceNumber;
            }
            else
            {
                if (IsReliable && IsNewerSequence(sequenceNumber, _lastAck))
                {
                    _client.SendAck(Type, sequenceNumber, packet.Received);
                    _lastAck = sequenceNumber;
                }

                PacketAvailable?.Invoke(packet);
            }
        }
    }

    /// <summary>
    ///     Handles the client's ack for this reliable channel
    /// </summary>
    /// <param name="cumulative">The client has every frame up to this one</param>
    /// <param name="selective">Frames that arrived past a gap</param>
    public void HandleAck(ushort cumulative, ReadOnlySpan<ushort> selective)
    {
        _reliable?.Acked(cumulative, selective, DateTime.UtcNow);
    }

    /// <summary>
    ///     Send a GSS message whose body is already serialized
    /// </summary>
    /// <param name="entityId">Id of the entity the message is for</param>
    /// <param name="messageId">Message id relative to the typecode</param>
    /// <param name="typecode">Typecode of the entity matching the view or controller</param>
    /// <param name="messageData">Serialized message body</param>
    /// <returns>true if the operation succeeded, false in all other cases</returns>
    public bool SendGssMessage(ulong entityId, byte messageId, byte typecode, ReadOnlySpan<byte> messageData)
    {
        Span<byte> prefix = stackalloc byte[_gssHeaderSize];
        WriteGssHeader(prefix, entityId, messageId, typecode);
        return SendGss(prefix, messageData, typecode, entityId, messageId);
    }

    /// <summary>
    ///     Send a GSS Update message (View or Controller) whose change set is already serialized
    /// </summary>
    /// <param name="entityId">Id of the entity the view represents</param>
    /// <param name="typecode">Typecode of the view or controller</param>
    /// <param name="messageData">Serialized change set</param>
    /// <returns>true if the operation succeeded, false in all other cases</returns>
    public bool SendGssChanges(ulong entityId, byte typecode, ReadOnlySpan<byte> messageData)
    {
        return SendGssMessage(entityId, _gssMessageUpdate, typecode, messageData);
    }

    /// <summary>
    ///     Send a GSS View Keyframe message whose body is already serialized
    /// </summary>
    /// <param name="entityId">Id of the entity the view represents</param>
    /// <param name="typecode">Typecode of the view</param>
    /// <param name="messageData">Serialized keyframe</param>
    /// <returns>true if the operation succeeded, false in all other cases</returns>
    public bool SendGssViewKeyframe(ulong entityId, byte typecode, ReadOnlySpan<byte> messageData)
    {
        return SendGssMessage(entityId, _gssMessageViewKeyframe, typecode, messageData);
    }

    /// <summary>
    ///     Send a GSS Controller Keyframe message whose body is already serialized
    /// </summary>
    /// <param name="entityId">Id of the entity the controller represents</param>
    /// <param name="typecode">Typecode of the controller</param>
    /// <param name="playerId">Player id used in controller messages</param>
    /// <param name="messageData">Serialized keyframe</param>
    /// <returns>true if the operation succeeded, false in all other cases</returns>
    public bool SendGssControllerKeyframe(ulong entityId, byte typecode, ulong playerId, ReadOnlySpan<byte> messageData)
    {
        Span<byte> prefix = stackalloc byte[_gssHeaderSize + _playerIdSize];
        WriteGssHeader(prefix, entityId, _gssMessageControllerKeyframe, typecode);
        BinaryPrimitives.WriteUInt64LittleEndian(prefix[_gssHeaderSize..], playerId);
        return SendGss(prefix, messageData, typecode, entityId, _gssMessageControllerKeyframe);
    }

    /// <summary>
    ///     Send a GSS Controller Remove message
    /// </summary>
    /// <param name="entityId">Id of the entity the controller represents</param>
    /// <param name="typecode">Typecode of the controller</param>
    /// <param name="playerId">Player id used in controller messages</param>
    /// <returns>true if the operation succeeded, false in all other cases</returns>
    public bool SendGssControllerRemove(ulong entityId, byte typecode, ulong playerId)
    {
        Span<byte> prefix = stackalloc byte[_gssHeaderSize + _playerIdSize];
        WriteGssHeader(prefix, entityId, _gssMessageControllerRemove, typecode);
        BinaryPrimitives.WriteUInt64LittleEndian(prefix[_gssHeaderSize..], playerId);
        return SendGss(prefix, ReadOnlySpan<byte>.Empty, typecode, entityId, _gssMessageControllerRemove);
    }

    /// <summary>
    ///     Send a GSS View Checksum message
    /// </summary>
    /// <param name="entityId">Id of the entity the checksum is for</param>
    /// <param name="typecode">Typecode the checksum is for</param>
    /// <param name="checksum">Checksum value</param>
    /// <returns>true if the operation succeeded, false in all other cases</returns>
    public bool SendGssChecksum(ulong entityId, byte typecode, uint checksum)
    {
        Span<byte> prefix = stackalloc byte[_gssHeaderSize + _checksumSize];
        WriteGssHeader(prefix, entityId, _gssMessageChecksum, typecode);
        BinaryPrimitives.WriteUInt32LittleEndian(prefix[_gssHeaderSize..], checksum);
        return SendGss(prefix, ReadOnlySpan<byte>.Empty, typecode, entityId, _gssMessageChecksum);
    }

    /// <summary>
    ///     Send a GSS View ScopeOut/Remove message
    /// </summary>
    /// <param name="entityId">Id of the entity the view represents</param>
    /// <param name="typecode">Typecode of the view</param>
    /// <returns>true if the operation succeeded, false in all other cases</returns>
    public bool SendGssScopeOut(ulong entityId, byte typecode)
    {
        Span<byte> prefix = stackalloc byte[_gssHeaderSize];
        WriteGssHeader(prefix, entityId, _gssMessageScopeOut, typecode);
        return SendGss(prefix, ReadOnlySpan<byte>.Empty, typecode, entityId, _gssMessageScopeOut);
    }

    /// <summary>
    ///     Send a Matrix message whose body is already serialized
    /// </summary>
    /// <param name="messageId">Wire id of the matrix message being sent</param>
    /// <param name="messageData">Serialized message body</param>
    /// <returns>true if the operation succeeded, false in all other cases</returns>
    public bool SendMatrixMessage(byte messageId, ReadOnlySpan<byte> messageData)
    {
        return SendMatrixOrControl(messageId, messageData);
    }

    /// <summary>
    ///     Send a Control message whose body is already serialized
    /// </summary>
    /// <param name="messageId">Id of the control message being sent</param>
    /// <param name="messageData">Serialized message body</param>
    /// <returns>true if the operation succeeded, false in all other cases</returns>
    public bool SendControlMessage(byte messageId, ReadOnlySpan<byte> messageData)
    {
        return SendMatrixOrControl(messageId, messageData);
    }

    /// <summary>
    ///     Send a GSS Update message (View or Controller) using Aero's change tracking
    /// </summary>
    /// <typeparam name="TViewOrController">The Aero Class of AeroGenTypes.View or AeroGenTypes.Controller</typeparam>
    /// <param name="view">The Aero View or Controller</param>
    /// <param name="entityId">Id of the entity the View represents</param>
    /// <returns>true if the operation succeeded, false in all other cases</returns>
    /// <exception cref="ArgumentException">The passed view does not have the AeroMessageIdAttribute</exception>
    public bool SendChanges<TViewOrController>(TViewOrController view, ulong entityId)
        where TViewOrController : class, IAeroViewInterface
    {
        if (AeroMessageMetadata<TViewOrController>.MessageId is null)
        {
            throw new ArgumentException($"The passed Aero class is required to be annotated with {nameof(AeroMessageIdAttribute)} (Type: {typeof(TViewOrController).FullName})");
        }

        if (!WireIds.TryGetGssWireIds(typeof(TViewOrController), GssProtocolVersion, out var typecode, out _))
        {
            _logger.Error("Could not resolve GSS wire id for {Type} in protocol version {Version}", typeof(TViewOrController).FullName, GssProtocolVersion);
            return false;
        }

        // Generate and send
        view.SerializeChangesToMemory(out var packetMemory);
        return SendGssChanges(entityId, typecode, packetMemory.Span);
    }

    /// <summary>
    ///     Send a GSS View Checksum message
    /// </summary>
    /// <param name="entityId">Id of the entity the checksum is for</param>
    /// <param name="typecode">View type the checksum is for</param>
    /// <param name="checksum">Checksum value</param>
    /// <returns>true if the operation succeeded, false in all other cases</returns>
    public bool SendChecksum(ulong entityId, byte typecode, uint checksum)
    {
        return SendGssChecksum(entityId, typecode, checksum);
    }

    /// <summary>
    ///     Send a GSS View Keyframe message
    /// </summary>
    /// <typeparam name="TView">The Aero Class of AeroGenTypes.View</typeparam>
    /// <param name="view">The Aero View</param>
    /// <param name="entityId">Id of the entity the View represents</param>
    /// <returns>true if the operation succeeded, false in all other cases</returns>
    /// <exception cref="ArgumentException">The passed view does not have the AeroMessageIdAttribute or AeroAttribute, or it is not a AeroGenTypes.View</exception>
    public bool SendViewKeyframe<TView>(TView view, ulong entityId)
        where TView : class, IAero
    {
        if (!TryValidateView<TView>(out var wireTypecode))
        {
            return false;
        }

        // Generate and send
        view.SerializeToMemory(out var packetMemory);
        return SendGssViewKeyframe(entityId, wireTypecode, packetMemory.Span);
    }

    /// <summary>
    ///     Send a GSS View ScopeOut/Remove message
    /// </summary>
    /// <typeparam name="TView">The Aero Class of AeroGenTypes.View</typeparam>
    /// <param name="view">The Aero View</param>
    /// <param name="entityId">Id of the entity the View represents</param>
    /// <returns>true if the operation succeeded, false in all other cases</returns>
    /// <exception cref="ArgumentException">The passed view does not have the AeroMessageIdAttribute or AeroAttribute, or it is not a AeroGenTypes.View</exception>
    public bool SendViewScopeOut<TView>(TView view, ulong entityId)
        where TView : class, IAero
    {
        if (!TryValidateView<TView>(out var wireTypecode))
        {
            return false;
        }

        return SendGssScopeOut(entityId, wireTypecode);
    }

    /// <summary>
    ///     Send a GSS View ScopeOut/Remove message
    /// </summary>
    /// <param name="entityId">Id of the entity the View represents</param>
    /// <param name="wireTypecode">Wire typecode of the view</param>
    /// <returns>true if the operation succeeded, false in all other cases</returns>
    public bool SendScopeOut(ulong entityId, byte wireTypecode)
    {
        return SendGssScopeOut(entityId, wireTypecode);
    }

    /// <summary>
    ///     Send a GSS Controller Keyframe message
    /// </summary>
    /// <typeparam name="TController">The Aero Class of AeroGenTypes.Controller</typeparam>
    /// <param name="controller">The Aero Controller</param>
    /// <param name="entityId">Id of the entity the View represents</param>
    /// <param name="playerId">Player id (?) used in controller messages</param>
    /// <returns>true if the operation succeeded, false in all other cases</returns>
    /// <exception cref="ArgumentException">The passed controller does not have the AeroMessageIdAttribute or AeroAttribute, or it is not a AeroGenTypes.Controller</exception>
    public bool SendControllerKeyframe<TController>(TController controller, ulong entityId, ulong playerId)
        where TController : class, IAero
    {
        if (!TryValidateController<TController>(out var wireTypecode))
        {
            return false;
        }

        // Generate and send
        controller.SerializeToMemory(out var packetMemory);
        return SendGssControllerKeyframe(entityId, wireTypecode, playerId, packetMemory.Span);
    }

    /// <summary>
    ///     Send a GSS Controller Remove message
    /// </summary>
    /// <typeparam name="TController">The Aero Class of AeroGenTypes.Controller</typeparam>
    /// <param name="controller">The Aero Controller</param>
    /// <param name="entityId">Id of the entity the View represents</param>
    /// <param name="playerId">Player id (?) used in controller messages</param>
    /// <returns>true if the operation succeeded, false in all other cases</returns>
    /// <exception cref="ArgumentException">The passed controller does not have the AeroMessageIdAttribute or AeroAttribute, or it is not a AeroGenTypes.Controller</exception>
    public bool SendControllerRemove<TController>(TController controller, ulong entityId, ulong playerId)
        where TController : class, IAero
    {
        if (!TryValidateController<TController>(out var wireTypecode))
        {
            return false;
        }

        return SendGssControllerRemove(entityId, wireTypecode, playerId);
    }

    /// <summary>
    ///     Send a Control, Matrix or Normal GSS Message
    /// </summary>
    /// <typeparam name="TNormal">The Aero class of AeroGenTypes.Normal</typeparam>
    /// <param name="message">The Aero message</param>
    /// <param name="entityId">For GSS Messages, Id of the entity the message is for</param>
    /// <returns>true if the operation succeeded, false in all other cases</returns>
    /// <exception cref="ArgumentException">The passed message does not have the AeroMessageIdAttribute, AeroAttribute, or it is not a AeroGenTypes.Normal</exception>
    public bool SendMessage<TNormal>(TNormal message, ulong entityId = 0)
        where TNormal : class, IAero
    {
        if (AeroMessageMetadata<TNormal>.MessageId is not { } aeroMessageIdAttribute)
        {
            throw new ArgumentException($"The passed package is required to be annotated with {nameof(AeroMessageIdAttribute)} (Type: {typeof(TNormal).FullName})");
        }

        if (AeroMessageMetadata<TNormal>.Aero is not { } aeroAttr)
        {
            throw new ArgumentException($"The passed Aero controller is required to be annotated with {nameof(AeroAttribute)} (Type: {typeof(TNormal).FullName})");
        }

        if (aeroAttr.AeroType != AeroGenTypes.Normal)
        {
            throw new ArgumentException($"The AeroType should be Normal if it should be sent as a message. Received {aeroAttr.AeroType} instead. (Type: {typeof(TNormal).FullName})");
        }

        var type = aeroMessageIdAttribute.Typ;

        switch (type)
        {
            case AeroMessageIdAttribute.MsgType.Matrix:
                if (!WireIds.TryGetMatrixWireId(typeof(TNormal), MatrixProtocolVersion, out var matrixMessageId))
                {
                    _logger.Error("Could not resolve Matrix wire id for {Type} in protocol version {Version}", typeof(TNormal).FullName, MatrixProtocolVersion);
                    return false;
                }

                message.SerializeToMemory(out var matrixMemory);
                return SendMatrixMessage(matrixMessageId, matrixMemory.Span);
            case AeroMessageIdAttribute.MsgType.GSS:
                if (!WireIds.TryGetGssWireIds(typeof(TNormal), GssProtocolVersion, out var typecode, out var gssMessageId))
                {
                    _logger.Error("Could not resolve GSS wire id for {Type} in protocol version {Version}", typeof(TNormal).FullName, GssProtocolVersion);
                    return false;
                }

                message.SerializeToMemory(out var gssMemory);
                return SendGssMessage(entityId, gssMessageId, typecode, gssMemory.Span);
            case AeroMessageIdAttribute.MsgType.Control:
                if (!WireIds.TryGetControlWireId(typeof(TNormal), out var controlMessageId))
                {
                    _logger.Error("Could not resolve Control wire id for {Type}", typeof(TNormal).FullName);
                    return false;
                }

                message.SerializeToMemory(out var controlMemory);
                return SendControlMessage(controlMessageId, controlMemory.Span);
            default:
                throw new ArgumentException("Message type not implemented");
        }
    }

    /// <summary>
    ///     Write the 9 byte GSS header: the entity id in little endian with its low byte replaced by the
    ///     typecode, followed by the message id
    /// </summary>
    /// <param name="destination">Destination, exactly <see cref="_gssHeaderSize" /> bytes long</param>
    /// <param name="entityId">Id of the entity the message is for</param>
    /// <param name="messageId">Message id relative to the typecode</param>
    /// <param name="typecode">Typecode of the entity matching the view or controller</param>
    private static void WriteGssHeader(Span<byte> destination, ulong entityId, byte messageId, byte typecode)
    {
        BinaryPrimitives.WriteUInt64LittleEndian(destination, entityId);
        destination[0] = typecode;
        destination[_gssHeaderSize - 1] = messageId;
    }

    private static bool IsNewerSequence(ushort candidate, ushort reference)
    {
        return unchecked((short)(candidate - reference)) > 0;
    }

    /// <summary>
    ///     Check a received sequence number against the window of sequence numbers already handled
    /// </summary>
    /// <param name="sequenceNumber">Sequence number of the received packet</param>
    /// <returns>true if the sequence number was already handled, false otherwise</returns>
    private bool IsDuplicateSequence(ushort sequenceNumber)
    {
        if (!_hasReceivedSequence)
        {
            return false;
        }

        var delta = unchecked((short)(sequenceNumber - _lastReceivedSequenceNumber));
        if (delta >= 0)
        {
            return delta == 0;
        }

        return delta > -_receivedSequenceWindow && (_receivedSequenceMask & (1UL << -delta)) != 0;
    }

    /// <summary>
    ///     Slide the received sequence number window so resends can be detected
    /// </summary>
    /// <param name="sequenceNumber">Sequence number of the packet that is about to be handled</param>
    private void TrackReceivedSequence(ushort sequenceNumber)
    {
        if (!_hasReceivedSequence)
        {
            _hasReceivedSequence = true;
            _lastReceivedSequenceNumber = sequenceNumber;
            _receivedSequenceMask = 1UL;
            return;
        }

        var delta = unchecked((short)(sequenceNumber - _lastReceivedSequenceNumber));
        if (delta > 0)
        {
            _receivedSequenceMask = delta >= _receivedSequenceWindow ? 1UL : (_receivedSequenceMask << delta) | 1UL;
            _lastReceivedSequenceNumber = sequenceNumber;
        }
        else if (delta < 0 && delta > -_receivedSequenceWindow)
        {
            _receivedSequenceMask |= 1UL << -delta;
        }
    }

    /// <summary>
    ///     Undo the resend obfuscation the client applies to retransmitted data
    /// </summary>
    /// <param name="packet">Received packet with a non-zero resend count</param>
    /// <returns>A packet whose data is the de-obfuscated payload</returns>
    private GamePacket DecodeResend(GamePacket packet)
    {
        var xorIndex = packet.Header.ResendCount - 1;
        var data = packet.Peek(packet.BytesRemaining).ToArray();
        for (var i = 0; i < data.Length; i++)
        {
            data[i] ^= _xorByte[xorIndex];
        }

        return new GamePacket(packet.Header, data, packet.Received);
    }

    /// <summary>
    ///     Concatenate the buffered fragments of a split message, in sequence number order
    /// </summary>
    /// <returns>The concatenated message data of all buffered fragments</returns>
    private ReadOnlyMemory<byte> CombineSplitMessagePackets()
    {
        var total = 0;
        foreach (var fragment in _incomingSplitMessagePackets)
        {
            total += fragment.Value.BytesRemaining;
        }

        var combined = new byte[total];
        var destination = combined.AsSpan();
        foreach (var fragment in _incomingSplitMessagePackets)
        {
            fragment.Value.Peek(fragment.Value.BytesRemaining).Span.CopyTo(destination);
            destination = destination[fragment.Value.BytesRemaining..];
        }

        _incomingSplitMessagePackets.Clear();
        return combined;
    }

    private bool TryValidateView<TView>(out byte wireTypecode)
        where TView : class, IAero
    {
        wireTypecode = 0;

        if (AeroMessageMetadata<TView>.MessageId is null)
        {
            throw new ArgumentException($"The passed Aero view is required to be annotated with {nameof(AeroMessageIdAttribute)} (Type: {typeof(TView).FullName})");
        }

        if (AeroMessageMetadata<TView>.Aero is not { } aeroAttr)
        {
            throw new ArgumentException($"The passed Aero view is required to be annotated with {nameof(AeroAttribute)} (Type: {typeof(TView).FullName})");
        }

        if (aeroAttr.AeroType != AeroGenTypes.View)
        {
            throw new ArgumentException($"The AeroType should be View to send View Keyframe. Received {aeroAttr.AeroType} instead. (Type: {typeof(TView).FullName})");
        }

        if (!WireIds.TryGetGssWireIds(typeof(TView), GssProtocolVersion, out wireTypecode, out _))
        {
            _logger.Error("Could not resolve GSS wire id for {Type} in protocol version {Version}", typeof(TView).FullName, GssProtocolVersion);
            return false;
        }

        return true;
    }

    private bool TryValidateController<TController>(out byte wireTypecode)
        where TController : class, IAero
    {
        wireTypecode = 0;

        if (AeroMessageMetadata<TController>.MessageId is null)
        {
            throw new ArgumentException($"The passed Aero controller is required to be annotated with {nameof(AeroMessageIdAttribute)} (Type: {typeof(TController).FullName})");
        }

        if (AeroMessageMetadata<TController>.Aero is not { } aeroAttr)
        {
            throw new ArgumentException($"The passed Aero controller is required to be annotated with {nameof(AeroAttribute)} (Type: {typeof(TController).FullName})");
        }

        if (aeroAttr.AeroType != AeroGenTypes.Controller)
        {
            throw new ArgumentException($"The AeroType should be Controller to send Controller Keyframe. Received {aeroAttr.AeroType} instead. (Type: {typeof(TController).FullName})");
        }

        if (!WireIds.TryGetGssWireIds(typeof(TController), GssProtocolVersion, out wireTypecode, out _))
        {
            _logger.Error("Could not resolve GSS wire id for {Type} in protocol version {Version}", typeof(TController).FullName, GssProtocolVersion);
            return false;
        }

        return true;
    }

    private bool SendMatrixOrControl(byte messageId, ReadOnlySpan<byte> messageData)
    {
        Span<byte> prefix = stackalloc byte[_messageIdSize];
        prefix[0] = messageId;

        if (_logger.IsEnabled(LogEventLevel.Verbose))
        {
            _logger.Verbose("<-- {Channel}: MsgID = 0x{MessageId:X2}", Type, messageId);
        }

        return Send(prefix, messageData);
    }

    private bool SendGss(ReadOnlySpan<byte> prefix, ReadOnlySpan<byte> messageData, byte typecode, ulong entityId, byte messageId)
    {
        if (_logger.IsEnabled(LogEventLevel.Verbose))
        {
            _logger.Verbose("<-- {Channel}: Typecode = {Typecode} Entity = 0x{EntityId:X16} MsgID = 0x{MessageId:X2}", Type, typecode, entityId, messageId);
        }

        return Send(prefix, messageData);
    }

    /// <summary>
    ///     Frame and queue a message: the prefix (message header plus any inline fields) and the message
    ///     body are written straight into the send buffer, splitting over multiple sub-packets if needed
    /// </summary>
    /// <param name="prefix">Message header and any inline fields, written before the message body</param>
    /// <param name="messageData">Serialized message body</param>
    /// <returns>true if the operation succeeded, false in all other cases</returns>
    private bool Send(ReadOnlySpan<byte> prefix, ReadOnlySpan<byte> messageData)
    {
        // TODO: Send UGSS messages that are split over RGSS
        var totalSize = prefix.Length + messageData.Length;
        if (totalSize <= _maxMessageSize)
        {
            var buffer = new byte[_channelFramingSize + totalSize];
            var destination = buffer.AsSpan(_channelFramingSize);
            prefix.CopyTo(destination);
            messageData.CopyTo(destination[prefix.Length..]);
            WriteFraming(buffer, (ushort)buffer.Length, false);
            Enqueue([buffer]);
            return true;
        }

        var fragments = new List<byte[]>();
        for (var offset = 0; offset < totalSize; offset += _maxMessageSize)
        {
            var fragmentSize = Math.Min(_maxMessageSize, totalSize - offset);
            var buffer = new byte[_channelFramingSize + fragmentSize];
            var destination = buffer.AsSpan(_channelFramingSize);

            var prefixBytes = Math.Min(prefix.Length - Math.Min(offset, prefix.Length), fragmentSize);
            if (prefixBytes > 0)
            {
                prefix.Slice(offset, prefixBytes).CopyTo(destination);
            }

            messageData.Slice(Math.Max(0, offset - prefix.Length), fragmentSize - prefixBytes)
                       .CopyTo(destination[prefixBytes..]);

            WriteFraming(buffer, (ushort)buffer.Length, offset + fragmentSize < totalSize);
            fragments.Add(buffer);
        }

        Enqueue(fragments.ToArray());
        return true;
    }

    private void WriteFraming(byte[] buffer, ushort packetLength, bool isSplit)
    {
        var destination = buffer.AsSpan();
        // Reliable frames get their sequence number when they go out, see ReliableSender
        if (IsSequenced && _reliable == null)
        {
            unchecked
            {
                var sequenceNumber = (ushort)Interlocked.Increment(ref _sendSequenceNumber);
                if (IsReliable && _logger.IsEnabled(LogEventLevel.Verbose))
                {
                    _logger.Verbose("<- {Channel} SeqNum = {SeqNum}", Type, sequenceNumber);
                }

                BinaryPrimitives.WriteUInt16BigEndian(destination[_channelHeaderSize..], sequenceNumber);
            }
        }

        new GamePacketHeader(Type, 0, isSplit, packetLength).Write(destination);
    }

    private void Enqueue(byte[][] frames)
    {
        if (_reliable != null)
        {
            _reliable.Enqueue(frames);
            return;
        }

        foreach (var frame in frames)
        {
            if (IsGSS)
            {
                _client.SequencedMessages.Enqueue(frame);
            }
            else
            {
                _outgoingPackets.Enqueue(frame);
            }
        }
    }

    /// <summary>
    ///     Aero attributes of a message type, resolved once per closed generic type instead of per send
    /// </summary>
    private static class AeroMessageMetadata<T>
        where T : class, IAero
    {
        internal static readonly AeroMessageIdAttribute? MessageId = typeof(T).GetAttribute<AeroMessageIdAttribute>();
        internal static readonly AeroAttribute? Aero = typeof(T).GetAttribute<AeroAttribute>();
    }
}
