using GameServer.Controllers;
using GameServer.Test;
using Shared.Udp;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks.Dataflow;

namespace GameServer;

internal class GameServer : PacketServer
{
    public delegate void SendPacketDelegate<T>(T pkt, IPEndPoint ep)
        where T : struct;

    public const double GameTickRate = 1.0 / 60.0;
    public const int MinPlayersPerShard = 16;
    public const int MaxPlayersPerShard = 64;

    protected ConcurrentDictionary<uint, INetworkPlayer> ClientMap;
    protected ConcurrentDictionary<ulong, IShard> Shards;

    protected byte nextShardId;
    protected ulong ServerId;

    public GameServer(ushort port, ulong serverId) : base(port)
    {
        ClientMap = new ConcurrentDictionary<uint, INetworkPlayer>();
        Shards = new ConcurrentDictionary<ulong, IShard>();

        nextShardId = 1;
        ServerId = serverId;
    }

    protected override void Startup(CancellationToken ct)
    {
        DataUtils.Init();
        Factory.Init();
    }

    protected override async void ServerRunThreadAsync(CancellationToken ct)
    {
        Packet? packet;
        while ((packet = await incomingPackets.ReceiveAsync(ct)) != null)
        {
            HandlePacket(packet.Value, ct);
        }
    }

    protected IShard GetNextShard(CancellationToken ct)
    {
        foreach (var shard in Shards.Values.OrderBy(s => s.CurrentPlayers))
        {
            if (shard.CurrentPlayers < MaxPlayersPerShard)
            {
                return shard;
            }
        }

        return NewShard(ct);
    }

    protected IShard NewShard(CancellationToken ct)
    {
        var id = ServerId | (ulong)(nextShardId++ << 8);
        var shard = Shards.AddOrUpdate(id, new Shard(GameTickRate, id, this), (id, old) => old);

        shard.Run(ct);

        return shard;
    }

    //protected bool Tick( double deltaTime, ulong currTime, CancellationToken ct ) {
    //	foreach( var s in Shards.Values ) {
    //		if( !s.Tick( deltaTime, currTime ) || s.CurrentPlayers < MinPlayersPerShard ) {
    //			// TODO: Shutdown Shard
    //		}
    //	}

    //	return true;
    //}

    protected override void HandlePacket(Packet packet, CancellationToken ct)
    {
        //Program.Logger.Information("[GAME] {0} sent {1} bytes.", packet.RemoteEndpoint, packet.PacketData.Length);
        //Program.Logger.Verbose(">  {0}", BitConverter.ToString(packet.PacketData.ToArray()).Replace("-", " "));

        var socketId = Utils.SimpleFixEndianness(packet.Read<uint>());
        INetworkClient client;

        if (!ClientMap.ContainsKey(socketId))
        {
            var newClient = new NetworkPlayer(packet.RemoteEndpoint, socketId);

            client = ClientMap.AddOrUpdate(socketId, newClient, (id, nc) => { return nc; });
            _ = GetNextShard(ct).MigrateIn((INetworkPlayer)client);
        }
        else
        {
            client = ClientMap[socketId];
        }

        client.HandlePacket(packet.PacketData[4..], packet);
    }
}