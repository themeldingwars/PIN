using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

using Shared.Udp;

namespace MyGameServer.Packets.Matrix {
	[MatrixMessage(Enums.MatrixPacketType.MatrixStatus)]
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct MatrixStatus : IWritable {
		public static MatrixStatus Parse( Packet packet ) {
			var ret = new MatrixStatus();

			ret.Unk = packet.Read(16).ToArray();

			return ret;
		}

		public byte[] Unk;

		public Memory<byte> Write() {
			Memory<byte> mem = new byte[Unk.Length];

			Unk.CopyTo(mem);

			return mem;
		}

		public Memory<byte> WriteBE() {
			return Write();
		}
	}
}
