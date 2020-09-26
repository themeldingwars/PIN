using System;
using System.Collections.Generic;
using System.Text;

namespace MyGameServer.Controllers.Character {
	[ControllerID(Enums.GSS.Controllers.Character)]
	public class Root : Base {
		public override void Init( INetworkClient client, IPlayer player, IShard shard ) {

		}
	}
}
