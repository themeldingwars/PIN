using System;
using System.Collections.Generic;
using System.Text;

namespace ServerShared {
	public interface IWritableStruct {
		Memory<byte> Write();
		Memory<byte> WriteBE();
	}
}
