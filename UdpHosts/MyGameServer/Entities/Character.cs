using System;
using System.Collections.Generic;
using System.Text;

namespace MyGameServer.Entities {
	public class Character {
		public Data.Character CharData { get; set; }
		public System.Numerics.Vector3 Position { get; set; }
		public Character() {
			Position = new System.Numerics.Vector3();
		}

		public void Load(ulong charID) {
			CharData = Data.Character.Load(charID);
		}
	}
}
