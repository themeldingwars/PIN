using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Shared.Udp {
	public struct Packet {
		public readonly IPEndPoint RemoteEndpoint;
		public readonly ReadOnlyMemory<byte> PacketData;
		public int CurrentPosition { get; private set; }
		public int TotalBytes { get { return PacketData.Length; } }
		public int BytesRemaining { get { return TotalBytes - CurrentPosition; } }
		public DateTime Recieved { get; set; }

		public Packet( IPEndPoint ep, ReadOnlyMemory<byte> data, DateTime? recvd = null ) {
			RemoteEndpoint = ep;
			PacketData = data;
			CurrentPosition = 0;
			Recieved = recvd == null ? DateTime.Now : recvd.Value;
		}

		/*public T ReadBE<T>() where T : struct {
			var len = Unsafe.SizeOf<T>();
			var p = CurrentPosition;
			CurrentPosition += len;

			return Utils.ReadStructBE<T>(PacketData.Slice(p, len));
		}*/

		public T Read<T>() {
			var p = CurrentPosition;
			var data = PacketData.Slice(CurrentPosition);

			var ret = Utils.Read<T>(ref data);
			CurrentPosition = PacketData.Length - data.Length;

			return ret;
		}

		public ReadOnlyMemory<byte> Read(int len) {
			var p = CurrentPosition;
			CurrentPosition += len;

			return PacketData.Slice(p, len);
		}

		public T Peek<T>() {
			var dis = PacketData.Slice(CurrentPosition);
			return Utils.Read<T>(ref dis);
		}
		public ReadOnlyMemory<byte> Peek( int len ) {
			return PacketData.Slice(CurrentPosition, len);
		}

		public void Skip(int len) {
			CurrentPosition += len;
		}

		public void Reset() {
			CurrentPosition = 0;
		}
	}
}
