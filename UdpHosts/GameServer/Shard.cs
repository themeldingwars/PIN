using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using DemoRenderer;
using GameServer.Entities;
using GameServer.Entities.Outpost;
using GameServer.Navigation;
using GameServer.Physics;
using GameServer.Protocol;
using GameServer.Systems.Admin;
using GameServer.Systems.Aptitude;
using GameServer.Systems.CharacterCreation;
using GameServer.Systems.CharacterLifecycle;
using GameServer.Systems.Chat;
using GameServer.Systems.Combat;
using GameServer.Systems.Encounters;
using GameServer.Systems.EntityManager;
using GameServer.Systems.Loot;
using GameServer.Systems.MovementRelay;
using GameServer.Systems.NpcDeath;
using GameServer.Systems.PlayerRespawn;
using GameServer.Systems.Progression;
using GameServer.Systems.ProjectileSim;
using GameServer.Systems.SystemEvents;
using GameServer.Systems.Turret;
using GameServer.Systems.WeaponSim;
using Shared.Common;
using Shared.Udp;

namespace GameServer;

public class Shard : IShard
{
    private readonly AutoResetEvent _wake = new(false);
    private readonly TimeSpan _gameTickRate;

    private long _startTime;
    private TimeSpan _lastNetTick;
    private Thread _debugRendererThread;

    public Shard(TimeSpan gameTickRate, ulong instanceId, uint zoneId, GameServerSettings settings, IPacketSender sender, Serilog.ILogger logger)
    {
        _gameTickRate = gameTickRate;
        InstanceId = instanceId;
        Settings = settings;
        ZoneId = zoneId;
        Sender = sender;
        Logger = logger;
        Clients = new ConcurrentDictionary<uint, INetworkPlayer>();
        Entities = new ConcurrentDictionary<ulong, IEntity>();
        Encounters = new ConcurrentDictionary<ulong, IEncounter>();
        Outposts = new ConcurrentDictionary<uint, IDictionary<uint, OutpostEntity>>();
        EventBus = new EventBus();
        Movement = new MovementRelay(this);
        var debugCallbacks = new DebugProjectileHitCallbacks(this);
        Physics = new PhysicsEngine(
            new()
            {
                ZoneId = zoneId,
                MapsPath = Settings.MapsPath,
                AssetDBPath = Settings.AssetDBPath,
                LoadMapsCollision = Settings.LoadMapsCollision,
                IsDebugPipeClient = false,
                EnableDebugPipe = false,
                CachePath = Settings.CachePath,
                ForceReload = Settings.ForceReloadZone,
                EnablePhysicsDebugRenderer = Settings.EnablePhysicsDebugRenderer,
                PhysicsDebugRendererFrameRate = Settings.PhysicsDebugRendererFrameRate,
                PhysicsDebugRendererShowConstraints = false,
                PhysicsDebugRendererShowContacts = false,
                PhysicsDebugRendererShowBoundingBoxes = false
            },
            EventBus,
            Movement,
            debugCallbacks);
        NavMesh = new NavMeshService(logger);
        NavMesh.Load(zoneId, Settings.CachePath);
        Crowd = new CrowdNavigationService(NavMesh, Settings);
        Hostility = new HostilityResolver();
        AI = new AIEngine(this);
        Abilities = new AbilitySystem(this);
        EntityMan = new EntityManager(this);
        EncounterMan = new EncounterManager(this);
        WeaponSim = new WeaponSim(this);
        TurretSim = new TurretSystem(this);
        ProjectileSim = new ProjectileSim(this, debugCallbacks);
        Chat = new ChatService(this, EventBus);
        Admin = new AdminService(this);
        var npcDeathRules = new StandardNpcDeathRules();
        Damage = new DamageSystem(EventBus, this, npcDeathRules);
        Combat = new CombatSim(EventBus, EntityMan, Damage, this);
        CharacterLifecycle = new CharacterLifecycleService(this, EventBus, new StandardCharacterLifecycleRules());
        PlayerRespawn = new PlayerRespawnService(this, EventBus, new StandardPlayerRespawnRules(), CharacterLifecycle);
        NpcDeath = new NpcDeathService(this, EventBus, npcDeathRules);
        Loot = new LootService(this, EventBus, new StandardLootRules());
        Progression = new ProgressionService(this, EventBus, new StandardProgressionRules());
        CharacterCreation = new CharacterCreationService(this, EventBus, new StandardCharacterCreationRules());
    }

