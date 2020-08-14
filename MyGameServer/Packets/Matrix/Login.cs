using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace MyGameServer.Packets.Matrix {
	[MatrixMessage(Enums.MatrixPacketType.Login)]
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct Login {
		public byte Unk1;
		public ushort ClientVersion;
		public fixed byte Unk2[3];
		public ulong CharacterGUID;
		public fixed byte Unk3[13];
		public fixed byte Red5Sig2[35]; // From Web Requests to ClientAPI
		public fixed byte Red5Sig1[370];    // From Web Requests to ClientAPI
	}
}
