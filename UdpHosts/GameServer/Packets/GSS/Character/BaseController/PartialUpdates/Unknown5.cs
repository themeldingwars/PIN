using Shared.Udp;

namespace GameServer.Packets.GSS.Character.BaseController.PartialUpdates
{
    [PartialUpdate.PartialShadowFieldAttribute(0x62)]
    public class Unknown5
    {
        [Field]
        public uint LastUpdateTime;
    }
}