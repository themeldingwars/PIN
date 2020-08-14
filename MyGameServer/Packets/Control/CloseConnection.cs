using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace MyGameServer.Packets.Control {
	[ControlMessage(ControlPacketType.CloseConnection)]
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public readonly struct CloseConnection {
		//public readonly uint Unk1;
		//public readonly ushort Unk2;
	}
}