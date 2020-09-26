using System;
using System.Collections.Generic;
using System.Text;

using Shared.Udp;

namespace MyGameServer.Packets.GSS.Character.BaseController {
	[GSSMessage(Enums.GSS.Controllers.Character_BaseController, (byte)Enums.GSS.Character.Commands.MovementInput)]
	public class MovementInput {
		[Field]
		public ushort Key;
		[Field]
		public byte UnkByte1;
		[Field]
		public byte UnkByte2;
		[Field]
		public short UnkShort1;
		[Field]
		public short UnkShort2;
		[Field]
		public byte UnkByte3;
		[Field]
		public ushort Time;
		[Field]
		public byte UnkByte4;
		[Field]
		public byte UnkByte5;
		[Field]
		public Common.Vector Position;
		[Field]
		public Common.Quaternion Rotation;
		[Field]
		public ushort State;
		[Field]
		public Common.Vector Velocity;
		[Field]
		public Common.Vector AimDirection;
		[Field]
		public ushort UnkUShort1;
		[Field]
		public short UnkShort3;
		[Field]
		public short LastJumpTimer;
		[Field]
		public int UnkInt1;
	}
}