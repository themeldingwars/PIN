using System;
using System.Collections.Generic;
using System.Text;

using MyGameServer.Packets;

namespace MyGameServer.Controllers {
	[ControllerID(Enums.GSS.Controllers.Generic2)]
	public class Generic2 : Base {

		public override void Init( INetworkClient client, IPlayer player, IShard shard ) {

		}

		[MessageID((byte)Enums.GSS.Generic.Commands.RequestLogout)]
		public void RequestLogout( INetworkClient client, IPlayer player, ulong EntityID, Packets.GamePacket packet ) {
		}
	}
}
