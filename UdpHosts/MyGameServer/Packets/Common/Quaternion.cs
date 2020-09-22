using System;
using System.Collections.Generic;
using System.Text;

using Shared.Udp;

namespace MyGameServer.Packets.Common {
	public class Quaternion {
		[Field]
		public float W;
		[Field]
		public float X;
		[Field]
		public float Y;
		[Field]
		public float Z;
	}
}
