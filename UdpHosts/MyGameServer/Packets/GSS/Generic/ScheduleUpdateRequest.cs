using System;
using System.Collections.Generic;
using System.Text;

using Shared.Udp;

namespace MyGameServer.Packets.GSS.Generic {
	[GSSMessage(Enums.GSS.Controllers.Generic, (byte)Enums.GSS.Generic.Commands.ScheduleUpdateRequest)]
	internal class ScheduleUpdateRequest {
		[Field]
		public uint requestClientTime;
	}
}
