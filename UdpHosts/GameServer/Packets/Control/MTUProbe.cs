using GameServer.Enums;
using System.Runtime.InteropServices;

namespace GameServer.Packets.Control;

[ControlMessage(ControlPacketType.MTUProbe)]
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct MTUProbe
{
}