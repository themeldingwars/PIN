using Shared.Udp;

namespace GameServer.Packets.GSS.Character.BaseController.PartialUpdates
{
    [PartialUpdate.PartialShadowFieldAttribute(0x14)]
    public class Unknown4
    {
        [Field] public uint Unk1;
    }
}