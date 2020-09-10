using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace MyGameServer.old {
	class PacketServer {
		public const int MTU = 1500;

		private readonly Socket serverSocket;
		private readonly IPEndPoint listenEndpoint;
		private ConcurrentQueue<Packet> incomingPackets = null;
		private ConcurrentQueue<Packet> outgoingPackets = null;

		private MatrixServer matrix;
		private GameServer game;
		private ConcurrentDictionary<uint,ClientConnection> Clients;

		public PacketServer() {
			listenEndpoint = new IPEndPoint( IPAddress.Loopback, 25000 );
			serverSocket = new Socket( AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp );

			matrix = new MatrixServer( this );
			game = new GameServer( this );
			Clients = new ConcurrentDictionary<uint, ClientConnection>();
		}

		public void AddClient(ClientConnection conn) {
			_ = Clients.TryAdd( conn.SocketID, conn );
		}

		public void Run() {
			var listenThread = Utils.RunThread(ListenThread);
			var readThread = Utils.RunThread(ReadThread);
			var sendThread = Utils.RunThread(SendThread);

			while( true ) {
				Thread.Sleep( 30000 );
				Console.WriteLine( "[HrtBt] 30 seconds passed. [" + ThreadPool.ThreadCount.ToString() + " threads, " + ThreadPool.PendingWorkItemCount.ToString() + " waiting, " + ThreadPool.CompletedWorkItemCount.ToString() + " completed]" );
			}
		}

		public readonly struct Packet {
			public readonly IPEndPoint RemoteEndpoint;
			public readonly ReadOnlyMemory<byte> PacketData;

			public Packet( IPEndPoint ep, ReadOnlyMemory<byte> data ) {
				RemoteEndpoint = ep;
				PacketData = data;
			}
		}

		private void ListenThread() {
			serverSocket.Blocking = true;
			serverSocket.ReceiveBufferSize = MTU * 100;
			serverSocket.SetSocketOption( SocketOptionLevel.IP, SocketOptionName.PacketInformation, true );
			serverSocket.Bind( listenEndpoint );

			Console.WriteLine( "Listening on " + listenEndpoint.ToString() );

			incomingPackets = new ConcurrentQueue<Packet>();
			outgoingPackets = new ConcurrentQueue<Packet>();
			byte[] buffer = new byte[MTU];
			int c;

			while( true ) {
				EndPoint remoteEP = new IPEndPoint( IPAddress.Any, 0 );

				if( (c = serverSocket.ReceiveFrom( buffer, SocketFlags.Partial, ref remoteEP )) > 0 ) {
					var p = new Packet((IPEndPoint)remoteEP, buffer.AsMemory().Slice(0,c));
					incomingPackets.Enqueue( p );
				}
			}
		}

		[StructLayout( LayoutKind.Sequential, Pack = 1 )]
		public readonly struct PacketBase {
			public readonly UInt32 SocketID;
		}

		private void ReadThread() {
			while( incomingPackets == null )
				Thread.Sleep( 1 );

			while( true ) {
				while( incomingPackets.TryDequeue( out Packet p ) ) {
					var pkt = Utils.ReadStruct<PacketBase>(p.PacketData);
					if( pkt != null ) {
						if( pkt?.SocketID == 0 ) {
							matrix.HandlePacket( p );
						} else {
							game.HandlePacket( p );
						}
					}
				}
				
				Thread.Sleep( 1 );
			}
		}

		private void SendThread() {
			while( outgoingPackets == null )
				Thread.Sleep( 1 );

			int c;

			while( true ) {
				while( outgoingPackets.TryDequeue( out Packet p ) ) {
					// TODO: Use MTU to split packets?
					c = serverSocket.SendTo( p.PacketData.ToArray(), p.RemoteEndpoint );
					Console.WriteLine("I sent "+c.ToString()+" bytes.");
				}

				Thread.Sleep( 1 );
			}
		}

		public void Send<T>( T p, IPEndPoint ep ) where T : struct {
			var pkt = new Packet(ep, Utils.WriteStruct(p));
			outgoingPackets.Enqueue( pkt );
		}
	}
}
