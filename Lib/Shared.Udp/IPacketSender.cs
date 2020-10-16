using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Udp {
	public interface IPacketSender {
		Task<bool> Send( Memory<byte> p, IPEndPoint ep );
	}
}
