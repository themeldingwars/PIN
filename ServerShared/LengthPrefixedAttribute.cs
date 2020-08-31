using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace ServerShared {
	public sealed class LengthPrefixedAttribute : Attribute {
		public Type LengthType;

		public LengthPrefixedAttribute( Type t ) {
			LengthType = t;
		}
	}
}
