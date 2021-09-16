using Shared.Udp;

namespace MyGameServer.Packets.GSS.Character.BaseController.PartialUpdates
{
    [PartialUpdate.PartialShadowFieldAttribute(0x13)]
    public class CurrentHealth
    {
        [Field] public uint Value;
    }
}