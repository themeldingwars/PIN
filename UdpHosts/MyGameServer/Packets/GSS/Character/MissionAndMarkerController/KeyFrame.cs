using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

using Shared.Udp;

namespace MyGameServer.Packets.GSS.Character.MissionAndMarkerController {
	[GSSMessage( Enums.GSS.Controllers.Character_MissionAndMarkerController, (byte)Enums.GSS.Character.Events.KeyFrame )]
	public class KeyFrame {
		[Field]
		public ulong PlayerID;
		[Field]
		[Length(12)]
		public IList<byte> UnkBytes;

		public KeyFrame( IShard shard ) {
			UnkBytes = new List<byte> {
                0xff, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xff,
				0xff, 0xff, 0xff, 0xff,
			};
		}
	}
}
