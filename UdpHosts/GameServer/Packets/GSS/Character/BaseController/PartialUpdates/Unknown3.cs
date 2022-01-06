using Shared.Udp;

namespace GameServer.Packets.GSS.Character.BaseController.PartialUpdates;

[PartialUpdate.PartialShadowFieldAttribute(0x0b)]
public class Unknown3
{
    [Field]
    public uint Unk1;

    [Field]
    public uint LastUpdateTime;
}