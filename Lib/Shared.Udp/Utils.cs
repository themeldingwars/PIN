using System;
using System.Buffers.Binary;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Shared.Udp
{
    public static class Utils
    {
        public static Thread RunThread(ThreadStart action)
        {
            var t = new Thread(action);
            t.Start();

            return t;
        }

        public static Thread RunThread(Action<CancellationToken> action, CancellationToken ct)
        {
            var t = new Thread(o => action((CancellationToken)o));
            t.Start(ct);

            return t;
        }

        public static unsafe void WriteFixed(byte* dest, byte[] src)
        {
            for (var i = 0; i < src.Length; i++)
            {
                *(dest + i) = src[i];
            }
        }

        public static unsafe void WriteFixed(byte* dest, Span<byte> src)
        {
            for (var i = 0; i < src.Length; i++)
            {
                *(dest + i) = src[i];
            }
        }

        public static unsafe string ReadFixedString(byte* src, int len)
        {
            return Encoding.ASCII.GetString(src, len);
        }

        public static unsafe byte[] ReadFixed(byte* src, int len)
        {
            return new Span<byte>(src, len).ToArray();
        }

        public static T ReadStruct<T>(ReadOnlyMemory<byte> mem) where T : struct
        {
            var size = Unsafe.SizeOf<T>();
            if (mem.Length < size)
            {
                return default;
            }

            return MemoryMarshal.Read<T>(mem.Span.Slice(0, size));
        }

        public static T ReadStruct<T>(ReadOnlyMemory<byte> mem, out int size) where T : struct
        {
            size = Unsafe.SizeOf<T>();
            if (mem.Length < size)
            {
                return default;
            }

            return MemoryMarshal.Read<T>(mem.Span.Slice(0, size));
        }

        public static unsafe T ReadStruct<T>(byte* mem, int len) where T : struct
        {
            return ReadStruct<T>(new ReadOnlyMemory<byte>(new Span<byte>(mem, 0).ToArray()));
        }

        public static T Read<T>(ref ReadOnlyMemory<byte> data)
        {
            return (T)Read(ref data, typeof(T));
        }

        public static unsafe object Read(ref ReadOnlyMemory<byte> data, Type t, IEnumerable<Attribute> attrs = null)
        {
            if (attrs == null)
            {
                attrs = t.GetCustomAttributes();
            }

            var preLen =
                attrs.Where(a => a is LengthPrefixedAttribute).FirstOrDefault() as LengthPrefixedAttribute;
            var len = attrs.Where(a => a is LengthAttribute).FirstOrDefault() as LengthAttribute;
            var pad = attrs.Where(a => a is PaddingAttribute).FirstOrDefault() as PaddingAttribute;
            var exists =
                attrs.Where(a => a is ExistsPrefixAttribute).FirstOrDefault() as ExistsPrefixAttribute;

            object ret = null;

            if (pad != null)
            {
                data = data.Slice(pad.Size);
            }

            if (exists != null)
            {
                if (Read(ref data, exists.ExistsType) != exists.TrueValue)
                {
                    return null;
                }

                data = data.Slice(Marshal.SizeOf(exists.ExistsType));
            }

            if (typeof(IEnumerable).IsAssignableFrom(t) && t.GenericTypeArguments != null &&
                t.GenericTypeArguments.Length > 0)
            {
                var l = 0;
                if (preLen != null)
                {
                    l = (int)Convert.ChangeType(Read(ref data, preLen.LengthType), typeof(int));
                    data = data.Slice(Marshal.SizeOf(preLen.LengthType));
                }
                else if (len != null)
                {
                    l = len.Length;
                }
                else
                {
                    throw new Exception();
                }

                var tempRet =
                    (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(t.GenericTypeArguments));
                for (var i = 0; i < l; i++)
                {
                    _ = tempRet.Add(Read(ref data, t.GenericTypeArguments[0]));
                }

                ret = tempRet;
            }
            else if (typeof(string) == t)
            {
                var l = 0;

                while (l < data.Length && data.Span[l] != 0x00)
                {
                    l++;
                }

                ret = Encoding.ASCII.GetString(data.Slice(0, l - 1).Span.ToArray());
                data = data.Slice(l);
            }
            else if (t.IsClass)
            {
                ret = ReadClass(ref data, t);
            }
            else if (t.IsPrimitive || typeof(Half) == t)
            {
                ret = ReadPrimitive(ref data, t);
            }
            else if (t.IsValueType && t.BaseType == typeof(Enum))
            {
                ret = Enum.ToObject(t, Read(ref data, Enum.GetUnderlyingType(t)));
            }
            else if (t.IsValueType)
            {
                var size = Marshal.SizeOf(t);
                var mh = data.Slice(0, size).Pin();

                ret = Marshal.PtrToStructure(new IntPtr(mh.Pointer), t);
                data = data.Slice(size);

                mh.Dispose();
            }

            return ret;
        }

        public static object ReadPrimitive(ref ReadOnlyMemory<byte> data, Type t)
        {
            ReadOnlySpan<byte> span;

            if (typeof(byte) == t)
            {
                span = data.Slice(0, 1).Span;
                data = data.Slice(1);
                return span[0];
            }

            if (typeof(char) == t)
            {
                span = data.Slice(0, 1).Span;
                data = data.Slice(1);
                return Encoding.ASCII.GetChars(span.ToArray())[0];
            }

            if (typeof(short) == t)
            {
                span = data.Slice(0, 2).Span;
                data = data.Slice(2);
                return BinaryPrimitives.ReadInt16LittleEndian(span);
            }

            if (typeof(ushort) == t)
            {
                span = data.Slice(0, 2).Span;
                data = data.Slice(2);
                return BinaryPrimitives.ReadUInt16LittleEndian(span);
            }

            if (typeof(int) == t)
            {
                span = data.Slice(0, 4).Span;
                data = data.Slice(4);
                return BinaryPrimitives.ReadInt32LittleEndian(span);
            }

            if (typeof(uint) == t)
            {
                span = data.Slice(0, 4).Span;
                data = data.Slice(4);
                return BinaryPrimitives.ReadUInt32LittleEndian(span);
            }

            if (typeof(long) == t)
            {
                span = data.Slice(0, 8).Span;
                data = data.Slice(8);
                return BinaryPrimitives.ReadInt64LittleEndian(span);
            }

            if (typeof(ulong) == t)
            {
                span = data.Slice(0, 8).Span;
                data = data.Slice(8);
                return BinaryPrimitives.ReadUInt64LittleEndian(span);
            }

            if (typeof(Half) == t)
            {
                span = data.Slice(0, 2).Span;
                data = data.Slice(2);
                return BinaryPrimitives.ReadUInt16LittleEndian(span);
            }

            if (typeof(float) == t)
            {
                span = data.Slice(0, 4).Span;
                data = data.Slice(4);
                return MemoryMarshal.Cast<byte, float>(span)[0];
            }

            if (typeof(double) == t)
            {
                span = data.Slice(0, 8).Span;
                data = data.Slice(8);
                return MemoryMarshal.Cast<byte, double>(span)[0];
            }

            throw new Exception();
        }

        public static T ReadClass<T>(ref ReadOnlyMemory<byte> data) where T : class
        {
            return (T)ReadClass(ref data, typeof(T));
        }

        public static object ReadClass(ref ReadOnlyMemory<byte> data, Type t)
        {
            var props = from prop in t.GetFields()
                        where Attribute.IsDefined(prop, typeof(FieldAttribute))
                        orderby ((FieldAttribute)prop
                                                 .GetCustomAttributes(typeof(FieldAttribute),
                                                                      false)
                                                 .Single()).Order
                        select prop;

            var ret = Activator.CreateInstance(t);
            foreach (var p in props)
            {
                var attrs = p.GetCustomAttributes();
                var v = Read(ref data, p.FieldType, attrs);

                p.SetValue(ret, v);
            }

            return ret;
        }

        public static Memory<byte> Write(object o, Type t, IEnumerable<Attribute> attrs = null)
        {
            var preLen =
                attrs?.Where(a => a is LengthPrefixedAttribute).FirstOrDefault() as LengthPrefixedAttribute;
            var len = attrs?.Where(a => a is LengthAttribute).FirstOrDefault() as LengthAttribute;
            var pad = attrs?.Where(a => a is PaddingAttribute).FirstOrDefault() as PaddingAttribute;
            var exists =
                attrs?.Where(a => a is ExistsPrefixAttribute).FirstOrDefault() as ExistsPrefixAttribute;
            var ret = new List<Memory<byte>>();

            if (pad != null)
            {
                ret.Add(new byte[pad.Size].AsMemory());
            }

            if (exists != null)
            {
                if (o != null)
                {
                    ret.Add(Write(Convert.ChangeType(exists.TrueValue, exists.ExistsType), exists.ExistsType));
                }
                else
                {
                    ret.Add(Write(Convert.ChangeType(0, exists.ExistsType), exists.ExistsType));

                    return Combine(ret);
                }
            }

            if (o == null)
            {
                if (preLen != null)
                {
                    ret.Add(Write(Convert.ChangeType(0, preLen.LengthType), preLen.LengthType));
                }

                if (len != null)
                {
                    if (typeof(IEnumerable).IsAssignableFrom(t) && t.GenericTypeArguments != null &&
                        t.GenericTypeArguments.Length > 0)
                    {
                        var st = t.GenericTypeArguments[0];
                        ret.Add(new byte[Marshal.SizeOf(st) * len.Length]);
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
            }
            else
            {
                if (o is IList ienum)
                {
                    if (preLen != null)
                    {
                        ret.Add(Write(Convert.ChangeType(ienum.Count, preLen.LengthType), preLen.LengthType));
                    }

                    ret.Add(WriteList(ienum, len));
                }
                else if (o is string s)
                {
                    ret.Add(Encoding.ASCII.GetBytes(s));
                    ret.Add(new byte[1].AsMemory());
                }
                else if (t.IsClass)
                {
                    ret.Add(WriteClass(o, t));
                }
                else if (t.IsPrimitive || o is Half)
                {
                    ret.Add(WritePrimitive(o, t));
                }
                else if (t.IsValueType)
                {
                    throw new Exception();
                }
            }

            return Combine(ret);
        }

        public static Memory<byte> WriteList(IList ienum, LengthAttribute len = null)
        {
            var mems = new List<Memory<byte>>();
            var totalSize = 0;
            var idx = 0;
            var eleSize = 0;
            Type t;
            var tt = ienum?.GetType();
            if (tt?.IsArray == true || tt?.GenericTypeArguments?.Length != 1)
            {
                t = ienum[0].GetType();
            }
            else
            {
                t = ienum?.GetType().GenericTypeArguments[0] ?? ienum[0]?.GetType() ?? typeof(object);
            }

            foreach (var item in ienum)
            {
                var mem = Write(item, t);
                totalSize += mem.Length;
                mems.Add(mem);
                idx++;
                eleSize = Math.Max(eleSize, mem.Length);
            }

            if (len != null && idx < len.Length)
            {
                var l = (len.Length - idx) * Marshal.SizeOf(t);
                totalSize += l;
                mems.Add(new byte[l].AsMemory());
            }

            return Combine(mems, totalSize);
        }

        public static Memory<byte> WritePrimitive<T>(T val) where T : struct
        {
            return WritePrimitive(val, typeof(T));
        }

        public static Memory<byte> WritePrimitive(object val, Type t)
        {
            byte[] span;

            if (val is byte b)
            {
                span = new byte[1];
                span[0] = b;
            }
            else if (val is char c)
            {
                span = new byte[1];
                span[0] = (byte)c;
            }
            else if (val is short s)
            {
                span = new byte[2];
                BinaryPrimitives.WriteInt16LittleEndian(span, s);
            }
            else if (val is ushort us)
            {
                span = new byte[2];
                BinaryPrimitives.WriteUInt16LittleEndian(span, us);
            }
            else if (val is int i)
            {
                span = new byte[4];
                BinaryPrimitives.WriteInt32LittleEndian(span, i);
            }
            else if (val is uint ui)
            {
                span = new byte[4];
                BinaryPrimitives.WriteUInt32LittleEndian(span, ui);
            }
            else if (val is long l)
            {
                span = new byte[8];
                BinaryPrimitives.WriteInt64LittleEndian(span, l);
            }
            else if (val is ulong ul)
            {
                span = new byte[8];
                BinaryPrimitives.WriteUInt64LittleEndian(span, ul);
            }
            else if (val is Half h)
            {
                span = new byte[2];
                BinaryPrimitives.WriteUInt16LittleEndian(span, Convert.ToUInt16(h));
            }
            else if (val is float f)
            {
                span = MemoryMarshal.Cast<float, byte>(new[] { f }).ToArray();
            }
            else if (val is double d)
            {
                span = MemoryMarshal.Cast<double, byte>(new[] { d }).ToArray();
            }
            else
            {
                throw new Exception();
            }

            return span.AsMemory();
        }

        public static Memory<byte> WriteStruct<T>(T pkt) where T : struct
        {
            if (pkt is IWritable write)
            {
                return write.Write();
            }

            var size = Unsafe.SizeOf<T>();
            Memory<byte> mem = new byte[size];

            MemoryMarshal.Write(mem.Span, ref pkt);

            return mem;
        }

        public static void WriteStruct<T>(Memory<byte> mem, T pkt) where T : struct
        {
            if (pkt is IWritable write)
            {
                write.Write().CopyTo(mem);
            }
            else
            {
                MemoryMarshal.Write(mem.Span, ref pkt);
            }
        }

        public static T SimpleFixEndianess<T>(T val) where T : struct
        {
            var s = MemoryMarshal.Cast<T, byte>(new[] { val });
            s.Reverse();
            return MemoryMarshal.Cast<byte, T>(s).ToArray().FirstOrDefault();
        }

        public static void Swap<T>(ref T a, ref T b)
        {
            var t = a;
            a = b;
            b = t;
        }

        public static Memory<byte> WriteClass<T>(T pkt) where T : class
        {
            return WriteClass(pkt, typeof(T));
        }

        public static Memory<byte> WriteClass(object pkt, Type t)
        {
            var props = from prop in pkt.GetType().GetFields()
                        where Attribute.IsDefined(prop, typeof(FieldAttribute))
                        orderby ((FieldAttribute)prop
                                                 .GetCustomAttributes(typeof(FieldAttribute),
                                                                      false)
                                                 .Single()).Order
                        select prop;

            var mems = new List<Memory<byte>>();
            var totalSize = 0;
            foreach (var p in props)
            {
                var attrs = p.GetCustomAttributes();
                var v = p.GetValue(pkt);

                var vm = Write(v, p.FieldType, attrs);

                totalSize += vm.Length;
                mems.Add(vm);
            }

            return Combine(mems, totalSize);
        }

        public static Memory<byte> Combine(IList<Memory<byte>> mems)
        {
            var totalSize = 0;
            foreach (var m in mems)
            {
                totalSize += m.Length;
            }

            return Combine(mems, totalSize);
        }

        public static Memory<byte> Combine(IList<Memory<byte>> mems, int totalSize)
        {
            var ret = new Memory<byte>(new byte[totalSize]);
            var idx = 0;
            foreach (var m in mems)
            {
                m.CopyTo(ret.Slice(idx));
                idx += m.Length;
            }

            return ret;
        }
    }
}