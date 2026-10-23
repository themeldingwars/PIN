using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Net;
using System.Threading;
using GameServer.Protocol;
using Shared.Udp;

namespace GameServer;

public interface INetworkClient
{
    ClientStatus NetClientStatus { get; }
    uint SocketId { get; }
    IPEndPoint RemoteEndpoint { get; }
    DateTime NetLastActive { get; }
    ImmutableDictionary<ChannelType, Channel> NetChannels { get; }
    IShard AssignedShard { get; }
    ConcurrentQueue<Memory<byte>> SequencedMessages { get; }

    /// <summary>
    ///     The player this connection belongs to
    /// </summary>
    IPlayer Player { get; }

    void Init(IPlayer player, IShard shard, IPacketSender sender);
    void AssignShard(IShard shard);
    void HandlePacket(ReadOnlyMemory<byte> data, Packet packet);
    void NetworkTick(double deltaTime, ulong currentTime, CancellationToken ct);
    void Send(Memory<byte> packet);
    void SendAck(ChannelType forChannel, ushort forSequenceNumber, DateTime? received = null);

    void SendDebugChat(string message);
    void SendDebugLog(string log);
}
