using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Shared.Udp.Packets {
	public enum BitEndianess {
		Unknown = 0,
		LittleEndian,
		LittleBigEndian,
		BigLittleEndian,
		BigEndian,
	}

	public class BasePacketView : IPacketView {
		public BitEndianess Endianess { get; protected set; }

		protected ConcurrentDictionary<string, object> fields;

		public BasePacketView( BitEndianess e ) {
			Endianess = e;

			if( Endianess == BitEndianess.LittleBigEndian || Endianess == BitEndianess.BigLittleEndian )
				throw new NotImplementedException("LittleBigEndian and BigLittleEndian NYI");
		}

		protected void SetupFields() {
			fields = new ConcurrentDictionary<string, object>();
		}

		public T Get<T>( Memory<byte> data, string name ) {
			return default(T);
		}

		public T Get<T>( Memory<byte> data, int offset, int length = -1 ) {
			return default(T);
		}
	}
}