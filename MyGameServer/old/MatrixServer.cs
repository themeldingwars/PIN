using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

namespace MyGameServer.old {
	class MatrixServer {
		private readonly PacketServer Parent;
		public MatrixServer( PacketServer p ) {
			Parent = p;
		}

		[StructLayout( LayoutKind.Sequential, Pack = 1 )]
		readonly struct MatrixPacketBase {
			public readonly UInt32 SocketID;
			public readonly UInt32 Type;
		}

		[StructLayout( LayoutKind.Sequential, Pack = 1 )]
		readonly struct MatrixPacketPoke {
			public readonly UInt32 SocketID;
			public readonly UInt32 Type;
			public readonly UInt32 ProtocolVersion;
		}
		[StructLayout( LayoutKind.Sequential, Pack = 1 )]
		readonly struct MatrixPacketHehe {
			public readonly UInt32 SocketID;
			public readonly UInt32 Type;
			public readonly UInt32 ClientSocketID;

			public MatrixPacketHehe( UInt32 id, UInt32 clientID ) {
				SocketID = id;
				Type = 0x45484548;
				ClientSocketID = clientID;
			}
		}
		[StructLayout( LayoutKind.Sequential, Pack = 1 )]
		readonly struct MatrixPacketKiss {
			public readonly UInt32 SocketID;
			public readonly UInt32 Type;
			public readonly UInt32 ProtocolVersion;
			public readonly UInt16 StreamingProtocolVersion;
		}
		[StructLayout( LayoutKind.Sequential, Pack = 1 )]
		readonly struct MatrixPacketHugg {
			public readonly UInt32 SocketID;
			public readonly UInt32 Type;
			public readonly UInt16 GameServerPort;
			public readonly UInt16 SequenceStart;

			public MatrixPacketHugg( UInt32 id, UInt16 port, UInt16 seqStart ) {
				SocketID = id;
				Type = 0x47475548;
				GameServerPort = port;
				SequenceStart = seqStart;
			}
		}
		[StructLayout( LayoutKind.Sequential, Pack = 1 )]
		readonly struct MatrixPacketAbrt {
			public readonly UInt32 SocketID;
			public readonly UInt32 Type;
		}

		public void HandlePacket(PacketServer.Packet packet) {
			Console.WriteLine("[MATRIX] "+packet.RemoteEndpoint.ToString()+" sent "+packet.PacketData.Length.ToString()+" bytes.");

			var matrixPkt = Utils.ReadStruct<MatrixPacketBase>(packet.PacketData);

			switch( matrixPkt?.Type ) {
			case 0x454B4F50:    // POKE
				var poke = Utils.ReadStruct<MatrixPacketPoke>(packet.PacketData);
				if( poke == null )
					return;
				Console.WriteLine( "[POKE]" );
				Parent.Send( new MatrixPacketHehe( 0, GenerateSocketID() ), packet.RemoteEndpoint );
				break;
			case 0x5353494B:    // KISS
				var kiss = Utils.ReadStruct<MatrixPacketKiss>(packet.PacketData);
				if( kiss == null )
					return;
				Console.WriteLine( "[KISS]" );
				Parent.Send( new MatrixPacketHugg( 0, 25001, 1 ), packet.RemoteEndpoint );
				break;
			case 0x53524241:    // ABRT
				var abrt = Utils.ReadStruct<MatrixPacketAbrt>(packet.PacketData);
				if( abrt == null )
					return;
				Console.WriteLine( "[ABRT]" );
				break;
			default:
				Console.WriteLine( "Unknown Matrix Packet Type: " + matrixPkt?.Type.ToString() );
				return;
			}
		}

		protected uint GenerateSocketID() {
			return 0x11223344;
		}
	}
}
