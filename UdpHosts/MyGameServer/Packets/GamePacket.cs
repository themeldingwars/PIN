using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;

using Shared.Udp;

namespace MyGameServer.Packets {
	public struct GamePacket {
		public readonly GamePacketHeader Header;
		public readonly Memory<byte> PacketData;
		public int CurrentPosition { get; private set; }
		public int TotalBytes { get { return PacketData.Length; } }
		public int BytesRemaining { get { return TotalBytes - CurrentPosition; } }

		public GamePacket( GamePacketHeader hdr, Memory<byte> data ) {
			Header = hdr;
			PacketData = data;
			CurrentPosition = 0;
		}

		public T ReadBE<T>() where T : struct {
			var len = Unsafe.SizeOf<T>();
			var p = CurrentPosition;
			CurrentPosition += len;

			return Utils.ReadStructBE<T>(PacketData.Slice(p, len));
		}

		public T Read<T>() {

			var buf = PacketData.Slice(CurrentPosition);
			var ret = Utils.Read<T>(ref buf);

			CurrentPosition = (TotalBytes - buf.Length);

			return ret;
		}

		public Memory<byte> Read( int len ) {
			var p = CurrentPosition;
			CurrentPosition += len;

			return PacketData.Slice(p, len);
		}

		public T Peek<T>() where T : struct {
			var buf = PacketData.Slice(CurrentPosition);
			return Utils.Read<T>(ref buf);
		}
		public Memory<byte> Peek( int len ) {
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
