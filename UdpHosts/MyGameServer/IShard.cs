using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

using Shared.Udp;

namespace MyGameServer {
	public interface IShard : IInstance, IPacketSender {
		public ConcurrentDictionary<uint, INetworkPlayer> Clients { get; }
		public PhysicsEngine Physics { get; }
		public AIEngine AI { get; }
		public int CurrentPlayers => Clients.Count;

		bool Tick( double deltaTime, double currTime );
		void NetworkTick( double deltaTime, double currTime );
		bool MigrateOut( INetworkPlayer player );
		bool MigrateIn( INetworkPlayer player );
	}
}
