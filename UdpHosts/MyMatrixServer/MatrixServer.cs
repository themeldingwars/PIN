using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

using Shared.Udp;
using MyMatrixServer.Packets;
using System.Linq;
using System.Threading.Tasks;

namespace MyMatrixServer {
	class MatrixServer : PacketServer {
		public MatrixServer( ushort port ) : base(port) {
		}

		protected override void Startup() { }
		protected override bool Tick( double deltaTime, double currTime ) { return true; }
		protected override void NetworkTick( double deltaTime, double currTime ) { }
		protected override void Shutdown() { }

		protected unsafe override async Task HandlePacket(Packet packet) {
			ReadOnlyMemory<byte> mem = packet.PacketData;
			var SocketID = Utils.ReadStruct<uint>(mem);
			if( SocketID != 0 )
				return;

			//mem = mem.Slice(4);
			Program.Logger.Verbose("[MATRIX] "+packet.RemoteEndpoint.ToString()+" sent "+packet.PacketData.Length.ToString()+" bytes.");

			var matrixPkt = Utils.ReadStruct<MatrixPacketBase>(mem);

			switch( matrixPkt.Type ) {
			case "POKE":    // POKE
				var poke = Utils.ReadStruct<MatrixPacketPoke>(mem);
				Program.Logger.Verbose( "[POKE]" );
				var socketID = GenerateSocketID();
				Program.Logger.Information("Assigning SocketID [" + socketID.ToString()+"] to ["+packet.RemoteEndpoint.ToString()+"]");
				Send( new MatrixPacketHehe(socketID), packet.RemoteEndpoint );
				break;
			case "KISS":    // KISS
				var kiss = Utils.ReadStruct<MatrixPacketKiss>(mem);
				Program.Logger.Verbose( "[KISS]" );
				Send( new MatrixPacketHugg( 1, 25001 ), packet.RemoteEndpoint );
				break;
			case "ABRT":    // ABRT
				var abrt = Utils.ReadStruct<MatrixPacketAbrt>(mem);
				Program.Logger.Verbose( "[ABRT]" );
				break;
			default:
				Program.Logger.Error( "Unknown Matrix Packet Type: "+ matrixPkt.Type );
				return;
			}
		}

		protected Random random = new Random();
		protected uint GenerateSocketID() {
			return unchecked( (uint)((0xff00ff << 8) | random.Next(0,256)) );
		}
	}
}
