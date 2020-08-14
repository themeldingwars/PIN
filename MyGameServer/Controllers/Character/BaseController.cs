using System;
using System.Collections.Generic;
using System.Text;

namespace MyGameServer.Controllers.Character {
	[ControllerID(Enums.GSS.Controllers.Character_BaseController)]
	public class BaseController : Base {
		[MessageID((byte)Enums.GSS.Character.Commands.FetchQueueInfo)]
		public void FetchQueueInfo( NetworkClient client, ulong EntityID, Packets.GamePacket packet ) {
			// TODO: Implement
		}
	}
}
