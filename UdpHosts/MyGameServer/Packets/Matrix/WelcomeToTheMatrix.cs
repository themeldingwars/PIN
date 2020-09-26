using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

using Shared.Udp;

namespace MyGameServer.Packets.Matrix {
	[MatrixMessage(Enums.MatrixPacketType.WelcomeToTheMatrix)]
	public class WelcomeToTheMatrix {
		[Field]
		public ulong InstanceID;
		[Field]
		public ushort Unk1;
		[Field]
		public ushort Unk2;
	}
}
