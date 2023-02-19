using GameServer.Enums.GSS.Generic;
using System.Runtime.InteropServices;

namespace GameServer.Packets.GSS;

[GSSMessage(Enums.GSS.Controllers.Generic, (byte)Events.InteractableStatusChanged)]
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct InteractableStatusChanged
{
    public uint Unknown1;
    public uint Unknown2;
    public uint Unknown3;
    public ushort Unknown4;
}