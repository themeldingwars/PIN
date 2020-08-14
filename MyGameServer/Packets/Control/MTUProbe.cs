using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace MyGameServer.Packets.Control {
	[ControlMessage(ControlPacketType.MTUProbe)]
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public readonly struct MTUProbe {
	}
}
