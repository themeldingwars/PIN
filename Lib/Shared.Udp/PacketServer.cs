using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using Serilog;

using Shared.Common;

namespace Shared.Udp {
	public abstract class PacketServer : IPacketSender {
		public const double NetworkTickRate = 1.0 / 20.0;
		public const int MTU = 1400;

		public static ILogger Logger;

		public DateTime StartTime { get { return DateTimeExtensions.Epoch.AddSeconds( startTime ); } }



		private readonly Socket serverSocket;
		private readonly IPEndPoint listenEndpoint;
		private ConcurrentQueue<Packet> incomingPackets = null;
		private ConcurrentQueue<Packet> outgoingPackets = null;

		protected long startTime;
		protected double lastNetTick;



		public PacketServer( ushort port ) {
			listenEndpoint = new IPEndPoint( IPAddress.Loopback, port );
			serverSocket = new Socket( AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp );
		}



		protected abstract void HandlePacket( Packet p );
		protected abstract void Startup();
		protected abstract bool Tick( double deltaTime, ulong currTime );
		protected abstract void NetworkTick( double deltaTime, ulong currTime );
		protected abstract void Shutdown();

		protected virtual bool ShouldNetworkTick( double deltaTime, ulong currTime ) => deltaTime >= NetworkTickRate;



		public void Run() {
			var listenThread = Utils.RunThread(ListenThread);
			var readThread = Utils.RunThread(ReadThread);
			var sendThread = Utils.RunThread(SendThread);

			startTime = (long)DateTime.Now.UnixTimestamp();
			lastNetTick = 0;

			var sw = new Stopwatch();
			var lastTime = 0.0;
			var currTime = 0uL;
			var delta = 0.0;

			Startup();

			sw.Start();

			while( true ) {
				var ct = (ulong)(DateTime.Now.UnixTimestamp() * 1000);
				currTime = unchecked((ulong)sw.Elapsed.TotalMilliseconds);
				delta = currTime - lastTime;

				if( !Tick( delta, ct ) )
					break;

				if( ShouldNetworkTick( currTime - lastNetTick, ct ) ) {
					NetworkTick( currTime - lastNetTick, ct );
					lastNetTick = currTime;
				}

				lastTime = currTime;
			}

			sw.Stop();
			Shutdown();
		}

		private void ListenThread() {
			serverSocket.Blocking = true;
			serverSocket.DontFragment = true;
			serverSocket.ReceiveBufferSize = MTU * 100;
			serverSocket.SendBufferSize = MTU * 100;
			serverSocket.SetSocketOption( SocketOptionLevel.IP, SocketOptionName.PacketInformation, true );
			serverSocket.Bind( listenEndpoint );

			Logger.Information( "Listening on {0}", listenEndpoint );

			incomingPackets = new ConcurrentQueue<Packet>();
			outgoingPackets = new ConcurrentQueue<Packet>();
			byte[] buffer = new byte[MTU*10];
			EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
			int c;

			Thread.CurrentThread.Priority = ThreadPriority.Highest;

			while( true ) {
				try {
					if( (c = serverSocket.ReceiveFrom(buffer, SocketFlags.None, ref remoteEP)) > 0 ) {
						// Should prolly change to ArrayPool<byte>, but can't return a Memory<byte> :(
						var buf = new byte[c];
						buffer.AsSpan().Slice(0, c).ToArray().CopyTo(buf, 0);
						incomingPackets.Enqueue(new Packet((IPEndPoint)remoteEP, new ReadOnlyMemory<byte>(buf, 0, c)));

						remoteEP = new IPEndPoint(IPAddress.Any, 0);
					}
				} catch( Exception ex ) {
					Logger.Error(ex, "Error {0}", "listenThread");
				}

				_ = Thread.Yield();
			}
		}

		private void ReadThread() {
			while( incomingPackets == null )
				Thread.Sleep( 10 );

			Thread.CurrentThread.Priority = ThreadPriority.Highest;

			while( true ) {
				while( incomingPackets.TryDequeue( out Packet p ) ) {
					_ = Task.Run(() => HandlePacket( p ));
					//HandlePacket(p);
				}

				_ = Thread.Yield();
			}
		}

		private void SendThread() {
			while( outgoingPackets == null )
				Thread.Sleep( 10 );

			Thread.CurrentThread.Priority = ThreadPriority.Highest;
			int c;
			while( true ) {
				while( outgoingPackets.TryDequeue( out Packet p ) ) {
					_ = serverSocket.SendTo( p.PacketData.ToArray(), p.PacketData.Length, SocketFlags.None, p.RemoteEndpoint );
				}

				_ = Thread.Yield();
			}
		}

		public void Send<T>( T pkt, IPEndPoint ep ) where T : struct {
			Send( Utils.WriteStruct( pkt ), ep );
		}

		public void Send( Memory<byte> p, IPEndPoint ep ) {
			var pkt = new Packet(ep, p);
			outgoingPackets.Enqueue( pkt );
		}
	}
}
