using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;

using Shared.Udp;

namespace MyGameServer.Packets {
	public struct GamePacket {
		public readonly GamePacketHeader Header;
		public readonly ReadOnlyMemory<byte> PacketData;
		public int CurrentPosition { get; private set; }
		public int TotalBytes { get { return PacketData.Length; } }
		public int BytesRemaining { get { return TotalBytes - CurrentPosition; } }
		public DateTime Recieved { get; set; }

		public GamePacket( GamePacketHeader hdr, ReadOnlyMemory<byte> data, DateTime? recvd = null ) {
			Header = hdr;
			PacketData = data;
			CurrentPosition = 0;
			Recieved = recvd == null ? DateTime.Now : recvd.Value;
		}

		public T Read<T>() {

			var buf = PacketData.Slice(CurrentPosition);
			var ret = Utils.Read<T>(ref buf);

			CurrentPosition = (TotalBytes - buf.Length);

			return ret;
		}

		public ReadOnlyMemory<byte> Read( int len ) {
			var p = CurrentPosition;
			CurrentPosition += len;

			return PacketData.Slice(p, len);
		}

		public T Peek<T>() where T : struct {
			var buf = PacketData.Slice(CurrentPosition);
			return Utils.Read<T>(ref buf);
		}
		public ReadOnlyMemory<byte> Peek( int len ) {
			return PacketData.Slice(CurrentPosition, len);
		}

		public void Skip( int len ) {
			CurrentPosition += len;
		}

		public void Reset() {
			CurrentPosition = 0;
		}
	}
}
