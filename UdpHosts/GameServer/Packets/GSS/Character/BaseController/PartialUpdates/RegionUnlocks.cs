using System;
using System.Collections.Generic;
using System.Text;

using Shared.Udp;

namespace GameServer.Packets.GSS.Character.BaseController.PartialUpdates {
    [PartialUpdate.PartialShadowField( 0x27 )]
    public class RegionUnlocks {
        [Field]
        public ulong Bitfield;
    }
}
