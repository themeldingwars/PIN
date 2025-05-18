using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using AeroMessages.Control;
using AeroMessages.GSS.V66.Generic;
using AeroMessages.Matrix.V25;
using GameServer.Controllers;
using GameServer.Controllers.Character;
using GameServer.Entities;
using GameServer.Enums;
using GameServer.Extensions;
using GameServer.Packets;
using Serilog;
using Shared.Udp;

namespace GameServer;

public class NetworkClient : INetworkClient
{
    protected readonly ILogger Logger;

    protected NetworkClient(IPEndPoint endPoint, uint socketId, ILogger logger)
    {
        Logger = logger;
        SocketId = socketId;
        RemoteEndpoint = endPoint;
        NetClientStatus = ClientStatus.Unknown;
        NetLastActive = DateTime.Now;
    }

    public ClientStatus NetClientStatus { get; private set; }
    public uint SocketId { get; }
    public IPEndPoint RemoteEndpoint { get; }
    public DateTime NetLastActive { get; private set; }
    public ImmutableDictionary<ChannelType, Channel> NetChannels { get; private set; }
    public IShard AssignedShard { get; private set; }
    public ConcurrentQueue<Memory<byte>> SequencedMessages { get; private set; }

    protected IPacketSender Sender { get; private set; }
    protected IPlayer Player { get; private set; }

