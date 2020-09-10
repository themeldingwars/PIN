using System;
using System.Collections.Generic;
using System.Text;

namespace MyGameServer.Data {
	public class AbilityModule {
		public static AbilityModule Load(uint id, byte slot) {
			return new AbilityModule { SdbID = id, SlotIDX = slot };
		}

		public uint SdbID { get; set; }
		public byte SlotIDX { get; set; }

		public AbilityModule() {

		}

		public static implicit operator Packets.GSS.Character.BaseController.KeyFrame.Ability( AbilityModule o ) {
			return new Packets.GSS.Character.BaseController.KeyFrame.Ability {
				Slot = o.SlotIDX,
				ID = o.SdbID,
				UnkByte1 = 0
			};
		}
	}
}
