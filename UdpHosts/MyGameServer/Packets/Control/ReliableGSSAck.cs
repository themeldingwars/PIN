using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

using Shared.Udp;

namespace MyGameServer.Packets.Control {
	[ControlMessage(Enums.ControlPacketType.ReliableGSSAck)]
	public class ReliableGSSAck {
		[Field]
		public ushort NextSeqNum;
		[Field]
		public ushort AckFor;
	}
}