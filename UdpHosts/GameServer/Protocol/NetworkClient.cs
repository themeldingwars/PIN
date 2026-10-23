using System;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Net;
using System.Threading;
using AeroMessages.Control;
using AeroMessages.GSS.Generic;
using GameServer.Enums;
using GameServer.Extensions;
using GameServer.Handlers;
using Serilog;
using Serilog.Events;
using Shared.Udp;

namespace GameServer.Protocol;

public class NetworkClient : INetworkClient
{
    protected readonly ILogger Logger;

    // UDP + IP overhead, matching the budget used by Channel. A datagram payload is the
    // socket id plus zero or more game-message sub-packets; keep the whole thing under MTU.
    private const int _protocolHeaderSize = 80;
    private const int _socketIdSize = 4;
    private const int _maxDatagramPayload = PacketServer.MTU - _protocolHeaderSize;

    private readonly List<Memory<byte>> _outgoingBatch = [];
    private bool _batchOutgoing;

    protected internal NetworkClient(IPEndPoint endPoint, uint socketId, ILogger logger)
    {
        Logger = logger;
        SocketId = socketId;
        RemoteEndpoint = endPoint;
        NetClientStatus = ClientStatus.Unknown;
        NetLastActive = DateTime.Now;
    }

    public ClientStatus NetClientStatus { get; internal set; }
    public uint SocketId { get; }
    public IPEndPoint RemoteEndpoint { get; }
    public DateTime NetLastActive { get; private set; }
    public ImmutableDictionary<ChannelType, Channel> NetChannels { get; private set; }
    public IShard AssignedShard { get; private set; }
    public ConcurrentQueue<Memory<byte>> SequencedMessages { get; private set; }
    public IPlayer Player { get; private set; }

    protected IPacketSender Sender { get; private set; }

    public void Init(IPlayer player, IShard shard, IPacketSender sender)
    {
        Player = player;
        Sender = sender;
        NetClientStatus = ClientStatus.Connecting;
        AssignedShard = shard;
        _batchOutgoing = shard.Settings.BatchOutgoingPackets;
        SequencedMessages = new();

        NetChannels = Channel.GetChannels(this, Logger, AssignedShard.Settings.GssProtocolVersion, AssignedShard.Settings.MatrixProtocolVersion).ToImmutableDictionary();

        NetChannels[ChannelType.Control].PacketAvailable += Control_PacketAvailable;
        NetChannels[ChannelType.Matrix].PacketAvailable += packet => MatrixRouter.Handle(packet, this, AssignedShard);
        NetChannels[ChannelType.ReliableGss].PacketAvailable += packet => GssRouter.Handle(packet, this, AssignedShard);
        NetChannels[ChannelType.UnreliableGss].PacketAvailable += packet => GssRouter.Handle(packet, this, AssignedShard);
    }

    public void AssignShard(IShard shard)
    {
        AssignedShard = shard;
    }

    public void HandlePacket(ReadOnlyMemory<byte> data, Packet packet)
    {
        if (NetClientStatus == ClientStatus.Connecting)
        {
            NetClientStatus = ClientStatus.Connected; // the connection must have been established in order to receive a packet, so we must now be connected
        }

        if (NetClientStatus != ClientStatus.Connected && NetClientStatus != ClientStatus.Idle)
        {
            return; // can't do anything if we're not ready yet!
        }

        var index = 0;
        while (index + GamePacketHeader.Size <= data.Length)
        {
            var header = GamePacketHeader.Read(data.Span.Slice(index, GamePacketHeader.Size));

            if (header.Length == 0 || data.Length < header.Length + index)
            {
                break;
            }

            var gamePacket = new GamePacket(header, data.Slice(index + GamePacketHeader.Size, header.Length - GamePacketHeader.Size), packet.Received);

            // Logger.Verbose("-> {0} = R:{1} S:{2} L:{3}", header.Channel, header.ResendCount, header.IsSplit, header.Length);
            NetChannels[header.Channel].HandlePacket(gamePacket);

            index += header.Length;
        }

        NetLastActive = DateTime.Now;
    }

    public virtual void NetworkTick(double deltaTime, ulong currentTime, CancellationToken ct)
    {
        while (SequencedMessages.TryDequeue(out var qi))
        {
            Send(qi);
        }

        foreach (var channel in NetChannels.Values)
        {
            channel.Process(ct);
        }

        FlushOutgoing();
    }

