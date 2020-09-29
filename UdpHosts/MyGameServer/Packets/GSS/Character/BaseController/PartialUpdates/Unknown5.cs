using System;
using System.Collections.Generic;
using System.Text;

using Shared.Udp;

namespace MyGameServer.Packets.GSS.Character.BaseController.PartialUpdates {
    [PartialUpdate.PartialShadowField( 0x62 )]
    public class Unknown5 {
        [Field]
        public uint LastUpdateTime;
    }
}
