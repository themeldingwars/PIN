using System;
using System.Collections.Generic;
using System.Threading;
using GameServer.Aptitude;
using GameServer.Entities;
using Shared.Udp;

namespace GameServer;

public interface IShard : IInstance, IPacketSender
{
    IDictionary<uint, INetworkPlayer> Clients { get; }
    IDictionary<ulong, IEntity> Entities { get; }
    PhysicsEngine Physics { get; }
    AIEngine AI { get; }
    MovementRelay Movement { get; }
    EntityManager EntityMan { get; }
    AbilitySystem Abilities { get; }
    ChatService Chat { get; }
    AdminService Admin { get; }

    int CurrentPlayers => Clients.Count;

    ulong CurrentTimeLong { get; }
    uint CurrentTime => unchecked((uint)CurrentTimeLong);
    ushort CurrentShortTime => unchecked((ushort)CurrentTime);
    IDictionary<ushort, Tuple<IEntity, Enums.GSS.Controllers>> EntityRefMap { get; }

    void Run(CancellationToken ct);
    bool Tick(double deltaTime, ulong currentTime, CancellationToken ct);
    void NetworkTick(double deltaTime, ulong currentTime, CancellationToken ct);
    bool MigrateOut(INetworkPlayer player);
    bool MigrateIn(INetworkPlayer player);
    ushort AssignNewRefId(IEntity entity, Enums.GSS.Controllers controller);
}