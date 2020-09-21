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
		protected abstract bool Tick( double deltaTime, double currTime );
		protected abstract void NetworkTick( double deltaTime, double currTime );
		protected abstract void Shutdown();

		protected virtual bool ShouldNetworkTick( double deltaTime, double currTime ) => deltaTime >= NetworkTickRate;



		public void Run() {
			var listenThread = Utils.RunThread(ListenThread);
			var readThread = Utils.RunThread(ReadThread);
			var sendThread = Utils.RunThread(SendThread);

			startTime = (long)DateTime.Now.UnixTimestamp();
			lastNetTick = 0;

			var sw = new Stopwatch();
			var lastTime = 0.0;
			var currTime = 0.0;
			var delta = 0.0;

			Startup();

			sw.Start();

			while( true ) {
				currTime = sw.Elapsed.TotalSeconds;
				delta = currTime - lastTime;

				if( !Tick( delta, currTime ) )
					break;

				if( ShouldNetworkTick( currTime - lastNetTick, currTime ) ) {
					NetworkTick( currTime - lastNetTick, currTime );
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

			while( true ) {
				if( (c = serverSocket.ReceiveFrom( buffer, SocketFlags.None, ref remoteEP )) > 0 ) {
					// Make sure each Packet has its own memory to prevent corruption
					// Should prolly change to ArrayPool<byte>, but can't return a Memory<byte> :(
					Memory<byte> buf = new byte[c];
					buffer.AsSpan( 0, c ).CopyTo( buf.Span );
					var p = new Packet((IPEndPoint)remoteEP, buf);
					incomingPackets.Enqueue( p );

					remoteEP = new IPEndPoint( IPAddress.Any, 0 );
				}
			}
		}

		private void ReadThread() {
			while( incomingPackets == null )
				Thread.Sleep( 10 );

			while( true ) {
				while( incomingPackets.TryDequeue( out Packet p ) ) {
					HandlePacket( p );
				}

				_ = Thread.Yield();
			}
		}

		private void SendThread() {
			while( outgoingPackets == null )
				Thread.Sleep( 10 );

			int c;
			while( true ) {
				while( outgoingPackets.TryDequeue( out Packet p ) ) {
					c = serverSocket.SendTo( p.PacketData.ToArray(), p.PacketData.Length, SocketFlags.None, p.RemoteEndpoint );
					//Logger.Verbose( "<- sent {0}/{1} bytes.", c, p.PacketData.Length );
					//Logger.Verbose("<  {0}", BitConverter.ToString(p.PacketData.ToArray()).Replace("-", ""));
				}

				_ = Thread.Yield();
			}
		}



		public void SendBE<T>( T pkt, IPEndPoint ep ) where T : struct {
			Send( Utils.WriteStructBE( pkt ), ep );
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
