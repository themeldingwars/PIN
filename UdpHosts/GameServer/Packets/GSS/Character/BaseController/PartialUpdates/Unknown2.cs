using Shared.Udp;

namespace GameServer.Packets.GSS.Character.BaseController.PartialUpdates
{
    [PartialUpdate.PartialShadowFieldAttribute(0x4f)]
    public class Unknown2
    {
        [Field]
        public ushort Unk1;

        [Field]
        public ushort Unk2;

        [Field]
        public uint LastUpdateTime;
    }
}