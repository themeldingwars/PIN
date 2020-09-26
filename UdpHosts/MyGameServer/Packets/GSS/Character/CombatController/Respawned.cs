using System;
using System.Collections.Generic;
using System.Text;

using Shared.Udp;

namespace MyGameServer.Packets.GSS.Character.CombatController {
    [GSSMessage( Enums.GSS.Controllers.Character_CombatController, (byte)Enums.GSS.Character.Events.Respawned )]
    public class Respawned {
        [Field]
        public uint LastUpdateTime;
        [Field]
        public ushort Unk1;
    }
}
