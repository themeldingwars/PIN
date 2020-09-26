﻿using System;
using System.Collections.Generic;
using System.Text;

namespace MyGameServer.Test.GSS {
	public static class ConfirmedPoseUpdate {
		public static Packets.GSS.ConfirmedPoseUpdate GetPreSpawnConfirmedPoseUpdate() {
			var ret = new Packets.GSS.ConfirmedPoseUpdate();

			ret.Key = 0x2cda;
			ret.Type = 0;
			ret.Velocity = System.Numerics.Vector3.Zero;
			ret.Unk1 = 0;
			ret.Unk2 = 0;
			ret.Unk3 = 0;
			ret.KeyTime = 0x2d31;

			return ret;
		}
	}
}
