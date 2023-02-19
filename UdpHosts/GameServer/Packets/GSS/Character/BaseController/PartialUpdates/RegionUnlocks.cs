using Shared.Udp;

namespace GameServer.Packets.GSS.Character.BaseController.PartialUpdates;

[PartialUpdate.PartialShadowFieldAttribute(0x27)]
public class RegionUnlocks
{
    [Field]
    public ulong Bitfield;
}