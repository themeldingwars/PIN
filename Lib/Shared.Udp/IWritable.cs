using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Udp {
	public interface IWritable {
		Memory<byte> Write();
		Memory<byte> WriteBE();
	}
}
