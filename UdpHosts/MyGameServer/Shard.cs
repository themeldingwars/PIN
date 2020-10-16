using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using MyGameServer.Data;
using MyGameServer.Entities;

using Shared.Udp;

namespace MyGameServer {
	public class Shard : IShard, IPacketSender {
		public IDictionary<uint, INetworkPlayer> Clients { get; protected set; }
		public IDictionary<ulong, IEntity> Entities { get; protected set; }
		public PhysicsEngine Physics { get; protected set; }
		public AIEngine AI { get; protected set; }
		public ulong InstanceID { get; }
		protected IPacketSender Sender { get; }
		public ulong CurrentTimeLong { get; protected set; }
		public IDictionary<ushort, Tuple<IEntity, Enums.GSS.Controllers>> EntityRefMap { get; }
		private ushort LastEntityRefId;

		public Shard( double gameTickRate, ulong instID, IPacketSender sender ) {
			Clients = new ConcurrentDictionary<uint, INetworkPlayer>();
			Entities = new ConcurrentDictionary<ulong, IEntity>();
			Physics = new PhysicsEngine( gameTickRate );
			AI = new AIEngine();
			InstanceID = instID;
			Sender = sender;
			EntityRefMap = new ConcurrentDictionary<ushort, Tuple<IEntity, Enums.GSS.Controllers>>();
			LastEntityRefId = 0;
		}

		public bool Tick( double deltaTime, ulong currTime ) {
			CurrentTimeLong = currTime;
			foreach( var c in Clients.Values ) {
				c.Tick( deltaTime, currTime );
			}

			AI.Tick( deltaTime, currTime );
			Physics.Tick( deltaTime, currTime );

			return true;
		}

		public void NetworkTick( double deltaTime, ulong currTime ) {
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

			Clients.Add( player.SocketID, player );
			//Entities.Add(player.CharacterEntity.EntityID, player.CharacterEntity);

			return true;
		}

		public async Task<bool> Send( Memory<byte> p, IPEndPoint ep ) => await Sender.Send( p, ep );

		public ushort AssignNewRefId( IEntity entity, Enums.GSS.Controllers controller ) {
			while( EntityRefMap.ContainsKey(unchecked(++LastEntityRefId)) || LastEntityRefId == 0 || LastEntityRefId == 0xffff )
				;

			EntityRefMap.Add(LastEntityRefId, new Tuple<IEntity, Enums.GSS.Controllers>(entity, controller));

			return unchecked(LastEntityRefId++);
		}
	}
}
