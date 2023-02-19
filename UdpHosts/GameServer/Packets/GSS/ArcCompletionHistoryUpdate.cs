using GameServer.Enums.GSS.Generic;
using Shared.Udp;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace GameServer.Packets.GSS;

[GSSMessage(Enums.GSS.Controllers.Generic, (byte)Events.ArcCompletionHistoryUpdate)]
public class ArcCompletionHistoryUpdate
{
    [Field]
    [LengthPrefixed(typeof(byte))]
    public IList<Entry> Entries;

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Entry
    {
        public uint Unknown1; // Sort? Position in GUI?
        public uint Unknown2; // Extra?
        public uint Unknown3; // Status?
        public uint Unknown4; // Arc ID?

        public Entry(uint u1, uint u2, uint u3, uint u4)
        {
            Unknown1 = u1;
            Unknown2 = u2;
            Unknown3 = u3;
            Unknown4 = u4;
        }
    }
}