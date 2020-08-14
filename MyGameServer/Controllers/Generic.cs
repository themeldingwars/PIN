using System;
using System.Collections.Generic;
using System.Text;

using MyGameServer.Packets;

namespace MyGameServer.Controllers {
	[ControllerID(Packets.GSS.Controllers.Generic)]
	public class Generic : BaseController {

		// Example GSS Message Handler
		[MessageID((byte)Packets.GSS.GenericCommands.ScheduleUpdateRequest)]
		public void ScheduleUpdateRequest( NetworkClient client, ulong EntityID, GamePacket packet) {
			// TODO: implement
		}
	}
}
