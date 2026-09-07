using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Net;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using GameServer.Data;
using GameServer.Entities;
using GameServer.Entities.Character;
using GameServer.Entities.Outpost;
using GameServer.Physics;
using GameServer.Systems.Admin;
using GameServer.Systems.Ai;
using GameServer.Systems.Aptitude;
using GameServer.Systems.CharacterLifecycle;
using GameServer.Systems.Chat;
using GameServer.Systems.Combat;
using GameServer.Systems.Encounters;
using GameServer.Systems.EntityManager;
using GameServer.Systems.MovementRelay;
using GameServer.Systems.NpcDeath;
using GameServer.Systems.PlayerRespawn;
using GameServer.Systems.ProjectileSim;
using GameServer.Systems.SystemEvents;
using GameServer.Systems.WeaponSim;
using Serilog;
using Shared.Udp;

namespace GameServer.Tests.Fakes;

/// <summary>
///     Minimal in-memory IShard for unit tests. Only the systems the tests
///     exercise are constructed; everything else stays null.
/// </summary>
public sealed class FakeShard : IShard
{
    public FakeShard()
    {
        EntityMan = new EntityManager(this);
        Damage = new DamageSystem(EventBus, this, new StandardNpcDeathRules());
        CharacterLifecycle = new CharacterLifecycleService(this, EventBus, new StandardCharacterLifecycleRules());
        FallDamage = new FallDamageSystem(this, Damage, new StandardFallDamageRules());
        AI = new AiEngine(this, EventBus, new StandardAiRules(), new AlwaysHostileAiHostility(), AiAttackFeedback, new FakeAiMonsterStats());
    }

    public EventBus EventBus { get; } = new();

    public RecordingAiAttackFeedback AiAttackFeedback { get; } = new();

    public EntityManager EntityMan { get; }

    public DamageSystem Damage { get; }

    public CharacterLifecycleService CharacterLifecycle { get; }

    public FallDamageSystem FallDamage { get; }

    public ulong CurrentTimeLong { get; set; } = 60_000;

    public ulong InstanceId { get; set; } = 1;

    public uint ZoneId { get; set; } = 448;

    public ILogger Logger { get; } = Log.Logger;

    public GameServerSettings Settings { get; } = null;

    public IDictionary<ulong, IEntity> Entities { get; } = new ConcurrentDictionary<ulong, IEntity>();

    public IDictionary<uint, INetworkPlayer> Clients { get; } = new ConcurrentDictionary<uint, INetworkPlayer>();

    public IDictionary<ulong, IEncounter> Encounters { get; } = new ConcurrentDictionary<ulong, IEncounter>();

    public IDictionary<uint, IDictionary<uint, OutpostEntity>> Outposts { get; } = new ConcurrentDictionary<uint, IDictionary<uint, OutpostEntity>>();

    public PhysicsEngine Physics { get; } = null;

    public AiEngine AI { get; set; }

    public MovementRelay Movement { get; } = null;

    public EncounterManager EncounterMan { get; } = null;

    public AbilitySystem Abilities { get; set; } = null;

    public ProjectileSim ProjectileSim { get; } = null;

    public WeaponSim WeaponSim { get; } = null;

    public ChatService Chat { get; } = null;

    public AdminService Admin { get; } = null;

    public CombatSim Combat { get; } = null;

    public PlayerRespawnService PlayerRespawn { get; } = null;

    private ulong _nextGuid = 0x1000;

    public ulong GetNextGuid(byte type = 0)
    {
        return ++_nextGuid;
    }

    public void Run(CancellationToken ct)
    {
    }

    public bool Tick(double deltaTime, ulong currentTime, CancellationToken ct)
    {
        return true;
    }

    public void NetworkTick(double deltaTime, ulong currentTime, CancellationToken ct)
    {
    }

    public bool MigrateOut(INetworkPlayer player)
    {
        return false;
    }

    public bool MigrateIn(INetworkPlayer player)
    {
        return false;
    }

    public Task<bool> SendAsync(Memory<byte> packet, IPEndPoint endPoint)
    {
        return Task.FromResult(true);
    }
}

/// <summary>
///     Minimal INetworkPlayer for unit tests; records debug chat so tests can
///     assert on player facing feedback.
/// </summary>
public sealed class FakeNetworkPlayer : INetworkPlayer
{
    public FakeNetworkPlayer(IShard shard = null)
    {
        AssignedShard = shard;
        NetChannels = ImmutableDictionary.Create<ChannelType, Channel>();
        SequencedMessages = new();
        Preferences = new PlayerPreferences();
        NetClientStatus = ClientStatus.Connected;
        Status = IPlayer.PlayerStatus.Playing;
        ConnectedAt = 0;
    }

    public ClientStatus NetClientStatus { get; set; }

    public uint SocketId { get; set; } = 1;

    public IPEndPoint RemoteEndpoint { get; } = new IPEndPoint(IPAddress.Loopback, 11111);

    public DateTime NetLastActive { get; set; }

    public ImmutableDictionary<ChannelType, Channel> NetChannels { get; private set; }

    public IShard AssignedShard { get; private set; }

    public ConcurrentQueue<Memory<byte>> SequencedMessages { get; }

    public List<string> SentDebugMessages { get; } = new();

    public int RespawnCount { get; private set; }

    public ulong PlayerId { get; set; } = 0x42;

    public ulong CharacterId { get; set; } = 0x4200;

    public CharacterEntity CharacterEntity { get; set; }

    public CharacterInventory Inventory { get; set; }

    public IPlayer.PlayerStatus Status { get; set; }

    public PlayerPreferences Preferences { get; }

    public Zone CurrentZone { get; }

    public uint ConnectedAt { get; }

    public uint LastRequestedUpdate { get; set; }

    public uint RequestedClientTime { get; set; }

    public bool FirstUpdateRequested { get; set; }

    public bool CanReceiveGSS => true;

    public ulong SteamUserId { get; set; }

    public void Init(IPlayer player, IShard shard, IPacketSender sender)
    {
        AssignedShard = shard;
    }

    public void HandlePacket(ReadOnlyMemory<byte> data, Packet packet)
    {
    }

    public void NetworkTick(double deltaTime, ulong currentTime, CancellationToken ct)
    {
    }

    public void Send(Memory<byte> packet)
    {
    }

    public void SendAck(ChannelType forChannel, ushort forSequenceNumber, DateTime? received = null)
    {
    }

    public void SendDebugChat(string message)
    {
        SentDebugMessages.Add(message);
    }

    public void SendDebugLog(string log)
    {
    }

    public void Init(IShard shard)
    {
        AssignedShard = shard;
    }

    public void Login(ulong characterId)
    {
    }

    public void EnterZoneAck()
    {
    }

    public void ExitZoneAck()
    {
    }

    public void Ready()
    {
    }

    public void Respawn()
    {
        ++RespawnCount;
    }

    public void Jump()
    {
    }

    public void Tick(double deltaTime, ulong currentTime, CancellationToken ct)
    {
    }

    public uint FindClosestAvailableOutpost(Zone zone, uint targetOutpostId = 0)
    {
        return 0;
    }

    public void HandleFireWeaponProjectile(uint time, Vector3 aim, Vector3? shooterVelocity = null)
    {
    }

    public void RequestRevive(INetworkPlayer reviver)
    {
    }
}