    public DateTime StartTime => DateTimeExtensions.Epoch.AddSeconds(_startTime);
    public IDictionary<ulong, IEntity> Entities { get; protected set; }
    public IDictionary<ulong, IEncounter> Encounters { get; protected set; }
    public IDictionary<uint, IDictionary<uint, OutpostEntity>> Outposts { get; protected set; }
    public IDictionary<uint, INetworkPlayer> Clients { get; }
    public EventBus EventBus { get; }
    public PhysicsEngine Physics { get; }
    public NavMeshService NavMesh { get; }
    public CrowdNavigationService Crowd { get; }
    public AIEngine AI { get; }
    public MovementRelay Movement { get; }
    public EntityManager EntityMan { get; }
    public EncounterManager EncounterMan { get; }
    public AbilitySystem Abilities { get; }
    public ProjectileSim ProjectileSim { get; }
    public WeaponSim WeaponSim { get; }
    public TurretSystem TurretSim { get; }
    public ChatService Chat { get; }
    public AdminService Admin { get; }
    public DamageSystem Damage { get; }
    public HostilityResolver Hostility { get; }
    public CombatSim Combat { get; }
    public CharacterCreationService CharacterCreation { get; }
    public CharacterLifecycleService CharacterLifecycle { get; }
    public PlayerRespawnService PlayerRespawn { get; }
    public NpcDeathService NpcDeath { get; }
    public LootService Loot { get; }
    public ProgressionService Progression { get; }
    public ulong InstanceId { get; }
    public uint ZoneId { get; }
    public ulong CurrentTimeLong { get; private set; }
    public uint CurrentTime => unchecked((uint)CurrentTimeLong);
    public ushort CurrentShortTime => unchecked((ushort)CurrentTime);
    public Serilog.ILogger Logger { get; }
    public GameServerSettings Settings { get; }
    private IPacketSender Sender { get; }

    public void Run(CancellationToken ct)
    {
        if (Settings.EnablePhysicsDebugRenderer)
        {
            // Build the one-shot navmesh overlay mesh (from the already-loaded DtNavMesh) before the
            // GL window thread starts rendering. Runs here, before the tick loop, so the GL thread
            // never races the extraction; the mesh cache lock serializes the upload with the GL thread.
            Physics.DebugRenderExtractNavMesh(NavMesh);
            SpawnDebugRenderer(ct);
        }

        Utils.RunThread(RunThread, ct);
    }

    public void NetworkTick(double deltaTime, ulong currentTime, CancellationToken ct)
    {
        // Handle timeout, reliable retransmission, normal rx/tx
        foreach (var client in Clients.Values)
        {
            client.NetworkTick(deltaTime, currentTime, ct);
        }
    }

    /// <summary>
    ///     Send an already serialized GSS message to the players of this shard. The caller serializes the
    ///     message once; each recipient only pays for its own sequence number and channel headers.
    /// </summary>
    /// <param name="entityId">Id of the entity the message is for</param>
    /// <param name="messageId">GSS message id</param>
    /// <param name="typecode">GSS typecode the message rides on</param>
    /// <param name="messageData">Serialized message body</param>
    /// <param name="channelType">GSS channel to send on: ReliableGss or UnreliableGss</param>
    /// <param name="filter">Optional filter deciding which players receive the message</param>
    public void BroadcastGssMessage(ulong entityId, byte messageId, byte typecode, ReadOnlySpan<byte> messageData, ChannelType channelType = ChannelType.UnreliableGss, Predicate<INetworkPlayer> filter = null)
    {
        foreach (var client in Clients.Values)
        {
            if (client.CanReceiveGSS && (filter == null || filter(client)))
            {
                client.NetChannels[channelType].SendGssMessage(entityId, messageId, typecode, messageData);
            }
        }
    }

    /// <summary>
    ///     Send an already serialized Matrix message to the players of this shard
    /// </summary>
    /// <param name="messageId">Wire id of the matrix message</param>
    /// <param name="messageData">Serialized message body</param>
    /// <param name="filter">Optional filter deciding which players receive the message</param>
    public void BroadcastMatrixMessage(byte messageId, ReadOnlySpan<byte> messageData, Predicate<INetworkPlayer> filter = null)
    {
        foreach (var client in Clients.Values)
        {
            if (filter == null || filter(client))
            {
                client.NetChannels[ChannelType.Matrix].SendMatrixMessage(messageId, messageData);
            }
        }
    }

    public bool Tick(double deltaTime, ulong currentTime, CancellationToken ct)
    {
        CurrentTimeLong = currentTime;

        // AI runs after physics so it can read the integrated pose back from dynamic NPC bodies,
        // steer, and broadcast the result. Players (kinematic) are unaffected by the ordering.
        Physics.Tick(deltaTime, currentTime, ct);
        AI.Tick(deltaTime, currentTime, ct);
        EntityMan.Tick(deltaTime, currentTime, ct);
        EncounterMan.Tick(deltaTime, currentTime, ct);
        Abilities.Tick(deltaTime, currentTime, ct);
        WeaponSim.Tick(deltaTime, currentTime, ct);
        TurretSim.Tick(deltaTime, currentTime, ct);
        ProjectileSim.Tick(deltaTime, currentTime, ct);
        Damage.Tick(deltaTime, currentTime, ct);
        CharacterLifecycle.Tick(deltaTime, currentTime, ct);
        PlayerRespawn.Tick(deltaTime, currentTime, ct);
        Loot.Tick(deltaTime, currentTime, ct);
        EventBus.Flush();

        return true;
    }

