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
		public const int MTU = 1400;

		public static ILogger Logger;


		protected readonly Socket serverSocket;
		protected readonly IPEndPoint listenEndpoint;
		protected BufferBlock<Packet?> incomingPackets = null;
		protected BufferBlock<Packet?> outgoingPackets = null;
		protected CancellationTokenSource source;

		public bool IsRunning { get; protected set; }
		



		public PacketServer( ushort port ) {
			listenEndpoint = new IPEndPoint( IPAddress.Any, port );
			serverSocket = new Socket( AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp );
		}


		protected virtual void HandleCommand( string line ) {
			if( line.Trim().StartsWith( "exit" ) ) {
				IsRunning = false;
				source.Cancel();
			}
		}
		protected abstract void HandlePacket( Packet p, CancellationToken ct );
		protected virtual void Startup( CancellationToken ct ) { }
		protected virtual async void ServerRunThread( CancellationToken ct ) {
			Packet? p;
			while( (p = await incomingPackets.ReceiveAsync( ct )) != null ) {
				HandlePacket( p.Value, ct );
			}
		}
		protected virtual void Shutdown( CancellationToken ct ) { }



		// TODO: Move to seperate thread? add console/rcon handling here?
		// FIXME: Move timing to GameServer
		public void Run() {
			source = new CancellationTokenSource();
			var ct = source.Token;

			incomingPackets = new BufferBlock<Packet?>();
			outgoingPackets = new BufferBlock<Packet?>();

			var listenThread = Utils.RunThread(ListenThread, ct);
			var runThread = Utils.RunThread(ServerRunThread, ct);
			var sendThread = Utils.RunThread(SendThread, ct);

			Startup(ct);

			IsRunning = true;

			while( IsRunning ) {
				// TODO: Handle Command
				var line = Console.ReadLine();
				HandleCommand( line );
            }

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
			
			byte[] buffer = new byte[MTU*10];
			EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
			int c;

			Thread.CurrentThread.Priority = ThreadPriority.Highest;

			while( true ) {
				if( ct.IsCancellationRequested )
					break;
				
				try {
					// Sockets don't support async yet :( Blocking here bc the win api will yield and wait better on the native side than we can here
					if( (c = serverSocket.ReceiveFrom(buffer, SocketFlags.None, ref remoteEP)) > 0 ) {
						// Should prolly change to ArrayPool<byte>, but can't return a Memory<byte> :(
						// TODO: Move Endpoint and Memory<byte> management to Packet (constructor + destructor)
						var buf = new byte[c];
						buffer.AsSpan().Slice(0, c).ToArray().CopyTo(buf, 0);
						_ = await incomingPackets.SendAsync(new Packet((IPEndPoint)remoteEP, new ReadOnlyMemory<byte>(buf, 0, c), DateTime.Now), ct);

						// Not 100% sure this needs to be cleared?
						remoteEP = new IPEndPoint(IPAddress.Any, 0);
					}
				} catch( Exception ex ) {
					Logger.Error(ex, "Error {0}", "listenThread");
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
