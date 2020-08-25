using System;
using System.Collections.Generic;
using System.Text;

using MyGameServer.Packets;

namespace MyGameServer.Controllers {
	[ControllerID(Enums.GSS.Controllers.Generic)]
	public class Generic : Base {

		public override void Init( NetworkClient client ) {
			
		}

		// Example GSS Message Handler
		[MessageID((byte)Enums.GSS.Generic.Commands.ScheduleUpdateRequest)]
		public void ScheduleUpdateRequest( NetworkClient client, ulong EntityID, GamePacket packet) {
			// TODO: implement
		}
	}
}
