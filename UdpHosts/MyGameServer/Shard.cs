using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using MyGameServer.Data;
using MyGameServer.Entities;

using Shared.Common;
using Shared.Udp;

namespace MyGameServer {
	public class Shard : IShard, IPacketSender {
		public const double NetworkTickRate = 1.0 / 20.0;
		protected long startTime;
		protected double lastNetTick;
		public DateTime StartTime { get { return DateTimeExtensions.Epoch.AddSeconds( startTime ); } }

		public IDictionary<uint, INetworkPlayer> Clients { get; protected set; }
		public IDictionary<ulong, IEntity> Entities { get; protected set; }
		public PhysicsEngine Physics { get; protected set; }
		public AIEngine AI { get; protected set; }
		public ulong InstanceID { get; }
		protected IPacketSender Sender { get; }
		public ulong CurrentTimeLong { get; protected set; }
		public IDictionary<ushort, Tuple<IEntity, Enums.GSS.Controllers>> EntityRefMap { get; }
		private ushort LastEntityRefId;
		protected Thread runThread;

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

		public void Run( CancellationToken ct ) {
			runThread = Utils.RunThread( RunThread, ct );
		}

		public void Stop( ) {
			runThread.Abort();
        }

		public void RunThread( CancellationToken ct ) {
			startTime = (long)DateTime.Now.UnixTimestamp();
            lastNetTick = 0;

            var sw = new Stopwatch();
            var lastTime = 0.0;
            ulong currTime;
            double delta;

            sw.Start();

            while( !ct.IsCancellationRequested ) {
                var currt = (ulong)(DateTime.Now.UnixTimestamp() * 1000);
                currTime = unchecked((ulong)sw.Elapsed.TotalMilliseconds);
                delta = currTime - lastTime;

                if( ShouldNetworkTick( currTime - lastNetTick, currt ) ) {
                    NetworkTick( currTime - lastNetTick, currt, ct );
                    lastNetTick = currTime;
                }

                if( !Tick( delta, currt, ct ) )
                    break;

                lastTime = currTime;
                _ = Thread.Yield();
            }

            sw.Stop();
        }

		public bool Tick( double deltaTime, ulong currTime, CancellationToken ct ) {
			CurrentTimeLong = currTime;
			foreach( var c in Clients.Values ) {
				c.Tick( deltaTime, currTime, ct );
			}

			AI.Tick( deltaTime, currTime, ct );
			Physics.Tick( deltaTime, currTime, ct );

			return true;
		}

		protected virtual bool ShouldNetworkTick( double deltaTime, ulong currTime ) => deltaTime >= NetworkTickRate;
		public void NetworkTick( double deltaTime, ulong currTime, CancellationToken ct ) {
			// Handle timeoutd, reliable retransmission, normal rx/tx
			foreach( var c in Clients.Values ) {
				c.NetworkTick( deltaTime, currTime, ct );
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
			while( EntityRefMap.ContainsKey( unchecked(++LastEntityRefId) ) || LastEntityRefId == 0 || LastEntityRefId == 0xffff )
				;

			EntityRefMap.Add( LastEntityRefId, new Tuple<IEntity, Enums.GSS.Controllers>( entity, controller ) );

			return unchecked(LastEntityRefId++);
		}
	}
}
