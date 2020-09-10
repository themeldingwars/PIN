using System;
using System.Collections.Generic;
using System.Text;

namespace MyGameServer.Controllers.Character {
	[ControllerID(Enums.GSS.Controllers.Character_BaseController)]
	public class BaseController : Base {
		public override void Init( INetworkClient client, IInstance inst ) {
			// TODO: Implement
			client.Channels[ChannelType.ReliableGss].SendGSSClass(Test.GSS.Character.BaseController.KeyFrame.Test(client.Player), client.Player.EntityID, msgEnumType: typeof(Enums.GSS.Character.Events));
		}

		[MessageID((byte)Enums.GSS.Character.Commands.FetchQueueInfo)]
		public void FetchQueueInfo( INetworkClient client, ulong EntityID, Packets.GamePacket packet ) {
			// TODO: Implement
		}
	}
}
