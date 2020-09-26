using System;
using System.Collections.Generic;
using System.Text;

namespace MyGameServer.Data {
	public class Army {
		public static Army Load(ulong guid) {
			return new Army { GUID = guid, Name = "[ARMY]" };
		}

		public ulong GUID { get; set; }
		public string Name { get; set; }

		public Army() {

		}
	}
}