    public bool MigrateOut(INetworkPlayer player)
    {
        if (Clients.ContainsKey(player.SocketId))
        {
            if (Entities.ContainsKey(player.CharacterId))
            {
                EntityMan.Remove(player.CharacterId);
            }

            Clients.Remove(player.SocketId);
            EntityMan.OnPlayerLeft(player);
            Admin.ClearPlayer(player);
            return true;
        }

        return false;
    }

    public bool MigrateIn(INetworkPlayer player)
    {
        if (Clients.ContainsKey(player.SocketId))
        {
            return true;
        }

        player.Init(this);

        Clients.Add(player.SocketId, player);

        return true;
    }

    public bool TransferIn(INetworkPlayer player)
    {
        if (Clients.ContainsKey(player.SocketId))
        {
            return true;
        }

        // Unlike MigrateIn this keeps the client's channels, so it's safe for a connection that's already talking to us
        player.AssignShard(this);

        Clients.Add(player.SocketId, player);

        return true;
    }

    public async Task<bool> SendAsync(Memory<byte> packet, IPEndPoint endPoint)
    {
        return await Sender.SendAsync(packet, endPoint);
    }

    public ulong GetNextGuid(byte type = 0)
    {
        return GuidService.GetNext(this, type);
    }

    public void Wake()
    {
        _wake.Set();
    }

    private void SpawnDebugRenderer(CancellationToken ct)
    {
        var bodyFrames = Physics.DebugBodyFrames;
        var staticFrames = Physics.DebugStaticFrames;
        var meshCache = Physics.DebugRendererMeshCache;
        var cameraState = Physics.DebugCameraState;
        var showFlagsState = Physics.DebugShowFlagsState;
        if (bodyFrames == null || staticFrames == null)
        {
            Logger.Warning("Physics debug renderer requested but the engine did not initialize the frame buffers");
            return;
        }

        Action<CancellationToken> runWindow = token =>
        {
            try
            {
                Func<DemoRenderer.ShapeDrawing.MeshInstance?> navMeshProvider =
                    () => Physics.DebugNavMesh is { Ready: true } nm ? nm.Instance : null;
                using var window = new PhysicsDebugWindow(
                    $"PIN Physics Debug - Zone {ZoneId}",
                    new BepuUtilities.Int2(1280, 720),
                    bodyFrames,
                    staticFrames,
                    Physics.DebugRendererFrameRate,
                    null,
                    meshCache,
                    ResolveDebugPosition,
                    cameraState,
                    showFlagsState,
                    navMeshProvider,
                    false);
                window.Run(token);
            }
            catch (Exception e)
            {
                Logger.Error(e, "Physics debug renderer window failed to run");
            }
        };
        _debugRendererThread = Utils.RunThread(runWindow, ct);
    }

    /// <summary>
    ///     Resolves a world position to a display string for the debug renderer overlay (the zone chunk
    ///     and subchunk the position falls in). Backed by the spatial index the zone loader builds.
    ///     Returns empty when the position is outside the loaded zone.
    /// </summary>
    /// <param name="position">The world position to resolve.</param>
    /// <returns>The chunk/subchunk label for the position, or an empty string when outside the loaded zone.</returns>
    private string ResolveDebugPosition(Vector3 position)
    {
        var index = Physics.DebugZoneSpatialIndex;
        if (index == null || !index.TryResolve(position, out var chunkName, out var subChunkName))
        {
            return string.Empty;
        }

        return $"Chunk: {chunkName}   SubChunk: {subChunkName}";
    }

    private void RunThread(CancellationToken ct)
    {
        _startTime = (long)DateTime.UtcNow.UnixTimestampUtc();
        _lastNetTick = TimeSpan.Zero;

        var stopwatch = Stopwatch.StartNew();
        var lastTick = TimeSpan.Zero;
        var nextTick = TimeSpan.Zero;
        var waitHandles = new[] { _wake, ct.WaitHandle };

        while (!ct.IsCancellationRequested)
        {
            // Packets get handled and acked right away, the live servers answered within a few ms
            NetworkTick(stopwatch.Elapsed, ct);

            var now = stopwatch.Elapsed;
            if (now >= nextTick)
            {
                var currentUnixTimestamp = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                if (!Tick((now - lastTick).TotalMilliseconds, currentUnixTimestamp, ct))
                {
                    break;
                }

                lastTick = now;

                nextTick += _gameTickRate;

                // No catch-up burst after a late tick
                if (nextTick < now)
                {
                    nextTick = now;
                }

                // Send what the tick produced now, not after the wait
                NetworkTick(stopwatch.Elapsed, ct);
            }

            var wait = nextTick - stopwatch.Elapsed;
            if (wait > TimeSpan.Zero)
            {
                // Rounded up, a wait below 1 ms would return at once and spin
                WaitHandle.WaitAny(waitHandles, (int)Math.Ceiling(wait.TotalMilliseconds));
            }
        }

        stopwatch.Stop();
    }

    private void NetworkTick(TimeSpan now, CancellationToken ct)
    {
        var currentUnixTimestamp = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        NetworkTick((now - _lastNetTick).TotalMilliseconds, currentUnixTimestamp, ct);
        _lastNetTick = now;
    }
}