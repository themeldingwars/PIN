using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace MyGameServer.Packets.GSS {
	[GSSMessage(Enums.GSS.Controllers.Generic, (byte)Enums.GSS.Generic.Events.InteractableStatusChanged)]
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct InteractableStatusChanged {
		public uint Unk1;
		public uint Unk2;
		public uint Unk3;
		public ushort Unk4;
	}
}
