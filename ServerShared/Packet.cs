using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace ServerShared {
	public struct Packet {
		public readonly IPEndPoint RemoteEndpoint;
		public readonly Memory<byte> PacketData;
		public int CurrentPosition { get; private set; }
		public int TotalBytes { get { return PacketData.Length; } }
		public int BytesRemaining { get { return TotalBytes - CurrentPosition; } }

		public Packet( IPEndPoint ep, Memory<byte> data ) {
			RemoteEndpoint = ep;
			PacketData = data;
			CurrentPosition = 0;
		}

		public T ReadBE<T>() where T : struct {
			var len = Unsafe.SizeOf<T>();
			var p = CurrentPosition;
			CurrentPosition += len;

			return Utils.ReadStructBE<T>(PacketData.Slice(p, len));
		}

		public T Read<T>() where T : struct {
			if( Utils.TryParseStruct<T>(this, out T pkt) )
				return pkt;

			var len = Unsafe.SizeOf<T>();
			var p = CurrentPosition;
			CurrentPosition += len;

			return Utils.ReadStruct<T>(PacketData.Slice(p, len));
		}

		public Memory<byte> Read(int len) {
			var p = CurrentPosition;
			CurrentPosition += len;

			return PacketData.Slice(p, len);
		}

		public T Peek<T>() where T : struct {
			if( Utils.TryParseStruct<T>(this, out T pkt) )
				return pkt;

			var len = Unsafe.SizeOf<T>();

			return Utils.ReadStruct<T>(PacketData.Slice(CurrentPosition, len));
		}
		public Memory<byte> Peek( int len ) {
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
