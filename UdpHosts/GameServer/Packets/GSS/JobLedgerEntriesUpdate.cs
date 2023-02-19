using GameServer.Enums.GSS.Generic;
using System.Runtime.InteropServices;

namespace GameServer.Packets.GSS;

[GSSMessage(Enums.GSS.Controllers.Generic, (byte)Events.JobLedgerEntriesUpdate)]
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct JobLedgerEntriesUpdate
{
    public byte Unk1;
}