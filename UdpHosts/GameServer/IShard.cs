using System;
using System.Collections.Generic;
using System.Threading;
using GameServer.Aptitude;
using GameServer.Entities;
using GameServer.Entities.Outpost;
using GameServer.Physics;
using GameServer.Systems.Chat;
using GameServer.Systems.Encounters;
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
    AIEngine AI { get; }
    MovementRelay Movement { get; }
    EntityManager EntityMan { get; }
    EncounterManager EncounterMan { get; }
    AbilitySystem Abilities { get; }
    ProjectileSim ProjectileSim { get; }
    WeaponSim WeaponSim { get; }
    ChatService Chat { get; }
    AdminService Admin { get; }
    uint ZoneId { get; }
    ILogger Logger { get; }

    int CurrentPlayers => Clients.Count;

    ulong CurrentTimeLong { get; }
    uint CurrentTime => unchecked((uint)CurrentTimeLong);
    ushort CurrentShortTime => unchecked((ushort)CurrentTime);
    IDictionary<ushort, Tuple<IEntity, Enums.GSS.Controllers>> EntityRefMap { get; }

    ulong GetNextGuid(byte type);
    void Run(CancellationToken ct);
    bool Tick(double deltaTime, ulong currentTime, CancellationToken ct);
    void NetworkTick(double deltaTime, ulong currentTime, CancellationToken ct);
    bool MigrateOut(INetworkPlayer player);
    bool MigrateIn(INetworkPlayer player);
    ushort AssignNewRefId(IEntity entity, Enums.GSS.Controllers controller);
}