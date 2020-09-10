using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Udp {
	public sealed class PaddingAttribute : Attribute {
		public int Size { get; private set; }
		public PaddingAttribute(int size) {
			Size = size;
		}
	}
}
