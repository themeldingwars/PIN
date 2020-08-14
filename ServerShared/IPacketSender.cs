using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace ServerShared {
	public interface IPacketSender {
		void Send( Memory<byte> p, IPEndPoint ep );
	}
}
