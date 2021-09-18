using GameServer.Enums.GSS.Generic;
using System.Runtime.InteropServices;

namespace GameServer.Packets.GSS
{
    [GSSMessage(Enums.GSS.Controllers.Generic, (byte)Events.TotalAchievementPoints)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TotalAchievementPoints
    {
        public uint Number;
    }
}