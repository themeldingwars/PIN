using System;
using System.Collections.Generic;
using System.Text;

namespace ServerShared {
	public interface IWritable {
		Memory<byte> Write();
		Memory<byte> WriteBE();
	}
}
