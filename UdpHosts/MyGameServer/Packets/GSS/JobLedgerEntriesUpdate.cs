using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace MyGameServer.Packets.GSS {
	[GSSMessage(Enums.GSS.Controllers.Generic, (byte)Enums.GSS.Generic.Events.JobLedgerEntriesUpdate)]
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct JobLedgerEntriesUpdate {
		public byte Unk1;
	}
}
