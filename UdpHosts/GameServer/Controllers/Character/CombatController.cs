using GameServer.Enums.GSS.Character;
using GameServer.Packets;

namespace GameServer.Controllers.Character;

[ControllerID(Enums.GSS.Controllers.Character_CombatController)]
public class CombatController : Base
{
    public override void Init(INetworkClient client, IPlayer player, IShard shard)
    {
        // TODO: Implement
    }

    [MessageID((byte)Commands.FireInputIgnored)]
    public void FireInputIgnored(INetworkClient client, IPlayer player, ulong entityID, GamePacket packet)
    {
        // TODO: Implement
    }

    [MessageID((byte)Commands.UseScope)]
    public void UseScope(INetworkClient client, IPlayer player, ulong entityID, GamePacket packet)
    {
        // TODO: Implement
    }
}