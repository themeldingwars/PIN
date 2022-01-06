using GameServer.Controllers;
using GameServer.Test;
using Shared.Udp;
using System;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks.Dataflow;

namespace GameServer;

internal class GameServer : PacketServer
{
    private const double GameTickRate = 1.0 / 60.0;
    private const int MinPlayersPerShard = 16;
    private const int MaxPlayersPerShard = 64;

    private readonly ConcurrentDictionary<uint, INetworkPlayer> _clientMap;
    private readonly ConcurrentDictionary<ulong, IShard> _shards;

    private readonly ulong _serverId;

    private byte _nextShardId;

    public GameServer(ushort port, ulong serverID) : base(port)
    {
        _clientMap = new ConcurrentDictionary<uint, INetworkPlayer>();
        _shards = new ConcurrentDictionary<ulong, IShard>();

        _nextShardId = 1;
        _serverId = serverID;
    }

    /// <summary>
    ///     Generate the Server Id
    ///     TODO: Incorporate the Sql Node Number as per https://gist.github.com/SilentCLD/881839a9f45578f1618db012fc789a71
    /// </summary>
    /// <returns></returns>
    public static ulong GenerateRandomServerId()
    {
        Span<byte> ranSpan = stackalloc byte[8];
        new Random().NextBytes(ranSpan.Slice(2, 6));
        var serverId = BinaryPrimitives.ReadUInt64LittleEndian(ranSpan);
        return serverId;
    }

    protected override void Startup(CancellationToken ct)
    {
        DataUtils.Init();
        Factory.Init();
    }

    protected override async void ServerRunThreadAsync(CancellationToken ct)
    {
        Packet? p;
        while ((p = await _incomingPackets.ReceiveAsync(ct)) != null)
        {
            HandlePacket(p.Value, ct);
        }
    }

    protected override void HandlePacket(Packet packet, CancellationToken ct)
    {
        //Program.Logger.Information("[GAME] {0} sent {1} bytes.", packet.RemoteEndpoint, packet.PacketData.Length);
        //Program.Logger.Verbose(">  {0}", BitConverter.ToString(packet.PacketData.ToArray()).Replace("-", " "));

        var socketID = Utils.SimpleFixEndianness(packet.Read<uint>());
        INetworkClient client;

        if (!_clientMap.ContainsKey(socketID))
        {
            var c = new NetworkPlayer(packet.RemoteEndpoint, socketID);

            client = _clientMap.AddOrUpdate(socketID, c, (id, nc) => nc);
            _ = GetNextShard(ct).MigrateIn((INetworkPlayer)client);
        }
        else
        {
            client = _clientMap[socketID];
        }

        client.HandlePacket(packet.PacketData[4..], packet);
    }

    private IShard GetNextShard(CancellationToken ct)
    {
        foreach (var s in _shards.Values.OrderBy(s => s.CurrentPlayers))
        {
            if (s.CurrentPlayers < MaxPlayersPerShard)
            {
                return s;
            }
        }

        return NewShard(ct);
    }

    private IShard NewShard(CancellationToken ct)
    {
        var id = _serverId | (ulong)(_nextShardId++ << 8);
        var s = _shards.AddOrUpdate(id, new Shard(GameTickRate, id, this), (id, old) => old);

        s.Run(ct);

        return s;
    }

    private delegate void SendPacketDelegate<T>(T pkt, IPEndPoint ep)
        where T : struct;

    //protected bool Tick( double deltaTime, ulong currTime, CancellationToken ct ) {
    //	foreach( var s in Shards.Values ) {
    //		if( !s.Tick( deltaTime, currTime ) || s.CurrentPlayers < MinPlayersPerShard ) {
    //			// TODO: Shutdown Shard
    //		}
    //	}

    //	return true;
    //}
}