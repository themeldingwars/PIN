using System;
using System.Collections.Generic;
using System.Text;

namespace ServerShared {
	public sealed class LengthAttribute : Attribute {
		public int Length { get; private set; }

		public LengthAttribute(int len) {
			Length = len;
		}
	}
}
