using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace MyGameServer.Packets.Matrix {
	[MatrixMessage(Enums.MatrixPacketType.WelcomeToTheMatrix)]
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public readonly struct WelcomeToTheMatrix {
		public readonly ulong InstanceID;
		public readonly ushort Unk1;
		public readonly ushort Unk2;

		public WelcomeToTheMatrix(ulong instanceID, ushort unk1, ushort unk2) {
			InstanceID = instanceID;
			Unk1 = unk1;
			Unk2 = unk2;
		}
	}
}
