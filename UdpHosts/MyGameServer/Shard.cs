using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Text;

using Shared.Udp;

namespace MyGameServer {
	public class Shard : IShard, IPacketSender {
		public ConcurrentDictionary<uint, INetworkPlayer> Clients { get; protected set; }
		public PhysicsEngine Physics { get; protected set; }
		public AIEngine AI { get; protected set; }
		public ulong InstanceID { get; }
		protected IPacketSender Sender { get; }

		public Shard( double gameTickRate, ulong instID, IPacketSender sender ) {
			Clients = new ConcurrentDictionary<uint, INetworkPlayer>();
			Physics = new PhysicsEngine( gameTickRate );
			AI = new AIEngine();
			InstanceID = instID;
			Sender = sender;
		}

		public bool Tick( double deltaTime, double currTime ) {
			foreach( var c in Clients.Values ) {
				c.Tick( deltaTime, currTime );
			}

			AI.Tick( deltaTime, currTime );
			Physics.Tick( deltaTime, currTime );

			return true;
		}

		public void NetworkTick( double deltaTime, double currTime ) {
			// Handle timeoutd, reliable retransmission, normal rx/tx
			foreach( var c in Clients.Values ) {
				c.NetworkTick( deltaTime, currTime );
			}
		}

		public bool MigrateOut( INetworkPlayer player ) { return false; }
		public bool MigrateIn( INetworkPlayer player ) {
			if( Clients.ContainsKey( player.SocketID ) )
				return true;

			player.Init( this );
			_ = Clients.AddOrUpdate( player.SocketID, player, ( s, old ) => old );
			return true;
		}

		public void Send( Memory<byte> p, IPEndPoint ep ) {
			Sender.Send( p, ep );
		}
	}
}
