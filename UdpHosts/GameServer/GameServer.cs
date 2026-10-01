using System;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GameServer.Controllers;
using GameServer.GRPC;
using GameServer.StaticDB;
using GameServer.Test;
using Serilog;
using Shared.Udp;
using SDB = FauFau.Formats.StaticDB;

namespace GameServer;

internal class GameServer : PacketServer, IShardManager
{
    private const double _gameTickRate = 1.0 / 60.0;

    private readonly ConcurrentDictionary<uint, INetworkPlayer> _clientMap;
    private readonly ConcurrentDictionary<uint, IShard> _shardsByZone;
    private readonly Lock _shardCreationLock = new();

    private readonly ulong _serverId;
    private readonly GameServerSettings  _settings;

    private byte _nextShardIndex = 1;
    private CancellationToken _shardCancellation;
    private bool _isReady;

    public GameServer(GameServerSettings serverSettings,
                      ILogger logger,
                      SDB sdb)
        : base(serverSettings.Port, logger)
    {
        _clientMap = new ConcurrentDictionary<uint, INetworkPlayer>();
        _shardsByZone = new ConcurrentDictionary<uint, IShard>();

        _serverId = GenerateServerId();

        _settings = serverSettings;
        HealthUrl = serverSettings.HealthUrl;

        Logger.Information("Serving client {Environment}/{Branch} version {ClientVersion}: GSS protocol {GssVersion}, Matrix protocol {MatrixVersion}", serverSettings.ClientEnvironment, serverSettings.ClientBranch, serverSettings.ClientVersion, serverSettings.GssProtocolVersion, serverSettings.MatrixProtocolVersion);

        Logger.ForContext<SDBInterface>().Information("Reading from SDB");
        SDBInterface.Init(sdb);

        Logger.ForContext<SDBInterface>().Information("Reading custom data");
        CustomDBInterface.Init();

        Logger.ForContext(typeof(GRPCService)).Information("Initializing GRPC");
        GRPCService.Init(serverSettings.GrpcChannelAddress);
    }

    public IShard DefaultShard => _shardsByZone[_settings.ZoneId];

    public IEnumerable<IShard> Shards => _shardsByZone.Values;

    public IShard GetOrCreateShard(uint zoneId)
    {
        if (_shardsByZone.TryGetValue(zoneId, out var shard))
        {
            return shard;
        }

        // Creating a shard loads the zone's collision, which can take a while, so only one gets built at a time
        lock (_shardCreationLock)
        {
            if (_shardsByZone.TryGetValue(zoneId, out shard))
            {
                return shard;
            }

            var shardId = _serverId | ((ulong)_nextShardIndex++ << 8) | (byte)GuidService.AdditionalTypes.Instance;
            Logger.Information("Creating shard {ShardId:X16} for zone {ZoneId}", shardId, zoneId);

            shard = new Shard(_gameTickRate, shardId, zoneId, _settings, this, Logger);
            shard.Run(_shardCancellation);
            _shardsByZone[zoneId] = shard;

            return shard;
        }
    }

    protected override bool IsReady => _isReady;

    protected override void Startup(CancellationToken ct)
    {
        DataUtils.Init();
        Factory.Init();

        _shardCancellation = ct;
        GetOrCreateShard(_settings.ZoneId);

        foreach (var zoneId in ParsePreloadZones(_settings.PreloadZones))
        {
            GetOrCreateShard(zoneId);
        }

        if (_settings.GrpcChannelAddress != string.Empty)
        {
            _ = ListenGrpcAsync(ct);
        }

        _isReady = true;
        Logger.Information("Server is ready to accept connections.");
    }

    protected override void HandlePacket(Packet packet, CancellationToken ct)
    {
        if (Logger.IsEnabled(Serilog.Events.LogEventLevel.Verbose))
        {
            Logger.Verbose("[GAME] {RemoteEndpoint} sent {PacketLength} bytes.", packet.RemoteEndpoint, packet.PacketData.Length);
            Logger.Verbose(">  {PacketData}", BitConverter.ToString(packet.PacketData.ToArray()).Replace("-", " "));
        }

        var client = RetrieveClient(packet);
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

    private IEnumerable<uint> ParsePreloadZones(string preloadZones)
    {
        foreach (var part in preloadZones.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (uint.TryParse(part, out var zoneId) && DataUtils.TryGetZone(zoneId, out _))
            {
                yield return zoneId;
            }
            else
            {
                Logger.Warning("Ignoring unknown preload zone {Zone}", part);
            }
        }
    }

    private INetworkClient RetrieveClient(Packet packet)
    {
        var socketId = Utils.SimpleFixEndianness(packet.Read<uint>());
        INetworkClient client;

        if (!_clientMap.ContainsKey(socketId))
        {
            var newClient = new NetworkPlayer(packet.RemoteEndpoint, socketId, this, Logger);

            if (!_isReady)
            {
                var rejected = new NetworkClient(packet.RemoteEndpoint, socketId, Logger);
                rejected.NetClientStatus = ClientStatus.Aborted;
                Logger.Information("Rejected connection from {Endpoint} — server not ready.", packet.RemoteEndpoint);
                return rejected;
            }

            // New connections join the default shard, and move to the shard of their character's zone when they log in
            client = _clientMap.AddOrUpdate(socketId, newClient, (_, nc) => nc);
            DefaultShard.MigrateIn((INetworkPlayer)client);
        }
        else
        {
            client = _clientMap[socketId];
        }

        return client;
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
                Logger.ForContext(typeof(GRPCService)).Error("Failed to establish GRPC stream, retrying in 30 seconds");
                await Task.Delay(TimeSpan.FromSeconds(30), ct);
            }
        }
    }
}