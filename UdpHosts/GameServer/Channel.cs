#nullable enable
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Aero.Gen;
using Aero.Gen.Attributes;
using GameServer.Extensions;
using GameServer.Packets;
using Serilog;
using Shared.Udp;

namespace GameServer;

public class Channel
{
    private const int ProtocolHeaderSize = 80; // UDP + IP
    private const int GameSocketHeaderSize = 4;
    private const int TotalHeaderSize = ProtocolHeaderSize + GameSocketHeaderSize;
    private const int MaxPacketSize = PacketServer.MTU - TotalHeaderSize;
    
    private static readonly byte[] XorByte = { 0x00, 0xFF, 0xCC, 0xAA };
    private static readonly ulong[] XorULong = { 0x00, 0xFFFFFFFFFFFFFFFF, 0xCCCCCCCCCCCCCCCC, 0xAAAAAAAAAAAAAAAA };

    private readonly ILogger _logger;

    private readonly INetworkClient _client;
    private readonly ConcurrentQueue<GamePacket> _incomingPackets;
    private readonly ConcurrentQueue<Memory<byte>> _outgoingPackets;
    private SortedDictionary<ushort, GamePacket> _incomingSplitMessagePackets;

    private Channel(ChannelType channelType, bool isSequenced, bool isReliable, INetworkClient networkClient, ILogger logger)
    {
        Type = channelType;
        IsSequenced = isSequenced;
        IsReliable = isReliable;
        _client = networkClient;
        _logger = logger;
        CurrentSequenceNumber = 1;
        LastAck = 0;

        _incomingPackets = new ConcurrentQueue<GamePacket>();
        _outgoingPackets = new ConcurrentQueue<Memory<byte>>();
        _incomingSplitMessagePackets = new SortedDictionary<ushort, GamePacket>();
    }

    public delegate void PacketAvailableDelegate(GamePacket packet);
    
    public event PacketAvailableDelegate? PacketAvailable;
    
    private ChannelType Type { get; }
    private bool IsSequenced { get; }
    private bool IsReliable { get; }
    private ushort CurrentSequenceNumber { get; set; }
    private DateTime LastActivity { get; set; }
    private ushort LastAck { get; set; }
    private bool InSplitMode { get; set; }

    public static Dictionary<ChannelType, Channel> GetChannels(INetworkClient client, ILogger logger)
    {
        return new Dictionary<ChannelType, Channel>
               {
                   { ChannelType.Control, new Channel(ChannelType.Control, false, false, client, logger) },
                   { ChannelType.Matrix, new Channel(ChannelType.Matrix, true, true, client, logger) },
                   { ChannelType.ReliableGss, new Channel(ChannelType.ReliableGss, true, true, client, logger) },
                   { ChannelType.UnreliableGss, new Channel(ChannelType.UnreliableGss, true, false, client, logger) }
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
            LastActivity = DateTime.Now;
        }

        while (_incomingPackets.TryDequeue(out var packet))
        {
            ushort sequenceNumber = 0;
            if (IsSequenced)
            {
                sequenceNumber = Utils.SimpleFixEndianness(packet.Read<ushort>());
            }

            // TODO: Verify if resent message handling works and resolve any issues
            if (packet.Header.ResendCount > 0)
            {
                // de-xor data
                var x = packet.PacketData.Length >> 3;
                var data = packet.PacketData.ToArray();
                var dataTemp = Array.Empty<byte>();

                if (x > 0)
                {
                    var uSpan = MemoryMarshal.Cast<byte, ulong>(data);

                    for (var i = 0; i < x; i++)
                    {
                        uSpan[i] ^= XorULong[packet.Header.ResendCount];
                    }

                    dataTemp = MemoryMarshal.Cast<ulong, byte>(uSpan).ToArray(); // override old size
                }

                // copy back
                for (var i = 0; i < dataTemp.Length; i++)
                {
                    data[i] = dataTemp[i];
                }

                for (var i = x * 8; i < packet.PacketData.Length; i++)
                {
                    data[i] ^= XorByte[packet.Header.ResendCount];
                }

                packet = new GamePacket(packet.Header, new ReadOnlyMemory<byte>(data));
                _logger.Fatal("---> Resent packet!!! C:{0}: {1} bytes", Type, packet.TotalBytes);
            }

            if (InSplitMode)
            {
                _incomingSplitMessagePackets.Add(sequenceNumber, packet);
                if (!packet.Header.IsSplit)
                {
                    // Finish split mode
                    InSplitMode = false;
                    var combined = _incomingSplitMessagePackets
                    .SelectMany((pair) => pair.Value.PacketData[2..].ToArray()) // Skip seqnum
                    .ToArray();
                    _incomingSplitMessagePackets.Clear();
                    
                    var combinedPacket = new GamePacket(packet.Header, new ReadOnlyMemory<byte>(combined));

                    _client.SendAck(Type, sequenceNumber, packet.Received);
                    LastAck = sequenceNumber;

                    PacketAvailable?.Invoke(combinedPacket);
                    _logger.Fatal("---> Collected Split Packet");
                }
            }
            else if (packet.Header.IsSplit)
            {
                // Enter split mode
                InSplitMode = true;
                _incomingSplitMessagePackets.Add(sequenceNumber, packet);
            }
            else
            {
                if (IsReliable && (sequenceNumber > LastAck || (sequenceNumber < 0xff && LastAck > 0xff00)))
                {
                    _client.SendAck(Type, sequenceNumber, packet.Received);
                    LastAck = sequenceNumber;
                }

                PacketAvailable?.Invoke(packet);
            }
            
            LastActivity = DateTime.Now;
        }
    }