    public void SendDebugChat(string message)
    {
        if (AssignedShard != null)
        {
            AssignedShard.Chat.SendToPlayer(message, ChatChannel.Debug, this);
        }
    }

    public void SendDebugLog(string message)
    {
        if (AssignedShard != null)
        {
            if (message.Length > 200)
            {
                List<string> splits = SplitConsoleMessage(message);
                foreach (var split in splits)
                {
                    var response = new TempConsoleMessage()
                    {
                        ConsoleNoticeMessage = split,
                        ConsoleCommand = string.Empty,
                        ChatNotification = string.Empty,
                        DebugReportArgType = string.Empty,
                        DebugReportArgData = string.Empty,
                    };
                    NetChannels[ChannelType.UnreliableGss].SendMessage(response, AssignedShard.InstanceId);
                }
            }
            else
            {
                var response = new TempConsoleMessage()
                {
                    ConsoleNoticeMessage = message,
                    ConsoleCommand = string.Empty,
                    ChatNotification = string.Empty,
                    DebugReportArgType = string.Empty,
                    DebugReportArgData = string.Empty,
                };
                NetChannels[ChannelType.UnreliableGss].SendMessage(response, AssignedShard.InstanceId);
            }
        }
    }

    public void Send(Memory<byte> packet)
    {
        if (NetClientStatus == ClientStatus.Disconnecting || NetClientStatus == ClientStatus.Aborted)
        {
            return;
        }

        if (_batchOutgoing)
        {
            _outgoingBatch.Add(packet);
            return;
        }

        SendDatagram(packet);
    }

    public void SendAck(ChannelType forChannel, ushort forSequenceNumber, DateTime? received = null)
    {
        ControlPacketType controlMessage;
        if (forChannel == ChannelType.Matrix)
        {
            controlMessage = ControlPacketType.MatrixAck;
        }
        else if (forChannel == ChannelType.ReliableGss)
        {
            controlMessage = ControlPacketType.ReliableGSSAck;
        }
        else
        {
            return;
        }

        if (Logger.IsEnabled(LogEventLevel.Verbose))
        {
            if (received != null)
            {
                Logger.Verbose("<-- {Channel} Ack for {SeqNum} on {ForChannel} after {ElapsedTime}ms.", ChannelType.Control, forSequenceNumber, forChannel, (DateTime.Now - received.Value).TotalMilliseconds);
            }
            else
            {
                Logger.Verbose("<-- {Channel} Ack for {SeqNum} on {ForChannel}.", ChannelType.Control, forSequenceNumber, forChannel);
            }
        }

        // MatrixAck/GSSAck is NextSeqNum followed by AckForNum, both in network byte order, with no additional acks
        Span<byte> messageData = stackalloc byte[4];
        unchecked
        {
            BinaryPrimitives.WriteUInt16BigEndian(messageData[..2], (ushort)(forSequenceNumber + 1));
        }

        BinaryPrimitives.WriteUInt16BigEndian(messageData[2..], forSequenceNumber);
        NetChannels[ChannelType.Control].SendControlMessage((byte)controlMessage, messageData);
    }

    private void Control_PacketAvailable(GamePacket packet)
    {
        if (packet.BytesRemaining < 1)
        {
            Logger.Warning("---> Truncated Control packet: {PacketBytes} bytes", packet.TotalBytes);
            return;
        }

        var messageId = (ControlPacketType)packet.ReadByte();
        if (Logger.IsEnabled(LogEventLevel.Verbose))
        {
            Logger.Verbose("--> {Channel}: MsgID = {Message} ({MessageId})", ChannelType.Control, messageId, (byte)messageId);
        }

        switch (messageId)
        {
            case ControlPacketType.CloseConnection:
                Logger.Information("RECEIVED CloseConnection");
                NetClientStatus = ClientStatus.Disconnecting;
                AssignedShard.MigrateOut((INetworkPlayer)this);
                break;
            case ControlPacketType.MatrixAck:
                // TODO: Track reliable packets we sent, detect misses and resend
                LogAck(packet, ChannelType.Matrix);
                break;
            case ControlPacketType.ReliableGSSAck:
                // TODO: Track reliable packets we sent, detect misses and resend
                LogAck(packet, ChannelType.ReliableGss);
                break;
            case ControlPacketType.TimeSyncRequest:
                var timeSyncRequestPackage = packet.Unpack<TimeSyncRequest>();
                NetChannels[ChannelType.Control].SendMessage(new TimeSyncResponse()
                {
                    ClientTime = timeSyncRequestPackage.ClientTime,
                    ServerTime = unchecked(AssignedShard.CurrentTimeLong * 1000)
                });
                break;
            case ControlPacketType.MTUProbe:
                // TODO: ???
                break;
            default:
                Logger.Warning("---> Unrecognized Control Packet {Message} ({MessageId:X2})!!!", messageId, (byte)messageId);
                Logger.Warning(">  {PacketData}", BitConverter.ToString(packet.PacketData.ToArray()).Replace("-", " "));
                break;
        }
    }

