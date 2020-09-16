using System;
using System.Collections.Generic;
using System.Text;

using Shared.Udp;

namespace MyGameServer.Packets.Matrix {
	[MatrixMessage( Enums.MatrixPacketType.KeyframeRequest )]
	public class KeyFrameRequest {
        [Field]
        public byte HaveRequestByEntityID;
        [Field]
        [LengthPrefixed(typeof(byte))]
        public IList<RequestByEntity> EntityRequests;
        [Field]
        public byte HaveRequestByRefID;
        [Field]
        [LengthPrefixed(typeof(byte))]
        public IList<ushort> RefRequests;

        public class RequestByEntity {
            [Field]
            public byte ControllerID;
            [Field]
            [Length(7)]
            public byte[] EntityID;
            [Field]
            public ushort RefID;
            [Field]
            public byte Unk2;
            [Field]
            public uint Checksum;
        }
    }
}
