using System;
using System.Collections.Generic;
using System.Text;

namespace MyGameServer.Data {
	public class Zone {
		public uint ID { get; set; }
		public string Name { get; set; }
		public ulong Timestamp { get; set; }
		public Dictionary<string, System.Numerics.Vector3> POIs { get; protected set; }

		public Zone() {
			POIs = new Dictionary<string, System.Numerics.Vector3>();
		}
	}
}
