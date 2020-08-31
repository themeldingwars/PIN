using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections;
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

		public unsafe static void WriteFixed( byte* dest, byte[] src ) {
			for( var i = 0; i < src.Length; i++ )
				*(dest + i) = src[i];
		}

		public unsafe static void WriteFixed( byte* dest, Span<byte> src ) {
			for( var i = 0; i < src.Length; i++ )
				*(dest + i) = src[i];
		}

		public unsafe static string ReadFixedString( byte* src, int len ) {
			return Encoding.ASCII.GetString( src, len );
		}

		public unsafe static byte[] ReadFixed( byte* src, int len ) {
			return new Span<byte>( src, len ).ToArray();
		}

		public static T ReadStruct<T>( Memory<byte> mem ) where T : struct {
			int size = Unsafe.SizeOf<T>();
			if( mem.Length < size )
				return default( T );

			return MemoryMarshal.Read<T>( mem.Span.Slice( 0, size ) );
		}

		public static T ReadStruct<T>( Memory<byte> mem, out int size ) where T : struct {
			size = Unsafe.SizeOf<T>();
			if( mem.Length < size )
				return default( T );

			return MemoryMarshal.Read<T>( mem.Span.Slice( 0, size ) );
		}

		public static T ReadStructBE<T>( Memory<byte> mem ) where T : struct {
			int size = Unsafe.SizeOf<T>();
			if( mem.Length < size )
				return default( T );

			FixEndianness<T>( mem );

			return MemoryMarshal.Read<T>( mem.Span.Slice( 0, size ) );
		}

		public static T ReadStructBE<T>( Memory<byte> mem, out int size ) where T : struct {
			size = Unsafe.SizeOf<T>();
			if( mem.Length < size )
				return default( T );

			FixEndianness<T>( mem );

			return MemoryMarshal.Read<T>( mem.Span.Slice( 0, size ) );
		}

		public unsafe static T ReadStruct<T>( byte* mem, int len ) where T : struct {
			return ReadStruct<T>( new Memory<byte>( new Span<byte>( mem, 0 ).ToArray() ) );
		}

		private static Dictionary<Type, MethodInfo> _cacheParseMethods = new Dictionary<Type, MethodInfo>();
		public static bool TryParseStruct<T>( Packet packet, out T pkt ) where T : struct {
			pkt = default( T );

			if( !_cacheParseMethods.ContainsKey( typeof( T ) ) ) {
				var t = typeof(T).GetMethod("Parse", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

				_cacheParseMethods.Add( typeof( T ), t );
			}

			if( _cacheParseMethods[typeof( T )] != null ) {
				try {
					pkt = (T)_cacheParseMethods[typeof( T )].Invoke( null, new object[] { packet } );
					return true;
				} catch {
					return false;
				}
			}

			return false;
		}

		public unsafe static Memory<byte> Write( object o, Type t, IEnumerable<Attribute> attrs = null ) {
			var preLen = attrs?.Where(a => a is LengthPrefixedAttribute).FirstOrDefault() as LengthPrefixedAttribute;
			var len = attrs?.Where(a => a is LengthAttribute).FirstOrDefault() as LengthAttribute;
			var pad = attrs?.Where(a => a is PaddingAttribute).FirstOrDefault() as PaddingAttribute;
			var ret = new List<Memory<byte>>();

			if( pad != null )
				ret.Add( new byte[pad.Size].AsMemory() );

			if( o == null ) {
				if( preLen != null )
					ret.Add( Write( Convert.ChangeType( 0, preLen.LengthType ), preLen.LengthType ) );

				if( len != null ) {
					if( typeof( IEnumerable ).IsAssignableFrom( t ) && t.GenericTypeArguments != null && t.GenericTypeArguments.Length > 0 ) {
						var st = t.GenericTypeArguments[0];
						ret.Add(new byte[Marshal.SizeOf( st ) * len.Length]);
					} else
						throw new Exception();
				}
			} else {
				if( o is IList ienum ) {
					if( preLen != null )
						ret.Add( Write( Convert.ChangeType( ienum.Count, preLen.LengthType ), preLen.LengthType ) );
					ret.Add( WriteList( ienum, len ) );
				} else if( o is string s ) {
					ret.Add( Encoding.ASCII.GetBytes( s ) );
					ret.Add( new byte[1].AsMemory() );
				} else if( t.IsClass ) {
					ret.Add( WriteClass( o, t ) );
				} else if( t.IsPrimitive || o is Half ) {
					ret.Add( WritePrimitive( o, t ) );
				} else if( t.IsValueType ) {
					throw new Exception();
				}
			}

			return Combine(ret);
		}

		public unsafe static Memory<byte> WriteList( IList ienum, LengthAttribute len = null ) {
			var mems = new List<Memory<byte>>();
			var totalSize = 0;
			var idx = 0;
			var eleSize = 0;
			Type t;
			var tt = ienum?.GetType();
			if( tt?.IsArray == true || tt?.GenericTypeArguments?.Length != 1 )
				t = ienum[0].GetType();
			else
				t = ienum?.GetType().GenericTypeArguments[0] ?? ienum[0]?.GetType() ?? typeof( object );

			foreach( var item in ienum ) {
				
				var mem = Write(item, t);
				totalSize += mem.Length;
				mems.Add( mem );
				idx++;
				eleSize = Math.Max( eleSize, mem.Length );
			}

			if( len != null && idx < len.Length ) {
				var l = (len.Length - idx) * Marshal.SizeOf(t);
				totalSize += l;
				mems.Add( new byte[l].AsMemory() );
			}

			return Combine( mems, totalSize );
		}

		public unsafe static Memory<byte> WritePrimitive<T>( T val ) where T : struct {
			return WritePrimitive( val, typeof( T ) );
		}

		public unsafe static Memory<byte> WritePrimitive( object val, Type t ) {
			byte[] span;

			if( val is byte b ) {
				span = new byte[1];
				span[0] = b;
			} else if( val is char c ) {
				span = new byte[1];
				span[0] = (byte)c;
			} else if( val is short s ) {
				span = new byte[2];
				BinaryPrimitives.WriteInt16LittleEndian( span, s );
			} else if( val is ushort us ) {
				span = new byte[2];
				BinaryPrimitives.WriteUInt16LittleEndian( span, us );
			} else if( val is int i ) {
				span = new byte[4];
				BinaryPrimitives.WriteInt32LittleEndian( span, i );
			} else if( val is uint ui ) {
				span = new byte[4];
				BinaryPrimitives.WriteUInt32LittleEndian( span, ui );
			} else if( val is long l ) {
				span = new byte[8];
				BinaryPrimitives.WriteInt64LittleEndian( span, l );
			} else if( val is ulong ul ) {
				span = new byte[8];
				BinaryPrimitives.WriteUInt64LittleEndian( span, ul );
			} else if( val is Half h) {
				span = new byte[2];
				BinaryPrimitives.WriteUInt16LittleEndian( span, h.Value );
			} else if( val is float f ) {
				span = MemoryMarshal.Cast<float, byte>( new[] { f } ).ToArray();
			} else if( val is double d ) {
				span = MemoryMarshal.Cast<double, byte>( new[] { d } ).ToArray();
			} else
				throw new Exception();

			return span.AsMemory();
		}

		public unsafe static Memory<byte> WriteStruct<T>( T pkt ) where T : struct {
			if( pkt is IWritable write ) {
				return write.Write();
			} else {
				int size = Unsafe.SizeOf<T>();
				Memory<byte> mem = new byte[size];

				MemoryMarshal.Write( mem.Span, ref pkt );

				return mem;
			}
		}

		public unsafe static void WriteStruct<T>( Memory<byte> mem, T pkt ) where T : struct {
			if( pkt is IWritable write )
				write.Write().CopyTo( mem );
			else
				MemoryMarshal.Write( mem.Span, ref pkt );
		}

		public unsafe static Memory<byte> WriteStructBE<T>( T pkt ) where T : struct {
			if( pkt is IWritable write ) {
				return write.WriteBE();
			} else {
				int size = Unsafe.SizeOf<T>();
				Memory<byte> mem = new byte[size];

				MemoryMarshal.Write( mem.Span, ref pkt );

				mem = mem.Slice( 0, size );

				FixEndianness<T>( mem );

				return mem;
			}
		}

		public unsafe static void WriteStructBE<T>( Memory<byte> mem, T pkt ) where T : struct {
			if( pkt is IWritable write )
				write.WriteBE().CopyTo( mem );
			else {
				MemoryMarshal.Write( mem.Span, ref pkt );

				FixEndianness<T>( mem );
			}
		}

		public unsafe static void FixEndianness<T>( ref T val ) where T : struct {
			var len = Unsafe.SizeOf<T>();
			byte* ptr = (byte*)Unsafe.AsPointer(ref val);

			for( var i = 0; i < len / 2; i++ )
				Swap( ref *(ptr + i), ref *((ptr + len) - i) );
		}

		public static void Swap<T>( ref T a, ref T b ) {
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
				data.Slice( startOffset, Marshal.SizeOf( type ) ).Span.Reverse();
			} else if( type.IsEnum ) {
				type = Enum.GetUnderlyingType( type );
				data.Slice( startOffset, Marshal.SizeOf( type ) ).Span.Reverse();
			} else {
				foreach( var field in type.GetFields( BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public ) ) {
					var fieldType = field.FieldType;
					if( field.IsStatic ) // don't process static fields
						continue;

					if( fieldType == typeof( string ) ) // don't swap bytes for strings
						continue;

					var offset = Marshal.OffsetOf(type, field.Name).ToInt32();

					if( fieldType.IsEnum ) // handle enums
						fieldType = Enum.GetUnderlyingType( fieldType );

					// check for sub-fields to recurse if necessary
					var subFields = fieldType.GetFields().Where(subField => subField.IsStatic == false).ToArray();

					var effectiveOffset = startOffset + offset;

					if( subFields.Length == 0 )
						data.Slice( effectiveOffset, Marshal.SizeOf( fieldType ) ).Span.Reverse();
					else
						FixEndianness( fieldType, data, effectiveOffset );
				}
			}
		}

		public static Memory<byte> WriteClass<T>( T pkt ) where T : class {
			return WriteClass( pkt, typeof( T ) );
		}

		public static Memory<byte> WriteClass( object pkt, Type t ) {
			var props = from prop in t.GetFields()
						where Attribute.IsDefined(prop, typeof(FieldAttribute))
						orderby ((FieldAttribute)prop
							.GetCustomAttributes(typeof(FieldAttribute), false)
							.Single()).Order
						select prop;

			var mems = new List<Memory<byte>>();
			var totalSize = 0;
			foreach( var p in props ) {
				var attrs = p.GetCustomAttributes();
				var v = p.GetValue( pkt );

				Memory<byte> vm = Write( v, p.FieldType, attrs );

				totalSize += vm.Length;
				mems.Add( vm );
			}

			return Combine(mems,totalSize);
		}

		public static Memory<byte> Combine( IList<Memory<byte>> mems ) {
			var totalSize = 0;
			foreach( var m in mems ) {
				totalSize += m.Length;
			}

			return Combine(mems, totalSize);
		}

		public static Memory<byte> Combine( IList<Memory<byte>> mems, int totalSize) {
			var ret = new Memory<byte>(new byte[totalSize]);
			var idx = 0;
			foreach( var m in mems ) {
				m.CopyTo( ret.Slice( idx ) );
				idx += m.Length;
			}

			return ret;
		}
	}
}