    public void Init(IPlayer player, IShard shard, IPacketSender sender)
    {
        Player = player;
        Sender = sender;
        NetClientStatus = ClientStatus.Connecting;
        AssignedShard = shard;
        SequencedMessages = new();

        NetChannels = Channel.GetChannels(this, Logger).ToImmutableDictionary();
        NetChannels[ChannelType.Control].PacketAvailable += Control_PacketAvailable;
        NetChannels[ChannelType.Matrix].PacketAvailable += Matrix_PacketAvailable;
        NetChannels[ChannelType.ReliableGss].PacketAvailable += GSS_PacketAvailable;
        NetChannels[ChannelType.UnreliableGss].PacketAvailable += GSS_PacketAvailable;
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
        var headerSize = Unsafe.SizeOf<GamePacketHeader>();
        while (index + 2 < data.Length)
        {
            var header = Deserializer.ReadStruct<GamePacketHeader>(data.Slice(index, 2).ToArray().Reverse().ToArray().AsMemory());

            if (header.Length == 0 || data.Length < header.Length + index)
            {
                break;
            }

            var gamePacket = new GamePacket(header, data.Slice(index + headerSize, header.Length - headerSize), packet.Received);

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

        NetLastActive = DateTime.Now;

        var t = new Memory<byte>(new byte[4 + packet.Length]);
        packet.CopyTo(t[4..]);
        Serializer.WriteStruct(Utils.SimpleFixEndianness(SocketId)).CopyTo(t);

        Sender.SendAsync(t, RemoteEndpoint);
    }

    public void SendAck(ChannelType forChannel, ushort forSequenceNumber, DateTime? received = null)
    {
        if (received != null)
        {
            Logger.Verbose("<-- {0} Ack for {1} on {2} after {3}ms.", ChannelType.Control, forSequenceNumber, forChannel, (DateTime.Now - received.Value).TotalMilliseconds);
        }
        else
        {
            Logger.Verbose("<-- {0} Ack for {1} on {2}.", ChannelType.Control, forSequenceNumber, forChannel);
        }

        var forNum = Utils.SimpleFixEndianness(forSequenceNumber);
        var nextNum = Utils.SimpleFixEndianness(unchecked((ushort)(forSequenceNumber + 1)));

        if (forChannel == ChannelType.Matrix)
        {
            NetChannels[ChannelType.Control].SendMessage(new MatrixAck { AckForNum = forNum, NextSeqNum = nextNum });
        }
        else if (forChannel == ChannelType.ReliableGss)
        {
            NetChannels[ChannelType.Control].SendMessage(new GSSAck { AckForNum = forNum, NextSeqNum = nextNum });
        }
    }

    private void GSS_PacketAvailable(GamePacket packet)
    {
        var controllerId = packet.Read<Enums.GSS.Controllers>();
        Span<byte> entity = stackalloc byte[8];
        packet.Read(7).ToArray().CopyTo(entity);
        var entityId = BitConverter.ToUInt64(entity) << 8;
        var messageId = packet.Read<byte>();

        var connection = Factory.Get(controllerId);

        if (connection == null)
        {
            Logger.Verbose("---> Unrecognized ControllerId for GSS Packet; Controller = {0} Entity = 0x{1:X16} MsgID = {2}!", controllerId, entityId, messageId);
            Logger.Warning(">  {0}", BitConverter.ToString(packet.PacketData.ToArray()).Replace("-", " "));
            return;
        }

        Logger.Verbose("--> {0}: Controller = {1} Entity = 0x{2:X16} MsgID = {3}", packet.Header.Channel, controllerId, entityId, messageId);
        connection.HandlePacket(this, Player, entityId, messageId, packet, Logger);
    }

    private void Matrix_PacketAvailable(GamePacket packet)
    {
        var messageId = packet.Read<MatrixPacketType>();
        Logger.Verbose("--> {0}: MsgID = {1} ({2})", ChannelType.Matrix, messageId, (byte)messageId);

        switch (messageId)
        {
            case MatrixPacketType.Login:
                var aeroLogin = packet.Unpack<Login>();
                Player.Login(aeroLogin.CharacterGuid);
                break;
            case MatrixPacketType.EnterZoneAck:
                Factory.Get<BaseController>().Init(this, Player, AssignedShard, Logger);
                Player.EnterZoneAck();
                break;
            case MatrixPacketType.ExitZoneAck:
                Player.ExitZoneAck();
                break;
            case MatrixPacketType.KeyframeRequest:
                var query = packet.Unpack<KeyframeRequest>();
                Logger.Verbose($"KeyframeRequest with {query.EntityRequests?.Length ?? 0} entity requests and {query.RefRequests?.Length ?? 0} ref requests. Total scoped for player: {AssignedShard.EntityMan.GetNumberOfScopedEntities(Player)}");
                foreach (var request in query.EntityRequests)
                {
                    Enums.GSS.Controllers typecode = (Enums.GSS.Controllers)(request.Entity & 0x00000000000000FFul);
                    AssignedShard.Entities.TryGetValue(request.Entity & 0xffffffffffffff00, out IEntity entity);
                    if (entity != null)
                    {
                        AssignedShard.EntityMan.KeyframeRequest(this, Player, entity, typecode, request.Checksum);
                    }
                    else
                    {
                        Console.WriteLine($"KeyframeRequest failed to find {request.Entity} ({typecode})");
                    }
                }

                break;
            case MatrixPacketType.ClientStatus:
                NetChannels[ChannelType.Matrix].SendMessage(new MatrixStatus
                {
                    MatrixBytesPerSecond = 0,
                    GameShapedBytes = 0,
                    PacketUploss = 0,
                    PacketDownloss = 0,
                    Unk5 = 0,
                    IsEverlastingGobsocket = 0,
                    HaveUnk7 = 0,
                    Unk8 = Array.Empty<byte>()
                });
                break;
            case MatrixPacketType.LogInstrumentation:
                // Ignore
                break;
            default:
                Logger.Error("---> Unrecognized Matrix Packet {0}[{1}]!!!", messageId, (byte)messageId);
                Logger.Warning(">  {0}", BitConverter.ToString(packet.PacketData.ToArray()).Replace("-", " "));
                break;
        }
    }

    private void Control_PacketAvailable(GamePacket packet)
    {
        var messageId = packet.Read<ControlPacketType>();
        Logger.Verbose("--> {0}: MsgID = {1} ({2})", ChannelType.Control, messageId, (byte)messageId);

        switch (messageId)
        {
            case ControlPacketType.CloseConnection:
                Logger.Information("RECEIVED CloseConnection");
                NetClientStatus = ClientStatus.Disconnecting;
                AssignedShard.MigrateOut((INetworkPlayer)this);
                break;
            case ControlPacketType.MatrixAck:
                // TODO: Track reliable packets
                var matrixAckPackage = packet.Unpack<MatrixAck>();
                Logger.Verbose("--> {0} Ack for {1} on {2}.", ChannelType.Control, Utils.SimpleFixEndianness(matrixAckPackage.AckForNum), ChannelType.Matrix);
                break;
            case ControlPacketType.ReliableGSSAck:
                // TODO: Track reliable packets
                var reliableGssAckPackage = packet.Unpack<GSSAck>();
                Logger.Verbose("--> {0} Ack for {1} on {2}.", ChannelType.Control, Utils.SimpleFixEndianness(reliableGssAckPackage.AckForNum), ChannelType.ReliableGss);
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
                Logger.Error("---> Unrecognized Control Packet {0} ({1:X2})!!!", messageId, (byte)messageId);
                Logger.Warning(">  {0}", BitConverter.ToString(packet.PacketData.ToArray()).Replace("-", " "));
                break;
        }
    }

    private List<string> SplitConsoleMessage(string input)
    {
        List<string> result = new List<string>();

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