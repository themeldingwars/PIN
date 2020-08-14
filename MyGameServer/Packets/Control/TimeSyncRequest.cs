using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace MyGameServer.Packets.Control {
	[ControlMessage(ControlPacketType.TimeSyncRequest)]
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public readonly struct TimeSyncRequest {
		public readonly ulong ClientTime;

		public TimeSyncRequest(ulong clientTime) {
			ClientTime = clientTime;
		}
	}
}
