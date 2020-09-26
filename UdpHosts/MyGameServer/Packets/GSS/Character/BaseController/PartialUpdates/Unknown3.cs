using System;
using System.Collections.Generic;
using System.Text;

using Shared.Udp;

namespace MyGameServer.Packets.GSS.Character.BaseController.PartialUpdates {
    [PartialUpdate.PartialShadowField( 0x0b )]
    public class Unknown3 {
        [Field]
        public uint Unk1;
        [Field]
        public uint LastUpdateTime;
    }
}
