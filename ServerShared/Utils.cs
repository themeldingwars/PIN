using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace ServerShared {
	public static class Utils {
		public static Thread RunThread( ThreadStart action ) {
			var t = new Thread(action);
			t.Start();

			return t;
		}

		public unsafe static void WriteFixed(byte* dest, byte[] src) {
			for( var i = 0; i < src.Length; i++ )
				*(dest + i) = src[i];
		}

		public unsafe static void WriteFixed( byte* dest, Span<byte> src ) {
			for( var i = 0; i < src.Length; i++ )
				*(dest + i) = src[i];
		}

		public unsafe static string ReadFixedString(byte* src, int len) {
			return Encoding.ASCII.GetString( src, len );
		}

		public unsafe static byte[] ReadFixed( byte* src, int len ) {
			return new Span<byte>(src, len).ToArray();
		}

		public static T ReadStruct<T>( Memory<byte> mem ) where T : struct {
			int size = Unsafe.SizeOf<T>();
			if( mem.Length < size )
				return default(T);

			return MemoryMarshal.Read<T>( mem.Span.Slice( 0, size ) );
		}

		public static T ReadStruct<T>( Memory<byte> mem, out int size ) where T : struct {
			size = Unsafe.SizeOf<T>();
			if( mem.Length < size )
				return default(T);

			return MemoryMarshal.Read<T>( mem.Span.Slice( 0, size ) );
		}

		public static T ReadStructBE<T>( Memory<byte> mem ) where T : struct {
			int size = Unsafe.SizeOf<T>();
			if( mem.Length < size )
				return default(T);

			FixEndianness<T>(mem);

			return MemoryMarshal.Read<T>(mem.Span.Slice(0, size));
		}

		public static T ReadStructBE<T>( Memory<byte> mem, out int size ) where T : struct {
			size = Unsafe.SizeOf<T>();
			if( mem.Length < size )
				return default(T);

			FixEndianness<T>(mem);

			return MemoryMarshal.Read<T>(mem.Span.Slice(0, size));
		}

		public unsafe static T ReadStruct<T>( byte* mem, int len ) where T : struct {
			return ReadStruct<T>(new Memory<byte>(new Span<byte>(mem, 0).ToArray()));
		}

		private static Dictionary<Type, MethodInfo> _cacheParseMethods = new Dictionary<Type, MethodInfo>();
		public static bool TryParseStruct<T>( Packet packet, out T pkt ) where T : struct {
			pkt = default(T);

			if( !_cacheParseMethods.ContainsKey(typeof(T)) ) {
				var t = typeof(T).GetMethod("Parse", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

				_cacheParseMethods.Add(typeof(T), t);
			}

			if( _cacheParseMethods[typeof(T)] != null ) {
				try {
					pkt = (T)_cacheParseMethods[typeof(T)].Invoke(null, new object[] { packet });
					return true;
				} catch {
					return false;
				}
			}

			return false;
		}

		public unsafe static Memory<byte> WriteStruct<T>( T pkt ) where T : struct {
			if( pkt is IWritableStruct write ) {
				return write.Write();
			} else {
				int size = Unsafe.SizeOf<T>();
				Memory<byte> mem = new byte[size];

				MemoryMarshal.Write(mem.Span, ref pkt);

				return mem.Slice(0, size);
			}
		}

		public unsafe static void WriteStruct<T>( Memory<byte> mem, T pkt ) where T : struct {
			if( pkt is IWritableStruct write )
				write.Write().CopyTo(mem);
			else
				MemoryMarshal.Write(mem.Span, ref pkt);
		}

		public unsafe static Memory<byte> WriteStructBE<T>( T pkt ) where T : struct {
			if( pkt is IWritableStruct write ) {
				return write.WriteBE();
			} else {
				int size = Unsafe.SizeOf<T>();
				Memory<byte> mem = new byte[size];

				MemoryMarshal.Write(mem.Span, ref pkt);

				mem = mem.Slice(0, size);

				FixEndianness<T>(mem);

				return mem;
			}
		}

		public unsafe static void WriteStructBE<T>( Memory<byte> mem, T pkt ) where T : struct {
			if( pkt is IWritableStruct write )
				write.WriteBE().CopyTo(mem);
			else {
				MemoryMarshal.Write(mem.Span, ref pkt);

				FixEndianness<T>(mem);
			}
		}

		public unsafe static void FixEndianness<T>( ref T val ) where T : struct {
			var len = Unsafe.SizeOf<T>();
			byte* ptr = (byte*)Unsafe.AsPointer(ref val);

			for( var i = 0; i < len / 2; i++ )
				Swap(ref *(ptr + i), ref *((ptr + len) - i));
		}

		public static void Swap<T>(ref T a, ref T b) {
			T t = a;
			a = b;
			b = t;
		}

		// From: https://stackoverflow.com/a/15020402
		public static void FixEndianness<T>( Memory<byte> data, int startOffset = 0 ) where T : struct {
			FixEndianness( typeof( T ), data, startOffset );
		}
		public static void FixEndianness( Type type, Memory<byte> data, int startOffset = 0 ) {
			if( !BitConverter.IsLittleEndian )
				return;

			if( type.IsPrimitive ) {
				data.Slice(startOffset, Marshal.SizeOf(type)).Span.Reverse();
			} else if( type.IsEnum ) {
				type = Enum.GetUnderlyingType(type);
				data.Slice(startOffset, Marshal.SizeOf(type)).Span.Reverse();
			} else {
				foreach( var field in type.GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public) ) {
					var fieldType = field.FieldType;
					if( field.IsStatic ) // don't process static fields
						continue;

					if( fieldType == typeof(string) ) // don't swap bytes for strings
						continue;

					var offset = Marshal.OffsetOf(type, field.Name).ToInt32();

					if( fieldType.IsEnum ) // handle enums
						fieldType = Enum.GetUnderlyingType(fieldType);

					// check for sub-fields to recurse if necessary
					var subFields = fieldType.GetFields().Where(subField => subField.IsStatic == false).ToArray();

					var effectiveOffset = startOffset + offset;

					if( subFields.Length == 0 )
						data.Slice(effectiveOffset, Marshal.SizeOf(fieldType)).Span.Reverse();
					else
						FixEndianness(fieldType, data, effectiveOffset);
				}
			}
		}
	}
}
