using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Buffers;
using Serilog;

namespace ServerShared {
	public abstract class PacketServer : IPacketSender {
		public static ILogger Logger;
		public const int MTU = 1400;

		private readonly Socket serverSocket;
		private readonly IPEndPoint listenEndpoint;
		private ConcurrentQueue<Packet> incomingPackets = null;
		private ConcurrentQueue<Packet> outgoingPackets = null;
		private DateTime StartTime;

		public PacketServer(ushort port) {
			listenEndpoint = new IPEndPoint( IPAddress.Loopback, port );
			serverSocket = new Socket( AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp );
		}

		protected abstract void HandlePacket( Packet p );

		protected abstract void Startup();
		protected abstract void Tick();
		protected abstract void NetworkTick();
		protected abstract void Shutdown();

		public void Run() {
			var listenThread = Utils.RunThread(ListenThread);
			var readThread = Utils.RunThread(ReadThread);
			var sendThread = Utils.RunThread(SendThread);

			StartTime = DateTime.Now;
			Startup();

			while( true ) {
				Tick();
				if( true /* Should Network tick*/) {
					NetworkTick();
				}
			}

			Shutdown();
		}

		

		private void ListenThread() {
			serverSocket.Blocking = true;
			serverSocket.ReceiveBufferSize = MTU * 100;
			serverSocket.SetSocketOption( SocketOptionLevel.IP, SocketOptionName.PacketInformation, true );
			serverSocket.Bind( listenEndpoint );

			Logger.Information( "Listening on {0}", listenEndpoint );

			incomingPackets = new ConcurrentQueue<Packet>();
			outgoingPackets = new ConcurrentQueue<Packet>();
			byte[] buffer = new byte[MTU];
			EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
			int c;

			while( true ) {
				if( (c = serverSocket.ReceiveFrom( buffer, SocketFlags.None, ref remoteEP )) > 0 ) {
					// Make sure each Packet has its own memory to prevent corruption
					// Should prolly change to ArrayPool<byte>, but can't return a Memory<byte> :(
					Memory<byte> buf = new byte[c];
					buffer.AsSpan(0, c).CopyTo(buf.Span);
					var p = new Packet((IPEndPoint)remoteEP, buf);
					incomingPackets.Enqueue( p );

					remoteEP = new IPEndPoint(IPAddress.Any, 0);
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
				
				Thread.Sleep( 1 );
			}
		}

		

		private void SendThread() {
			while( outgoingPackets == null )
				Thread.Sleep( 10 );

			int c;
			while( true ) {
				while( outgoingPackets.TryDequeue( out Packet p ) ) {
					c = serverSocket.SendTo( p.PacketData.ToArray(), p.RemoteEndpoint );
					//Logger.Verbose( "<- sent {0}/{1} bytes.", c, p.PacketData.Length );
					//Logger.Verbose("<  {0}", BitConverter.ToString(p.PacketData.ToArray()).Replace("-", " "));
				}

				Thread.Sleep( 1 );
			}
		}

		public void SendBE<T>( T pkt, IPEndPoint ep ) where T : struct {
			Send(Utils.WriteStructBE(pkt), ep);
		}

		public void Send<T>( T pkt, IPEndPoint ep ) where T : struct {
			Send(Utils.WriteStruct(pkt), ep);
		}

		public void Send( Memory<byte> p, IPEndPoint ep ) {
			var pkt = new Packet(ep, p);
			outgoingPackets.Enqueue( pkt );
		}
	}
}
