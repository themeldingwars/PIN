using System;
using System.Collections.Generic;
using System.Threading;
using GameServer.Entities;
using GameServer.Entities.Outpost;
using GameServer.Navigation;
using GameServer.Physics;
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
using GameServer.Systems.PlayerRespawn;
using GameServer.Systems.Progression;
using GameServer.Systems.ProjectileSim;
using GameServer.Systems.SystemEvents;
using GameServer.Systems.WeaponSim;
using Serilog;
using Shared.Udp;

namespace GameServer;

public interface IShard : IPacketSender
{
    ulong InstanceId { get; }
    IDictionary<uint, INetworkPlayer> Clients { get; }
    IDictionary<ulong, IEntity> Entities { get; }
    IDictionary<ulong, IEncounter> Encounters { get; }
    IDictionary<uint, IDictionary<uint, OutpostEntity>> Outposts { get; }
    PhysicsEngine Physics { get; }
    NavMeshService NavMesh { get; }
    CrowdNavigationService Crowd { get; }
    AIEngine AI { get; }
    MovementRelay Movement { get; }
    EntityManager EntityMan { get; }
    EncounterManager EncounterMan { get; }
    AbilitySystem Abilities { get; }
    ProjectileSim ProjectileSim { get; }
    WeaponSim WeaponSim { get; }
    ChatService Chat { get; }
    AdminService Admin { get; }
    DamageSystem Damage { get; }
    HostilityResolver Hostility { get; }
    CharacterCreationService CharacterCreation { get; }
    CharacterLifecycleService CharacterLifecycle { get; }
    PlayerRespawnService PlayerRespawn { get; }
    LootService Loot { get; }
    ProgressionService Progression { get; }
    uint ZoneId { get; }
    ILogger Logger { get; }
    GameServerSettings Settings { get; }
    EventBus EventBus { get; }

    int CurrentPlayers => Clients.Count;

    ulong CurrentTimeLong { get; }
    uint CurrentTime => unchecked((uint)CurrentTimeLong);
    ushort CurrentShortTime => unchecked((ushort)CurrentTime);

    ulong GetNextGuid(byte type);
    void Run(CancellationToken ct);
    bool Tick(double deltaTime, ulong currentTime, CancellationToken ct);
    void NetworkTick(double deltaTime, ulong currentTime, CancellationToken ct);
    bool MigrateOut(INetworkPlayer player);
    bool MigrateIn(INetworkPlayer player);
    bool TransferIn(INetworkPlayer player);
}