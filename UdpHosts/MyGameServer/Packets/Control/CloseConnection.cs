using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace MyGameServer.Packets.Control {
	[ControlMessage(Enums.ControlPacketType.CloseConnection)]
	public class CloseConnection {
		public uint Unk1;
	}
}