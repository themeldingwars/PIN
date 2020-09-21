using System;
using System.Collections.Generic;
using System.Text;

using MyGameServer.Packets;

namespace MyGameServer.Controllers.Character {
	[ControllerID(Enums.GSS.Controllers.Character_MissionAndMarkerController)]
	internal class MissionAndMarkerController : Base {
		public override void Init( INetworkClient client, IPlayer player, IShard shard ) {

		}

		[MessageID((byte)Enums.GSS.Character.Commands.RequestAllAchievements)]
		public void RequestAllAchievements( INetworkClient client, IPlayer player, ulong EntityID, Packets.GamePacket packet ) {
			// TODO: Implement
		}

		[MessageID((byte)Enums.GSS.Character.Commands.TryResumeTutorialChain)]
		public void TryResumeTutorialChain( INetworkClient client, IPlayer player, ulong EntityID, Packets.GamePacket packet ) {
			// TODO: Implement
		}
	}
}
