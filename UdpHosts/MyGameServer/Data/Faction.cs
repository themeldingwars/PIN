using System;
using System.Collections.Generic;
using System.Text;

namespace MyGameServer.Data {
	public class Faction {
		public static Faction Load(byte id) {
			return new Faction { ID = 1, Mode = 1 };
		}

		public byte ID { get; set; }
		public byte Mode { get; set; }

		public Faction() { }
	}
}
