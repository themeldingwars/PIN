using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace MyGameServer.old {
	static class Utils {
		public static Thread RunThread( ThreadStart action ) {
			var t = new Thread(action);
			t.Start();

			return t;
		}

		public static T? ReadStruct<T>( ReadOnlyMemory<byte> mem ) where T : struct {
			int size = Unsafe.SizeOf<T>();
			if( mem.Length < size )
				return null;

			return MemoryMarshal.Read<T>( mem.Span.Slice( 0, size ) );
		}

		public static T? ReadStruct<T>( ReadOnlyMemory<byte> mem, ref int size ) where T : struct {
			size = Unsafe.SizeOf<T>();
			if( mem.Length < size )
				return null;

			return MemoryMarshal.Read<T>( mem.Span.Slice( 0, size ) );
		}

		public static ReadOnlyMemory<byte> WriteStruct<T>( T pkt ) where T : struct {
			int size = Unsafe.SizeOf<T>();
			Span<byte> span = stackalloc byte[size];

			MemoryMarshal.Write<T>( span, ref pkt );

			byte[] mem = span.ToArray();
			return new ReadOnlyMemory<byte>( mem );
		}
	}
}
