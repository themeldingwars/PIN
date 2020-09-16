using System;
using System.Collections.Generic;
using System.Text;

using MyGameServer.Packets;

namespace MyGameServer.Controllers {
	[ControllerID(Enums.GSS.Controllers.Generic)]
	public class Generic : Base {

		public override void Init( INetworkClient client, IPlayer player, IInstance inst ) {
			
		}

		// Example GSS Message Handler
		[MessageID((byte)Enums.GSS.Generic.Commands.ScheduleUpdateRequest)]
		public void ScheduleUpdateRequest( INetworkClient client, IPlayer player, ulong EntityID, GamePacket packet) {
			// TODO: implement
		}
	}
}
