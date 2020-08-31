using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

using ServerShared;

namespace MyGameServer.Packets.Matrix {
	[MatrixMessage(Enums.MatrixPacketType.SynchronizationRequest)]
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SynchronizationRequest : IWritable {
		public static SynchronizationRequest Parse( Packet packet ) {
			var ret = new SynchronizationRequest();

			ret.Unk = packet.Read(packet.BytesRemaining).ToArray();

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
