using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

using MyGameServer.Data;

using Shared.Udp;

namespace MyGameServer {
	public interface IShard : IInstance, IPacketSender {
		IDictionary<uint, INetworkPlayer> Clients { get; }
		PhysicsEngine Physics { get; }
		AIEngine AI { get; }
		int CurrentPlayers => Clients.Count;
		uint CurrentTime { get; }
		ushort CurrentShortTime { get { return unchecked((ushort)CurrentTime); } }
		IDictionary<ushort, Tuple<Entities.IEntity, Enums.GSS.Controllers>> EntityRefMap { get; }

		bool Tick( double deltaTime, uint currTime );
		void NetworkTick( double deltaTime, uint currTime );
		bool MigrateOut( INetworkPlayer player );
		bool MigrateIn( INetworkPlayer player );
		ushort AssignNewRefId( Entities.IEntity entity, Enums.GSS.Controllers controller);
	}
}
