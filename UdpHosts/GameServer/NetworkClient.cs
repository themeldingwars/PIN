using AeroMessages.Matrix.V25;
using GameServer.Controllers;
using GameServer.Controllers.Character;
using GameServer.Enums;
using GameServer.Extensions;
using GameServer.Packets;
using GameServer.Packets.Control;
using Serilog;
using Shared.Udp;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;

namespace GameServer;

public class NetworkClient : INetworkClient
{
    protected readonly ILogger Logger;

    protected NetworkClient(IPEndPoint endPoint, uint socketId, ILogger logger)
    {
        Logger = logger;
        SocketId = socketId;
        RemoteEndpoint = endPoint;
        NetClientStatus = Status.Unknown;
        NetLastActive = DateTime.Now;
    }

    protected IPacketSender Sender { get; private set; }
    protected IPlayer Player { get; private set; }

    public Status NetClientStatus { get; private set; }
    public uint SocketId { get; }
    public IPEndPoint RemoteEndpoint { get; }
    public DateTime NetLastActive { get; private set; }
    public ImmutableDictionary<ChannelType, Channel> NetChannels { get; private set; }
    public IShard AssignedShard { get; private set; }

    public void Init(IPlayer player, IShard shard, IPacketSender sender)
    {
        Player = player;
        Sender = sender;
        NetClientStatus = Status.Connecting;
        AssignedShard = shard;

        NetChannels = Channel.GetChannels(this, Logger).ToImmutableDictionary();
        NetChannels[ChannelType.Control].PacketAvailable += Control_PacketAvailable;
        NetChannels[ChannelType.Matrix].PacketAvailable += Matrix_PacketAvailable;
        NetChannels[ChannelType.ReliableGss].PacketAvailable += GSS_PacketAvailable;
        NetChannels[ChannelType.UnreliableGss].PacketAvailable += GSS_PacketAvailable;
    }

    public void HandlePacket(ReadOnlyMemory<byte> data, Packet packet)
    {
        if (NetClientStatus == Status.Connecting)
        {
            NetClientStatus = Status.Connected; // the connection must have been established in order to receive a packet, so we must now be connected
        }

        if (NetClientStatus != Status.Connected && NetClientStatus != Status.Idle)
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

            //Program.Logger.Verbose("-> {0} = R:{1} S:{2} L:{3}", hdr.Channel, hdr.ResendCount, hdr.IsSplit, hdr.Length);

            NetChannels[header.Channel].HandlePacket(gamePacket);

            index += header.Length;
        }

        NetLastActive = DateTime.Now;
    }

    public virtual void NetworkTick(double deltaTime, ulong currentTime, CancellationToken ct)
    {
        foreach (var channel in NetChannels.Values)
        {
            channel.Process(ct);
        }
    }

    public void Send(Memory<byte> packet)
    {
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
            NetChannels[ChannelType.Control].SendClass(new MatrixAck { AckFor = forNum, NextSeqNum = nextNum });
        }
        else if (forChannel == ChannelType.ReliableGss)
        {
            NetChannels[ChannelType.Control].SendClass(new ReliableGSSAck { AckFor = forNum, NextSeqNum = nextNum });
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
                break;
            case MatrixPacketType.KeyframeRequest:
                packet.Unpack<KeyframeRequest>();
                // TODO: Send checksums/keyframes in return.
                break;
            case MatrixPacketType.ClientStatus:
                NetChannels[ChannelType.Matrix].SendIAero(new MatrixStatus
                                                          {
                                                              Unk1 = 0,
                                                              Unk2 = 0,
                                                              Unk3 = 0,
                                                              Unk4 = 0,
                                                              Unk5 = 0,
                                                              Unk6 = 0,
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
                packet.Read<CloseConnection>();
                // TODO: Cleanly dispose of client
                break;
            case ControlPacketType.MatrixAck:
                var matrixAckPackage = packet.Read<MatrixAck>();
                Logger.Verbose("--> {0} Ack for {1} on {2}.", ChannelType.Control, Utils.SimpleFixEndianness(matrixAckPackage.AckFor), ChannelType.Matrix);
                // TODO: Track reliable packets
                break;
            case ControlPacketType.ReliableGSSAck:
                var reliableGssAckPackage = packet.Read<ReliableGSSAck>();
                Logger.Verbose("--> {0} Ack for {1} on {2}.", ChannelType.Control, Utils.SimpleFixEndianness(reliableGssAckPackage.AckFor), ChannelType.ReliableGss);
                // TODO: Track reliable packets
                break;
            case ControlPacketType.TimeSyncRequest:
                var timeSyncRequestPackage = packet.Read<TimeSyncRequest>();

                NetChannels[ChannelType.Control].Send(new TimeSyncResponse(timeSyncRequestPackage.ClientTime, unchecked(AssignedShard.CurrentTimeLong * 1000)));
                break;
            case ControlPacketType.MTUProbe:
                packet.Read<MTUProbe>();
                // TODO: ???
                break;
            default:
                Logger.Error("---> Unrecognized Control Packet {0} ({1:X2})!!!", messageId, (byte)messageId);
                Logger.Warning(">  {0}", BitConverter.ToString(packet.PacketData.ToArray()).Replace("-", " "));
                break;
        }
    }
}