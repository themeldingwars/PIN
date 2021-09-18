using GameServer.Entities;
using Shared.Udp;
using System;
using System.Collections.Generic;
using System.Threading;

namespace GameServer
{
    public interface IShard : IInstance, IPacketSender
    {
        IDictionary<uint, INetworkPlayer> Clients { get; }
        PhysicsEngine Physics { get; }
        AIEngine AI { get; }

        int CurrentPlayers => Clients.Count;

        ulong CurrentTimeLong { get; }
        uint CurrentTime => unchecked((uint)CurrentTimeLong);
        ushort CurrentShortTime => unchecked((ushort)CurrentTime);
        IDictionary<ushort, Tuple<IEntity, Enums.GSS.Controllers>> EntityRefMap { get; }

        void Run(CancellationToken ct);
        void Stop();
        bool Tick(double deltaTime, ulong currTime, CancellationToken ct);
        void NetworkTick(double deltaTime, ulong currTime, CancellationToken ct);
        bool MigrateOut(INetworkPlayer player);
        bool MigrateIn(INetworkPlayer player);
        ushort AssignNewRefId(IEntity entity, Enums.GSS.Controllers controller);
    }
}