    /// <summary>
    ///     Send a GSS View Checksum message
    /// </summary>
    /// <param name="entityId">Id of the entity</param>
    /// <param name="controllerId">View the checksum is for</param>
    /// <param name="checksum">Checksum value</param>
    /// <returns>true if the operation succeeded, false in all other cases</returns>
    public bool SendChecksum(ulong entityId, Enums.GSS.Controllers controllerId, uint checksum)
    {
        var messageData = Serializer.WritePrimitive(checksum);
        return SendPacketMemory(entityId, 2, controllerId, ref messageData);
    }

    /// <summary>
    ///     Send an <see cref="IAero" /> package to the client
    /// </summary>
    /// <typeparam name="TPacket">The type of the packet</typeparam>
    /// <param name="packet">The Aero message</param>
    /// <param name="entityId">Id of the entity the packet is for</param>
    /// <param name="messageIdOverride">Optional way to override the message ID</param>
    /// <param name="messageEnumType">Optionally, the enum type containing the message id may be specified for enhanced verbose-level logging</param>
    /// <returns>true if the operation succeeded, false in all other cases</returns>
    /// <exception cref="ArgumentException">The passed packet does not have the AeroMessageIdAttribute</exception>
    public bool SendMessage<TPacket>(TPacket packet, ulong entityId = 0, byte messageIdOverride = 0, Type? messageEnumType = null)
        where TPacket : class, IAero
    {
        if (typeof(TPacket).GetCustomAttributes(typeof(AeroMessageIdAttribute), false).FirstOrDefault() is not AeroMessageIdAttribute aeroMessageIdAttribute)
        {
            throw new ArgumentException($"The passed package is required to be annotated with {nameof(AeroMessageIdAttribute)} (Type: {typeof(TPacket).FullName})");
        }

        var type = aeroMessageIdAttribute.Typ;
        var messageId = (byte)aeroMessageIdAttribute.MessageId;
        if (messageIdOverride != 0)
        {
            messageId = messageIdOverride;
        }

        packet.SerializeToMemory(out var packetMemory);

        switch (type)
        {
            case AeroMessageIdAttribute.MsgType.Matrix:
                return SendPacketMemoryMatrix(messageId, ref packetMemory, messageEnumType);
            case AeroMessageIdAttribute.MsgType.GSS:
                {
                    var controllerId = (Enums.GSS.Controllers)aeroMessageIdAttribute.ControllerId;
                    return SendPacketMemory(entityId, messageId, controllerId, ref packetMemory, messageEnumType);
                }
            
            case AeroMessageIdAttribute.MsgType.Control:
                return SendPacketMemoryMatrix(messageId, ref packetMemory, messageEnumType); // Everything's gonna be just fine
            default:
                throw new ArgumentException("Message type not implemented");
        }
    }

