using GameServer.Enums.GSS.Generic;
using Shared.Udp;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace GameServer.Packets.GSS
{
    [GSSMessage(Enums.GSS.Controllers.Generic, (byte)Events.ArcCompletionHistoryUpdate)]
    public class ArcCompletionHistoryUpdate
    {
        [Field] [LengthPrefixed(typeof(byte))] public IList<Entry> Entries;

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Entry
        {
            public uint Unk1; // Sort? Position in GUI?
            public uint Unk2; // Extra?
            public uint Unk3; // Status?
            public uint Unk4; // Arc ID?

            public Entry(uint u1, uint u2, uint u3, uint u4)
            {
                Unk1 = u1;
                Unk2 = u2;
                Unk3 = u3;
                Unk4 = u4;
            }
        }
    }
}