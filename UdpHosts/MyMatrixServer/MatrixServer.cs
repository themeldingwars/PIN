using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

using Shared.Udp;
using MyMatrixServer.Packets;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace MyMatrixServer {
	class MatrixServer : PacketServer {
		public MatrixServer( ushort port ) : base(port) {
		}

		protected override void HandlePacket(Packet packet, CancellationToken ct ) {
			ReadOnlyMemory<byte> mem = packet.PacketData;
			var SocketID = Utils.ReadStruct<uint>(mem);
			if( SocketID != 0 )
				return;

			Program.Logger.Verbose("[MATRIX] "+packet.RemoteEndpoint.ToString()+" sent "+packet.PacketData.Length.ToString()+" bytes.");

			var matrixPkt = Utils.ReadStruct<MatrixPacketBase>(mem);

			switch( matrixPkt.Type ) {
			case "POKE":    // POKE
				var poke = Utils.ReadStruct<MatrixPacketPoke>(mem);
				Program.Logger.Verbose( "[POKE]" );
				var socketID = GenerateSocketID();
				Program.Logger.Information("Assigning SocketID [" + socketID.ToString()+"] to ["+packet.RemoteEndpoint.ToString()+"]");
				_ = Send( Utils.WriteStruct(new MatrixPacketHehe(socketID)), packet.RemoteEndpoint );
				break;
			case "KISS":    // KISS
				var kiss = Utils.ReadStruct<MatrixPacketKiss>(mem);
				Program.Logger.Verbose( "[KISS]" );
				_ = Send( Utils.WriteStruct( new MatrixPacketHugg( 1, 25001 )), packet.RemoteEndpoint );
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
