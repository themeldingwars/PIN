using MyGameServer.Enums.GSS.Generic;
using System.Runtime.InteropServices;

namespace MyGameServer.Packets.GSS
{
    [GSSMessage(Enums.GSS.Controllers.Generic, (byte)Events.TotalAchievementPoints)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TotalAchievementPoints
    {
        public uint Number;
    }
}