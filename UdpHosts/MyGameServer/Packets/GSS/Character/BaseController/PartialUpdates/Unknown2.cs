using System;
using System.Collections.Generic;
using System.Text;

using Shared.Udp;

namespace MyGameServer.Packets.GSS.Character.BaseController.PartialUpdates {
    [PartialUpdate.PartialShadowField( 0x4f )]
    public class Unknown2 {
        [Field]
        public ushort Unk1;
        [Field]
        public ushort Unk2;
        [Field]
        public uint LastUpdateTime;
    }
}
