using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace MyGameServer.Packets.Control {
	[ControlMessage(Enums.ControlPacketType.MatrixAck)]
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public readonly struct MatrixAck {
		public readonly ushort NextSeqNum;
		public readonly ushort AckFor;

		public MatrixAck(ushort seqNum, ushort ackFor) {
			NextSeqNum = seqNum;
			AckFor = ackFor;
		}
	}
}