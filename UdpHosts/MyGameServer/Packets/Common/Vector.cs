using System;
using System.Collections.Generic;
using System.Text;

using Shared.Udp;

namespace MyGameServer.Packets.Common {
	public class Vector {
		[Field]
		public float X;
		[Field]
		public float Y;
		[Field]
		public float Z;

		public static implicit operator System.Numerics.Vector3( Vector v ) {
			return new System.Numerics.Vector3( v.X, v.Y, v.Z );
		}

		public static implicit operator Vector( System.Numerics.Vector3 v ) {
			return new Vector { X = v.X, Y = v.Y, Z = v.Z };
		}
	}
}
