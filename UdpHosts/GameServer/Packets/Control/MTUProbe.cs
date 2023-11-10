using System.Runtime.InteropServices;
using GameServer.Enums;

namespace GameServer.Packets.Control;

[ControlMessage(ControlPacketType.MTUProbe)]
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct MTUProbe
{
}