using Shared.Udp;

namespace GameServer.Packets.GSS.Character.BaseController.PartialUpdates
{
    [PartialUpdate.PartialShadowFieldAttribute(0x10)]
    public class CharacterState
    {
        [Field] public uint LastUpdateTime;

        [Field] public byte State;
    }
}