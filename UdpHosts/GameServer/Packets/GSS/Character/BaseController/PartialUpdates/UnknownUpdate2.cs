using Shared.Udp;

namespace GameServer.Packets.GSS.Character.BaseController.PartialUpdates;

[PartialUpdate.PartialShadowFieldAttribute(0x4f)]
public class UnknownUpdate2
{
    [Field]
    public ushort Unknown1;

    [Field]
    public ushort Unknown2;

    [Field]
    public uint LastUpdateTime;
}