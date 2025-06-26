﻿using System;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using GameServer.Controllers;
using GameServer.Data.SDB;
using GameServer.GRPC;
using GameServer.Test;
using Serilog;
using Shared.Udp;
using SDB = FauFau.Formats.StaticDB;

namespace GameServer;

internal class GameServer : PacketServer
{
    private const int MinPlayersPerShard = 16;
    private const int MaxPlayersPerShard = 64;

    private readonly ConcurrentDictionary<uint, INetworkPlayer> _clientMap;
    private readonly ConcurrentDictionary<ulong, IShard> _shards;

    private readonly ulong _serverId;
    private readonly GameServerSettings _settings;
    private readonly IShardFactory _shardFactory;

    private byte _nextShardId;

    public GameServer(GameServerSettings serverSettings,
                      ILogger logger,
                      SDB sdb,
                      IShardFactory shardFactory)
        : base(serverSettings.Port, logger)
    {
        _clientMap = new ConcurrentDictionary<uint, INetworkPlayer>();
        _shards = new ConcurrentDictionary<ulong, IShard>();

        _nextShardId = 1;
        _serverId = GenerateServerId();

        _settings = serverSettings;
        _shardFactory = shardFactory;

        Logger.Information("Reading from SDB");
        SDBInterface.Init(sdb);

        Logger.Information("Reading custom data");
        CustomDBInterface.Init();

        Logger.Information("Initializing GRPC");
        GRPCService.Init(serverSettings.GrpcChannelAddress);
    }

    protected override void Startup(CancellationToken ct)
    {
        DataUtils.Init();
        Factory.Init();
        NewShard(ct);

        _ = ListenGrpcAsync(ct);
    }

    protected override async void ServerRunThreadAsync(CancellationToken ct)
    {
        Packet? packet;
        while ((packet = await IncomingPackets.ReceiveAsync(ct)) != null)
        {
            HandlePacket(packet.Value, ct);
        }
    }

    protected override void HandlePacket(Packet packet, CancellationToken ct)
    {
        Logger.Verbose("[GAME] {0} sent {1} bytes.", packet.RemoteEndpoint, packet.PacketData.Length);
        Logger.Verbose(">  {0}", BitConverter.ToString(packet.PacketData.ToArray()).Replace("-", " "));

        var client = RetrieveClient(packet, ct);
        client.HandlePacket(packet.PacketData[4..], packet);
    }

    /// <summary>
    ///     Generate the Server Id
    ///     TODO: Incorporate the Sql Node Number as per https://gist.github.com/SilentCLD/881839a9f45578f1618db012fc789a71
    /// </summary>
    private static ulong GenerateServerId()
    {
        Span<byte> ranSpan = stackalloc byte[8];
        new Random().NextBytes(ranSpan.Slice(2, 6));
        return BinaryPrimitives.ReadUInt64LittleEndian(ranSpan);
    }

    private INetworkClient RetrieveClient(Packet packet, CancellationToken ct)
    {
        var socketId = Utils.SimpleFixEndianness(packet.Read<uint>());
        INetworkClient client;

        if (!_clientMap.ContainsKey(socketId))
        {
            var newClient = new NetworkPlayer(packet.RemoteEndpoint, socketId, Logger);

            client = _clientMap.AddOrUpdate(socketId, newClient, (_, nc) => nc);
            _ = GetNextShard(ct).MigrateIn((INetworkPlayer)client);
        }
        else
        {
            client = _clientMap[socketId];
        }

        return client;
    }

    private IShard GetNextShard(CancellationToken ct)
    {
        foreach (var shard in _shards.Values.OrderBy(s => s.CurrentPlayers))
        {
            if (shard.CurrentPlayers < MaxPlayersPerShard)
            {
                return shard;
            }
        }

        return NewShard(ct);
    }

    private IShard NewShard(CancellationToken ct)
    {
        var id = _serverId | (uint)(_nextShardId++ << 8) | (byte)Enums.GSS.Controllers.GenericShard;
        var shard = _shards.AddOrUpdate(id, _shardFactory.Create(id), (_, old) => old);

        shard.Run(ct);

        return shard;
    }

    private async Task ListenGrpcAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                await GRPCService.ListenAsync(_clientMap, ct);
            }
            catch (Exception)
            {
                Console.WriteLine("Failed to establish GRPC stream, retrying in 30 seconds");
                await Task.Delay(TimeSpan.FromSeconds(30), ct);
            }
        }
    }
}