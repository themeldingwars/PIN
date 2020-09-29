using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace MyGameServer.Packets {
	[StructLayout( LayoutKind.Sequential, Pack = 1, Size = 2 )]
	public readonly struct GamePacketHeader {
		public readonly UInt16 PacketHeader;

		public ChannelType Channel { get { return (ChannelType)(byte)(PacketHeader >> 14); } }
		public byte ResendCount { get { return (byte)((PacketHeader >> 12) & 0b11); } }
		public bool IsSplit { get { return ((PacketHeader >> 11) & 0b1) == 1; } }
		public UInt16 Length { get { return (UInt16)(PacketHeader & 0x7ff); } }

		public GamePacketHeader( ChannelType channel, byte resendCount, bool isSplit, UInt16 len ) {
			PacketHeader = (UInt16)(((((byte)channel) & 0b11) << 14) |
									((resendCount & 0b11) << 12) |
									((isSplit ? 1 : 0) << 11) |
									(len & 0x07FF));
		}
	}
}