    public bool SendControllerKeyframe<TPacket>(TPacket packet, ulong entityId, ulong playerId)
        where TPacket : class, IAero
    {
        if (typeof(TPacket).GetCustomAttributes(typeof(AeroMessageIdAttribute), false).FirstOrDefault() is not AeroMessageIdAttribute aeroMsgAttr)
        {
            throw new ArgumentException($"The passed package is required to be annotated with {nameof(AeroMessageIdAttribute)} (Type: {typeof(TPacket).FullName})");
        }

        var controllerId = (Enums.GSS.Controllers)aeroMsgAttr.ControllerId;
        packet.SerializeToMemory(out var packetMemory);
        var messageData = new Memory<byte>(new byte[8 + packetMemory.Length]);
        packetMemory.CopyTo(messageData[8..]);
        Serializer.WritePrimitive(playerId).CopyTo(messageData);
        return SendPacketMemory(entityId, 4, controllerId, ref messageData);
    }

    public bool SendControllerRemove<TPacket>(TPacket packet, ulong entityId, ulong playerId)
        where TPacket : class, IAero
    {
        if (typeof(TPacket).GetCustomAttributes(typeof(AeroMessageIdAttribute), false).FirstOrDefault() is not AeroMessageIdAttribute aeroMsgAttr)
        {
            throw new ArgumentException($"The passed package is required to be annotated with {nameof(AeroMessageIdAttribute)} (Type: {typeof(TPacket).FullName})");
        }

        var controllerId = (Enums.GSS.Controllers)aeroMsgAttr.ControllerId;
        var messageData = new Memory<byte>(new byte[8]);
        Serializer.WritePrimitive(playerId).CopyTo(messageData);
        return SendPacketMemory(entityId, 5, controllerId, ref messageData);
    }

    public bool SendViewScopeOut<TPacket>(TPacket packet, ulong entityId)
        where TPacket : class, IAero
    {
        if (typeof(TPacket).GetCustomAttributes(typeof(AeroMessageIdAttribute), false).FirstOrDefault() is not AeroMessageIdAttribute aeroMsgAttr)
        {
            throw new ArgumentException($"The passed package is required to be annotated with {nameof(AeroMessageIdAttribute)} (Type: {typeof(TPacket).FullName})");
        }

        var controllerId = (Enums.GSS.Controllers)aeroMsgAttr.ControllerId;
        var messageData = new Memory<byte>(new byte[0]);
        return SendPacketMemory(entityId, 6, controllerId, ref messageData);
    }

    public bool SendChanges<TPacket>(TPacket packet, ulong entityId)
        where TPacket : class, IAeroViewInterface
    {
        if (typeof(TPacket).GetCustomAttributes(typeof(AeroMessageIdAttribute), false).FirstOrDefault() is not AeroMessageIdAttribute aeroMsgAttr)
        {
            throw new ArgumentException($"The passed package is required to be annotated with {nameof(AeroMessageIdAttribute)} (Type: {typeof(TPacket).FullName})");
        }

        var typeCode = (Enums.GSS.Controllers)aeroMsgAttr.ControllerId;

        packet.SerializeChangesToMemory(out var packetMemory);

        return SendPacketMemory(entityId, 1, typeCode, ref packetMemory);
    }

    public bool SendChanges<TPacket>(TPacket packet, ulong entityId, Memory<byte> packetMemory)
        where TPacket : class, IAeroViewInterface
    {
        if (typeof(TPacket).GetCustomAttributes(typeof(AeroMessageIdAttribute), false).FirstOrDefault() is not AeroMessageIdAttribute aeroMsgAttr)
        {
            throw new ArgumentException($"The passed package is required to be annotated with {nameof(AeroMessageIdAttribute)} (Type: {typeof(TPacket).FullName})");
        }

        var typeCode = (Enums.GSS.Controllers)aeroMsgAttr.ControllerId;

        return SendPacketMemory(entityId, 1, typeCode, ref packetMemory);
    }

