using Shared.Udp;

namespace MyGameServer.Packets.GSS.Character.BaseController.PartialUpdates
{
    [PartialUpdate.PartialShadowFieldAttribute(0x0b)]
    public class Unknown3
    {
        [Field] public uint LastUpdateTime;

        [Field] public uint Unk1;
    }
}