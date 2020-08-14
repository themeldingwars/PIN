using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace MyGameServer.Packets.Control {
	[ControlMessage(ControlPacketType.ReliableGSSAck)]
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public readonly struct ReliableGSSAck {
		public readonly ushort NextSeqNum;
		public readonly ushort AckFor;

		public ReliableGSSAck( ushort seqNum, ushort ackFor ) {
			NextSeqNum = seqNum;
			AckFor = ackFor;
		}
	}
}