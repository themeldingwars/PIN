using System;
using System.Collections.Generic;
using System.Text;

namespace MyGameServer.Controllers.Character {
	[ControllerID(Enums.GSS.Controllers.Character_BaseController)]
	internal class BaseController : Base {
		public override void Init( INetworkClient client, IPlayer player, IInstance inst ) {
			// TODO: Implement
			client.NetChans[ChannelType.ReliableGss].SendGSSClass(Test.GSS.Character.BaseController.KeyFrame.Test(player), player.EntityID, msgEnumType: typeof(Enums.GSS.Character.Events));
		}

		[MessageID((byte)Enums.GSS.Character.Commands.FetchQueueInfo)]
		public void FetchQueueInfo( INetworkClient client, IPlayer player, ulong EntityID, Packets.GamePacket packet ) {
			// TODO: Implement
		}
	}
}
