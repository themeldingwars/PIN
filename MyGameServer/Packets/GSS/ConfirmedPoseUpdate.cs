using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace MyGameServer.Packets.GSS {
	[GSSMessage(Enums.GSS.Controllers.Character_BaseController, (byte)Enums.GSS.Character.Events.ConfirmedPoseUpdate)]
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct ConfirmedPoseUpdate {
		public ushort Key;
		public ushort Type;
		private float vel_x;
		private float vel_y;
		private float vel_z;
		public ushort Unk1;
		public uint Unk2;
		public byte Unk3;
		public ushort KeyTime;

		public System.Numerics.Vector3 Velocity {
			get { return new System.Numerics.Vector3(vel_x, vel_y, vel_z); }
			set { vel_x = value.X; vel_y = value.Y; vel_z = value.Z; }
		}
	}
}
