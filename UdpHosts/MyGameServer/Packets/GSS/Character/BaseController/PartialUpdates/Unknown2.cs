using Shared.Udp;

namespace MyGameServer.Packets.GSS.Character.BaseController.PartialUpdates
{
    [PartialUpdate.PartialShadowFieldAttribute(0x4f)]
    public class Unknown2
    {
        [Field] public uint LastUpdateTime;

        [Field] public ushort Unk1;

        [Field] public ushort Unk2;
    }
}