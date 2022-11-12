﻿using Shared.Udp;
using System;
using System.Collections.Immutable;
using System.Net;
using System.Threading;

namespace GameServer;

public enum Status
{
    Unknown = 0,
    Connecting = 1,
    Connected,
    Idle,
    Disconnecting,
    Aborted
}

public interface INetworkClient
{
    Status NetClientStatus { get; }
    uint SocketID { get; }
    IPEndPoint RemoteEndpoint { get; }
    DateTime NetLastActive { get; }
    ImmutableDictionary<ChannelType, Channel> NetChans { get; }
    IShard AssignedShard { get; }

    void Init(IPlayer player, IShard shard, IPacketSender sender);
    void HandlePacket(ReadOnlyMemory<byte> data, Packet packet);
    void NetworkTick(double deltaTime, ulong currTime, CancellationToken ct);
    void Send(Memory<byte> p);
    void SendAck(ChannelType forChannel, ushort forSeqNumber, DateTime? received = null);
}