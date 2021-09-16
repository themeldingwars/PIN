using MyGameServer.Enums;
using MyGameServer.Enums.GSS.Generic;
using MyGameServer.Packets;
using MyGameServer.Packets.Control;
using MyGameServer.Packets.GSS.Generic;
using System;

namespace MyGameServer.Controllers
{
    [ControllerID(Enums.GSS.Controllers.Generic)]
    public class Generic : Base
    {
        public override void Init(INetworkClient client, IPlayer player, IShard shard)
        {
        }

        [MessageID((byte)Commands.ScheduleUpdateRequest)]
        public void ScheduleUpdateRequest(INetworkClient client, IPlayer player, ulong EntityID, GamePacket packet)
        {
            var req = packet.Read<ScheduleUpdateRequest>();

            player.LastRequestedUpdate = client.AssignedShard.CurrentTime;
            player.RequestedClientTime = Math.Max(req.requestClientTime, player.RequestedClientTime);

            if (!player.FirstUpdateRequested)
            {
                player.FirstUpdateRequested = true;
                player.Respawn();
            }

            //Program.Logger.Error( "Update scheduled" );
        }

        [MessageID((byte)Commands.RequestLogout)]
        public void RequestLogout(INetworkClient client, IPlayer player, ulong EntityID, GamePacket packet)
        {
            var resp = new CloseConnection { Unk1 = 0 };
            client.NetChans[ChannelType.Control].SendClass(resp, typeof(ControlPacketType));
        }
    }
}