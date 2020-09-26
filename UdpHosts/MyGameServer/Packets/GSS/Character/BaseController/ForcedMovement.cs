using System;
using System.Collections.Generic;
using System.Text;

using Shared.Udp;

namespace MyGameServer.Packets.GSS.Character.BaseController {
    [GSSMessage( Enums.GSS.Controllers.Character_BaseController, (byte)Enums.GSS.Character.Events.ForcedMovement )]
    public class ForcedMovement {
        [Field]
        public ushort Type;
        [Field]
        public uint Unk1;
        [Field]
        public Common.Vector Position;
        [Field]
        public Common.Vector AimDirection;
        [Field]
        public Common.Vector Velocity;
        [Field]
        public uint GameTime;
        [Field]
        public ushort KeyFrame;
    }
}
