using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace MyGameServer.Packets.GSS {
	[GSSMessage(Enums.GSS.Controllers.Generic, (byte)Enums.GSS.Generic.Events.TotalAchievementPoints)]
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct TotalAchievementPoints {
		public uint Number;
	}
}
