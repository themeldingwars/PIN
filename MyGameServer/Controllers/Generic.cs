using System;
using System.Collections.Generic;
using System.Text;

using MyGameServer.Packets;

namespace MyGameServer.Controllers {
	[ControllerID(Enums.GSS.Controllers.Generic)]
	public class Generic : BaseController {

		// Example GSS Message Handler
		[MessageID((byte)Enums.GSS.GenericCommands.ScheduleUpdateRequest)]
		public void ScheduleUpdateRequest( NetworkClient client, ulong EntityID, GamePacket packet) {
			// TODO: implement
		}
	}
}
