using System;
using System.Collections.Generic;
using System.Text;

using Shared.Udp;

namespace MyGameServer.Packets.GSS.Character.BaseController.PartialUpdates {
    [PartialUpdate.PartialShadowField( 0x10 )]
    public class CharacterState {
        [Field]
        public byte State;
        [Field]
        public uint LastUpdateTime;
    }
}
