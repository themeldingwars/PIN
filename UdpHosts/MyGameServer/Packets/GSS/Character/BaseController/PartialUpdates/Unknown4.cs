using System;
using System.Collections.Generic;
using System.Text;

using Shared.Udp;

namespace MyGameServer.Packets.GSS.Character.BaseController.PartialUpdates {
    [PartialUpdate.PartialShadowField( 0x14 )]
    public class Unknown4 {
        [Field]
        public uint Unk1;
    }
}