    /// <summary>
    ///     Send serialized data of a gss channel packet to the client
    /// </summary>
    /// <param name="entityId">Id of the entity the packet is for</param>
    /// <param name="messageId">Message Id relative to the controllerId</param>
    /// <param name="controllerId">Typecode of the entity matching the view or controller</param>
    /// <param name="packetToSend">Memory buffer</param>
    /// <param name="msgEnumType">Optionally, the enum type containing the message id may be specified for enhanced verbose-level logging</param>
    /// <returns>true if the operation succeeded, false in all other cases</returns>
    /// <exception cref="InvalidOperationException">If <see cref="msgEnumType" /> is not null and does not contain an element with a value equal to <see cref="messageId" /> </exception>
    private bool SendPacketMemory(ulong entityId,
                                  byte messageId,
                                  Enums.GSS.Controllers controllerId,
                                  ref Memory<byte> packetToSend,
                                  Type? msgEnumType = null)
    {
        const int HeaderByteSize = 9;

        var serializedData = new Memory<byte>(new byte[HeaderByteSize + packetToSend.Length]);
        packetToSend.CopyTo(serializedData[HeaderByteSize..]);

        Serializer.WritePrimitive(entityId).CopyTo(serializedData);

        // Intentionally overwrite first byte of Entity ID
        Serializer.WritePrimitive((byte)controllerId).CopyTo(serializedData);
        Serializer.WritePrimitive(messageId).CopyTo(serializedData[8..]);

        if (msgEnumType == null)
        {
            _logger.Verbose("<-- {0}: Controller = {1} Entity = 0x{2:X16} MsgID = 0x{3:X2}",
                            Type,
                            controllerId,
                            entityId,
                            messageId);
        }
        else
        {
            _logger.Verbose("<-- {0}: Controller = {1} Entity = 0x{2:X16} MsgID = {3} (0x{4:X2})",
                            Type,
                            controllerId,
                            entityId,
                            Enum.Parse(msgEnumType,
                                       Enum.GetName(msgEnumType, messageId) ?? throw new InvalidOperationException($"Message enum type {msgEnumType.FullName} has no element with a value of {messageId}")),
                            messageId);
        }

        return Send(serializedData);
    }

    /// <summary>
    ///     Send serialized data of a matrix channel packet to the client
    /// </summary>
    /// <param name="messageId">Id of the matrix message being sent</param>
    /// <param name="packetMemory">Memory buffer</param>
    /// <param name="msgEnumType">TODO: Optionally, the enum type containing the message id may be specified for enhanced verbose-level logging</param>
    /// <returns>true if the operation succeeded, false in all other cases</returns>
    private bool SendPacketMemoryMatrix(byte messageId, ref Memory<byte> packetMemory, Type? msgEnumType = null)
    {
        const int HeaderByteSize = 1;
        var serializedData = new Memory<byte>(new byte[HeaderByteSize + packetMemory.Length]);
        packetMemory.CopyTo(serializedData[HeaderByteSize..]);
        Serializer.WritePrimitive(messageId).CopyTo(serializedData);
        return Send(serializedData);
    }

    /// <summary>
    ///     Send data to the client
    /// </summary>
    /// <param name="packetData">Memory buffer</param>
    /// <returns>true if the operation succeeded, false in all other cases</returns>
    private bool Send(Memory<byte> packetData)
    {
        var headerLength = 2;
        if (IsSequenced)
        {
            headerLength += 2;
        }

        // TODO: Send UGSS messages that are split over RGSS
        while (packetData.Length > 0)
        {
            var length = Math.Min(packetData.Length + headerLength, MaxPacketSize);

            var t = new Memory<byte>(new byte[length]);
            packetData[.. (length - headerLength)].CopyTo(t[headerLength..]);

            if (IsSequenced)
            {
                if (IsReliable)
                {
                    _logger.Verbose("<- {0} SeqNum =  {1}", Type, CurrentSequenceNumber);
                }

                Serializer.WritePrimitive(Utils.SimpleFixEndianness(CurrentSequenceNumber)).CopyTo(t.Slice(2, 2));
                unchecked
                {
                    CurrentSequenceNumber++;
                }
            }

            var header = new GamePacketHeader(Type, 0, packetData.Length + headerLength > MaxPacketSize, (ushort)t.Length);
            var headerData = Serializer.WritePrimitive(Utils.SimpleFixEndianness(header.PacketHeader));
            headerData.CopyTo(t);

            _outgoingPackets.Enqueue(t);

            packetData = packetData[(length - headerLength) ..];
        }

        return true;
    }
}