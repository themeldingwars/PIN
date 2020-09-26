using System;
using System.Collections.Generic;
using System.Text;

using Shared.Udp;

namespace MyGameServer.Packets.Matrix {
	[MatrixMessage( Enums.MatrixPacketType.KeyframeRequest )]
	public class KeyFrameRequest {
		[ExistsPrefix( typeof( byte ), 1 )]
		[Field]
		[LengthPrefixed(typeof(byte))]
		public IList<RequestByEntity> EntityRequests;
		[ExistsPrefix( typeof( byte ), 1 )]
		[Field]
		[LengthPrefixed(typeof(byte))]
		public IList<ushort> RefRequests;

		public class RequestByEntity {
			[Field]
			public byte ControllerID;
			[Field]
			[Length(7)]
			public IList<byte> EntityID;
			[Field]
			public ushort RefID;
			[Field]
			public byte Unk2;
			[Field]
			public uint Checksum;
		}
	}
}
