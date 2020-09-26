using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

using Shared.Udp;

namespace MyGameServer.Packets.Matrix {
	[MatrixMessage(Enums.MatrixPacketType.MatrixStatus)]
	public class MatrixStatus {
		[Field]
		[Length(16)]
		public IList<byte> Unk;

		public MatrixStatus() {
			Unk = new byte[] {
				0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00,
			}.ToList();
		}
	}
}
