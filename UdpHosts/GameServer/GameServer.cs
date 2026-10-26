using System;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FauFau.Formats;
using GameServer.GRPC;
using GameServer.Protocol;
using GameServer.StaticDB;
using GameServer.Test;
using Serilog;
using Shared.Udp;

namespace GameServer;

internal class GameServer : PacketServer, IShardManager
{
    private static readonly TimeSpan _gameTickRate = TimeSpan.FromSeconds(1.0 / 60.0);
    private const int _socketIdSize = 4;
    private static readonly TimeSpan _closedClientGrace = TimeSpan.FromSeconds(10);

    private readonly ConcurrentDictionary<uint, INetworkPlayer> _clientMap;
    private readonly ConcurrentDictionary<uint, IShard> _shardsByZone;
    private readonly Lock _shardCreationLock = new();

    private readonly ulong _serverId;
    private readonly GameServerSettings  _settings;

    private byte _nextShardIndex = 1;
    private CancellationToken _shardCancellation;
    private bool _isReady;

    public GameServer(GameServerSettings serverSettings,
                      ILogger logger)
        : base(serverSettings.Port, logger)
    {
        _clientMap = new ConcurrentDictionary<uint, INetworkPlayer>();
        _shardsByZone = new ConcurrentDictionary<uint, IShard>();

        _serverId = GenerateServerId();

        _settings = serverSettings;
        HealthUrl = serverSettings.HealthUrl;

        Logger.Information("Serving client {Environment}/{Branch} version {ClientVersion}: GSS protocol {GssVersion}, Matrix protocol {MatrixVersion}", serverSettings.ClientEnvironment, serverSettings.ClientBranch, serverSettings.ClientVersion, serverSettings.GssProtocolVersion, serverSettings.MatrixProtocolVersion);

        // The tables are copied into records, so the SDB is only needed while they load
        Logger.ForContext<SDBInterface>().Information("Reading from SDB {StaticDBPath}", serverSettings.StaticDBPath);
        using (var sdb = StaticDBView.Open(serverSettings.StaticDBPath))
        {
            SDBInterface.Init(sdb);
        }

        Logger.ForContext<SDBInterface>().Information("Reading custom data");
        CustomDBInterface.Init();

        Logger.ForContext(typeof(GRPCService)).Information("Initializing GRPC");
        GRPCService.Init(serverSettings.GrpcChannelAddress);
    }

    public IShard DefaultShard => _shardsByZone[_settings.ZoneId];

    public IEnumerable<IShard> Shards => _shardsByZone.Values;

    protected override bool IsReady => _isReady;

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

    protected override void Startup(CancellationToken ct)
    {
        DataUtils.Init();
        LoginService.Init(this);

        _shardCancellation = ct;
        GetOrCreateShard(_settings.ZoneId);

        foreach (var zoneId in ParsePreloadZones(_settings.PreloadZones))
        {
            GetOrCreateShard(zoneId);
        }

        _ = ForgetClosedClientsAsync(ct);

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

        if (packet.PacketData.Length < _socketIdSize)
        {
            Logger.Warning("[GAME] Ignoring runt datagram of {PacketLength} bytes from {RemoteEndpoint}.", packet.PacketData.Length, packet.RemoteEndpoint);
            return;
        }

        var client = RetrieveClient(packet);
        client?.HandlePacket(packet.PacketData[_socketIdSize..], packet);
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
        var socketId = BinaryPrimitives.ReadUInt32BigEndian(packet.PacketData.Span);

        if (_clientMap.TryGetValue(socketId, out var knownClient))
        {
            // Another client got the same socket id or someone guessed it, either way it isn't this connection
            if (!knownClient.RemoteEndpoint.Equals(packet.RemoteEndpoint))
            {
                Logger.Debug("Ignoring a datagram for socket {SocketId} from {RemoteEndpoint}, the socket belongs to {Owner}", socketId, packet.RemoteEndpoint, knownClient.RemoteEndpoint);
                return null;
            }

            return knownClient;
        }

        var newClient = new NetworkPlayer(packet.RemoteEndpoint, socketId, Logger);

        if (!_isReady)
        {
            var rejected = new NetworkClient(packet.RemoteEndpoint, socketId, Logger);
            rejected.NetClientStatus = ClientStatus.Aborted;
            Logger.Information("Rejected connection from {Endpoint} — server not ready.", packet.RemoteEndpoint);
            return rejected;
        }

        // New connections join the default shard, and move to the shard of their character's zone when they log in
        var client = _clientMap.GetOrAdd(socketId, newClient);
        if (ReferenceEquals(client, newClient))
        {
            DefaultShard.MigrateIn(client);
        }

        return client;
    }

    private async Task ForgetClosedClientsAsync(CancellationToken ct)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));
        try
        {
            while (await timer.WaitForNextTickAsync(ct))
            {
                var now = DateTime.UtcNow;
                foreach (var entry in _clientMap)
                {
                    if (entry.Value.NetClosedAt is { } closedAt && now - closedAt > _closedClientGrace)
                    {
                        _clientMap.TryRemove(entry);
                    }
                }
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
        }
    }

    private async Task ListenGrpcAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                await GRPCService.ListenAsync(_clientMap, ct);

                // RIN tears the stream down when a command hits a database error, which ends the read without throwing
                Logger.ForContext(typeof(GRPCService)).Warning("GRPC stream closed, reconnecting in 30 seconds");
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                break;
            }
            catch (Exception e)
            {
                Logger.ForContext(typeof(GRPCService)).Error(e, "Failed to establish GRPC stream, retrying in 30 seconds");
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(30), ct);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }
}