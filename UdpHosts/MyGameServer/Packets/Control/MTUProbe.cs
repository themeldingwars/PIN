using MyGameServer.Enums;
using System.Runtime.InteropServices;

namespace MyGameServer.Packets.Control
{
    [ControlMessage(ControlPacketType.MTUProbe)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly struct MTUProbe
    {
    }
}