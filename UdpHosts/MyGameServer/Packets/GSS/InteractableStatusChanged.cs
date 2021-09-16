using MyGameServer.Enums.GSS.Generic;
using System.Runtime.InteropServices;

namespace MyGameServer.Packets.GSS
{
    [GSSMessage(Enums.GSS.Controllers.Generic, (byte)Events.InteractableStatusChanged)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct InteractableStatusChanged
    {
        public uint Unk1;
        public uint Unk2;
        public uint Unk3;
        public ushort Unk4;
    }
}