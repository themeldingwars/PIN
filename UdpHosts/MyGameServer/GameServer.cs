using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

using Shared.Udp;
using MyGameServer.Packets;
using System.Collections.Concurrent;
using System.Reflection.Metadata.Ecma335;
using System.Buffers;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace MyGameServer {
	class GameServer : PacketServer {
		public const double GameTickRate = 1.0 / 60.0;
		public const int MinPlayersPerShard = 16;
		public const int MaxPlayersPerShard = 64;
		public delegate void SendPacketDelegate<T>( T pkt, IPEndPoint ep ) where T : struct;

		protected ConcurrentDictionary<uint, INetworkPlayer> ClientMap;
		protected ConcurrentDictionary<ulong, IShard> Shards;

		protected byte nextShardID;
		protected ulong ServerID;

		public GameServer( ushort port, ulong serverID ) : base( port ) {
			ClientMap = new ConcurrentDictionary<uint, INetworkPlayer>();
			Shards = new ConcurrentDictionary<ulong, IShard>();

			nextShardID = 1;
			ServerID = serverID;
		}

		protected override void Startup() {
			Test.DataUtils.Init();
			Controllers.Factory.Init();
		}

		protected IShard GetNextShard() {
			foreach( var s in Shards.Values ) {
				if( s.CurrentPlayers < MaxPlayersPerShard )
					return s;
			}

			return NewShard();
		}

		protected IShard NewShard() {
			var id = ServerID | (ulong)((nextShardID++) << 8);
			return Shards.AddOrUpdate( id, new Shard( GameTickRate, id, this ), ( id, old ) => old );
		}

		protected override bool Tick( double deltaTime, double currTime ) {
			foreach( var s in Shards.Values ) {
				if( !s.Tick( deltaTime, currTime ) || s.CurrentPlayers < MinPlayersPerShard ) {
					// TODO: Shutdown Shard
				}
			}

			return true;
		}

		protected override void NetworkTick( double deltaTime, double currTime ) {
			foreach( var s in Shards.Values ) {
				s.NetworkTick( deltaTime, currTime );
			}
		}
		protected override void Shutdown() {
		}

		protected override void HandlePacket( Packet packet ) {
			//Program.Logger.Information("[GAME] {0} sent {1} bytes.", packet.RemoteEndpoint, packet.PacketData.Length);
			//Program.Logger.Verbose(">  {0}", BitConverter.ToString(packet.PacketData.ToArray()).Replace("-", " "));

			var socketID = Utils.SimpleFixEndianess(packet.Read<uint>());
			INetworkClient client;

			if( !ClientMap.ContainsKey( socketID ) ) {
				var c = new NetworkPlayer(packet.RemoteEndpoint, socketID);
				
				client = ClientMap.AddOrUpdate( socketID, c, ( id, nc ) => { return nc; } );
				_ = GetNextShard().MigrateIn( (INetworkPlayer)client );
			} else
				client = ClientMap[socketID];

			client.HandlePacket( packet.PacketData.Slice( 4 ) );
		}
	}
}