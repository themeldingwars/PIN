using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace MyGameServer.old {
	class GameServer {
		private readonly PacketServer Parent;
		public GameServer( PacketServer p ) {
			Parent = p;
		}

		[StructLayout( LayoutKind.Sequential, Pack = 1 )]
		public readonly struct GamePacketHeader {
			public readonly UInt16 PacketHeader;

			public byte Channel { get { return (byte)(PacketHeader >> 14); } }
			public byte ResendCount { get { return (byte)((PacketHeader >> 12) & 0b11); } }
			public bool IsSplit { get { return ((PacketHeader >> 11) & 0b1) == 1; } }
			public UInt16 Length { get { return (UInt16)(PacketHeader & 0b111_11111111); } }

			public GamePacketHeader( byte channel, byte resendCount, bool isSplit, UInt16 len ) {
				PacketHeader = (UInt16)(((channel & 0b11) << 14) |
										((resendCount & 0b11) << 12) |
										((isSplit ? 1 : 0) << 11) |
										(len & 0b111_11111111));
			}
		}
		[StructLayout( LayoutKind.Sequential, Pack = 1 )]
		readonly struct GamePacketBase {
			public readonly UInt32 SocketID;
			public readonly GamePacketHeader PacketHeader;
		}

		public void HandlePacket( PacketServer.Packet packet ) {
			Console.WriteLine( "[GAME] " + packet.RemoteEndpoint.ToString() + " sent " + packet.PacketData.Length.ToString() + " bytes." );

			int packetSize = 0;
			var gamePkt = Utils.ReadStruct<GamePacketBase>(packet.PacketData, ref packetSize);
			if( gamePkt == null )
				return;

			// TODO: Handle split and/or out of order packets
			if( packet.PacketData.Length < packetSize + gamePkt?.PacketHeader.Length )
				return;

			var dataMem = packet.PacketData.Slice(packetSize, (int)gamePkt?.PacketHeader.Length);
			packetSize += (int)gamePkt?.PacketHeader.Length;

			Console.WriteLine( "[GAME] " + packetSize.ToString() + " bytes" );
		}
	}
}