    /// <summary>
    ///     Read the acknowledged sequence number of an incoming ack without unpacking the whole Aero message
    /// </summary>
    /// <param name="packet">The received control packet carrying the ack</param>
    /// <param name="forChannel">The channel the ack acknowledges</param>
    private void LogAck(GamePacket packet, ChannelType forChannel)
    {
        if (!Logger.IsEnabled(LogEventLevel.Verbose) || packet.BytesRemaining < 4)
        {
            return;
        }

        // AckForNum is the second field of the message and travels in network byte order
        var ackForNum = BinaryPrimitives.ReadUInt16BigEndian(packet.PacketData.Span[packet.CurrentPosition..][2..]);
        Logger.Verbose("--> {Channel} Ack for {AckForNum} on {ForChannel}.", ChannelType.Control, ackForNum, forChannel);
    }

    /// <summary>
    ///     Send a single game-message sub-packet as its own UDP datagram (socket id + sub-packet).
    ///     Used when <see cref="GameServerSettings.BatchOutgoingPackets" /> is disabled.
    /// </summary>
    /// <param name="packet">One framed game sub-packet: channel header, sequence number on sequenced channels, then the message body</param>
    private void SendDatagram(Memory<byte> packet)
    {
        NetLastActive = DateTime.Now;

        var buffer = new byte[_socketIdSize + packet.Length];
        BinaryPrimitives.WriteUInt32BigEndian(buffer, SocketId);
        packet.Span.CopyTo(buffer.AsSpan(_socketIdSize));

        Sender.SendAsync(buffer, RemoteEndpoint);
    }

    /// <summary>
    ///     Flush any accumulated game-message sub-packets to the client, packing as many as fit
    ///     into each UDP datagram (socket id + sub-packets, capped at the MTU budget). Called once
    ///     at the end of <see cref="NetworkTick" />; all sub-packets were enqueued on the same thread.
    /// </summary>
    private void FlushOutgoing()
    {
        if (_outgoingBatch.Count == 0)
        {
            return;
        }

        NetLastActive = DateTime.Now;

        var datagram = new byte[_maxDatagramPayload];
        BinaryPrimitives.WriteUInt32BigEndian(datagram, SocketId);
        var currentSize = _socketIdSize;

        foreach (var packet in _outgoingBatch)
        {
            if (currentSize + packet.Length > _maxDatagramPayload)
            {
                Sender.SendAsync(datagram.AsMemory(0, currentSize), RemoteEndpoint);

                datagram = new byte[_maxDatagramPayload];
                BinaryPrimitives.WriteUInt32BigEndian(datagram, SocketId);
                currentSize = _socketIdSize;
            }

            packet.Span.CopyTo(datagram.AsSpan(currentSize));
            currentSize += packet.Length;
        }

        Sender.SendAsync(datagram.AsMemory(0, currentSize), RemoteEndpoint);

        _outgoingBatch.Clear();
    }

    private List<string> SplitConsoleMessage(string input)
    {
        List<string> result = [];

        int index = 0;

        while (index < input.Length)
        {
            int endIndex = index + 700;
            if (endIndex >= input.Length)
            {
                endIndex = input.Length;
            }
            else
            {
                endIndex = FindSplitIndex(input, endIndex);
            }

            result.Add($"\n\n{input.Substring(index, endIndex - index).Trim()}");
            index = endIndex;
        }

        return result;
    }

    private int FindSplitIndex(string input, int endIndex)
    {
        int splitIndex = endIndex;
        while (splitIndex > endIndex - 745 && splitIndex > 0)
        {
            if (input[splitIndex] == '\n')
            {
                return splitIndex + 1;
            }

            splitIndex--;
        }

        return endIndex;
    }
}