using System;
using System.Collections.Generic;
using System.Text;

using MyGameServer.Packets;
using MyGameServer.Packets.Control;

namespace MyGameServer.Controllers {
	[ControllerID(Enums.GSS.Controllers.Generic)]
	public class Generic : Base {

		public override void Init( INetworkClient client, IPlayer player, IShard shard ) {

		}

		[MessageID((byte)Enums.GSS.Generic.Commands.ScheduleUpdateRequest)]
		public void ScheduleUpdateRequest( INetworkClient client, IPlayer player, ulong EntityID, GamePacket packet ) {
			var req = packet.Read<Packets.GSS.Generic.ScheduleUpdateRequest>();

			player.LastRequestedUpdate = client.AssignedShard.CurrentTime;
			player.RequestedClientTime = Math.Max(req.requestClientTime, player.RequestedClientTime);

			if( !player.FirstUpdateRequested ) {
				player.FirstUpdateRequested = true;
				player.Respawn();
			}

			//Program.Logger.Error( "Update scheduled" );
		}

		[MessageID((byte)Enums.GSS.Generic.Commands.RequestLogout)]
		public void RequestLogout( INetworkClient client, IPlayer player, ulong EntityID, GamePacket packet ) {
			var resp = new CloseConnection {
				Unk1 = 0
			};
			client.NetChans[ChannelType.Control].SendClass(resp, msgEnumType: typeof(Enums.ControlPacketType));
		}
	}
}
