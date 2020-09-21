using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

using Shared.Udp;

namespace MyGameServer.Packets.GSS.Character.BaseController {
	[GSSMessage( Enums.GSS.Controllers.Character_BaseController, (byte)Enums.GSS.Character.Events.CharacterLoaded )]
	public class CharacterLoaded {
		[Field]
		[Length(2)]
		public IList<byte> UnkBytes;

		public CharacterLoaded() {
			UnkBytes = new byte[] { 0x00, 0x00 };
		}
	}
}
