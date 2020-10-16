using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

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
		private BufferBlock<Packet?> incomingPackets = null;
		private BufferBlock<Packet?> outgoingPackets = null;
		private CancellationTokenSource source;

		protected long startTime;
		protected double lastNetTick;



		public PacketServer( ushort port ) {
			listenEndpoint = new IPEndPoint( IPAddress.Loopback, port );
			serverSocket = new Socket( AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp );
		}



		protected abstract void HandlePacket( Packet p );
		protected abstract void Startup( CancellationToken ct );
		protected abstract bool Tick( double deltaTime, ulong currTime, CancellationToken ct );
		protected abstract void Shutdown( CancellationToken ct );



		protected virtual bool ShouldNetworkTick( double deltaTime, ulong currTime ) => deltaTime >= NetworkTickRate;
		protected virtual void NetworkTick( double deltaTime, ulong currTime, CancellationToken ct ) {
			Packet? p;
			while( incomingPackets.Count > 0 && ( p = incomingPackets.Receive( ct )) != null ) {
				HandlePacket( p.Value );
			}
		}



		// TODO: Move to seperate thread? add console/rcon handling here?
		public void Run() {
			source = new CancellationTokenSource();

			var ct = source.Token;
			var listenThread = Utils.RunThread(ListenThread, ct);
			//var readThread = Utils.RunThread(ReadThread, ct);
			var sendThread = Utils.RunThread(SendThread, ct);

			Startup(ct);

			startTime = (long)DateTime.Now.UnixTimestamp();
			lastNetTick = 0;

			var sw = new Stopwatch();
			var lastTime = 0.0;
			ulong currTime;
			double delta;

			sw.Start();

			while( true ) {
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

			if( !source.IsCancellationRequested)
				source.Cancel();

			Shutdown(ct);
		}

		private async void ListenThread( CancellationToken ct ) {
			serverSocket.Blocking = true;
			serverSocket.DontFragment = true;
			serverSocket.ReceiveBufferSize = MTU * 100;
			serverSocket.SendBufferSize = MTU * 100;
			serverSocket.SetSocketOption( SocketOptionLevel.IP, SocketOptionName.PacketInformation, true );
			serverSocket.Bind( listenEndpoint );

			Logger.Information( "Listening on {0}", listenEndpoint );

			incomingPackets = new BufferBlock<Packet?>();
			outgoingPackets = new BufferBlock<Packet?>();
			byte[] buffer = new byte[MTU*10];
			EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
			int c;

			Thread.CurrentThread.Priority = ThreadPriority.Highest;

			while( true ) {
				if( ct.IsCancellationRequested )
					break;
				
				try {
					// Sockets don't support async yet :( Blocking here bc the win api will yiald and wait better on the native side than we can here
					if( (c = serverSocket.ReceiveFrom(buffer, SocketFlags.None, ref remoteEP)) > 0 ) {
						// Should prolly change to ArrayPool<byte>, but can't return a Memory<byte> :(
						var buf = new byte[c];
						buffer.AsSpan().Slice(0, c).ToArray().CopyTo(buf, 0);
						_ = await incomingPackets.SendAsync(new Packet((IPEndPoint)remoteEP, new ReadOnlyMemory<byte>(buf, 0, c), DateTime.Now), ct);

						//remoteEP = new IPEndPoint(IPAddress.Any, 0);
					}
				} catch( Exception ex ) {
					Logger.Error(ex, "Error {0}", "listenThread");
				}

				_ = Thread.Yield();
			}
		}

		// FIXME: Move this to NetworkTick
		private async void ReadThread( CancellationToken ct ) {
			while( incomingPackets == null )
				Thread.Sleep( 10 );

			Thread.CurrentThread.Priority = ThreadPriority.Highest;
			Packet? p;

			while( true ) {
				if( ct.IsCancellationRequested )
					break;

				while( (p = await incomingPackets.ReceiveAsync(ct)) != null ) {
					HandlePacket( p.Value );
				}

				_ = Thread.Yield();
			}
		}

		private async void SendThread( CancellationToken ct ) {
			while( outgoingPackets == null )
				Thread.Sleep( 10 );

			Thread.CurrentThread.Priority = ThreadPriority.Highest;
			Packet? p;

			while( true ) {
				if( ct.IsCancellationRequested )
					break;

				while( (p = await outgoingPackets.ReceiveAsync(ct)) != null ) {
					_ = serverSocket.SendTo( p.Value.PacketData.ToArray(), p.Value.PacketData.Length, SocketFlags.None, p.Value.RemoteEndpoint );
				}

				_ = Thread.Yield();
			}
		}

		public async Task<bool> Send( Memory<byte> p, IPEndPoint ep ) => await outgoingPackets.SendAsync( new Packet( ep, p ) );
	}
}
