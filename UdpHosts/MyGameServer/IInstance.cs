using System;
using System.Collections.Generic;
using System.Text;

namespace MyGameServer {
	public interface IInstance {
		ulong InstanceID { get; }
	}
}
