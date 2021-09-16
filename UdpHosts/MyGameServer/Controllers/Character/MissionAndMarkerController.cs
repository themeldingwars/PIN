using MyGameServer.Enums.GSS.Character;
using MyGameServer.Packets;

namespace MyGameServer.Controllers.Character
{
    [ControllerID(Enums.GSS.Controllers.Character_MissionAndMarkerController)]
    public class MissionAndMarkerController : Base
    {
        public override void Init(INetworkClient client, IPlayer player, IShard shard)
        {
        }

        [MessageID((byte)Commands.RequestAllAchievements)]
        public void RequestAllAchievements(INetworkClient client, IPlayer player, ulong EntityID, GamePacket packet)
        {
            // TODO: Implement
        }

        [MessageID((byte)Commands.TryResumeTutorialChain)]
        public void TryResumeTutorialChain(INetworkClient client, IPlayer player, ulong EntityID, GamePacket packet)
        {
            // TODO: Implement
        }
    }
}