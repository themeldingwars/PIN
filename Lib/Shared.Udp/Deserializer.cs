using System;
using System.Buffers.Binary;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Shared.Udp;

public static class Deserializer
{
    public static unsafe byte[] ReadFixed(byte* source, int length)
    {
        return new Span<byte>(source, length).ToArray();
    }

    public static unsafe string ReadFixedString(byte* source, int length)
    {
        return Encoding.ASCII.GetString(source, length);
    }

    public static T ReadStruct<T>(ReadOnlyMemory<byte> data)
        where T : struct
    {
        var size = Unsafe.SizeOf<T>();
        if (data.Length < size)
        {
            return default;
        }

        return MemoryMarshal.Read<T>(data.Span[..size]);
    }

    public static T ReadPrimitive<T>(ref ReadOnlyMemory<byte> data)
        where T : class
    {
        return (T)ReadPrimitive(ref data, typeof(T));
    }

    public static T ReadClass<T>(ref ReadOnlyMemory<byte> data)
        where T : class
    {
        return (T)ReadClass(ref data, typeof(T));
    }

    public static T Read<T>(ref ReadOnlyMemory<byte> data)
    {
        return (T)Read(ref data, typeof(T));
    }

    private static unsafe object Read(ref ReadOnlyMemory<byte> data, Type type, IEnumerable<Attribute> attributes = null)
    {
        attributes = attributes?.ToList();

        if (attributes == null)
        {
            attributes = type.GetCustomAttributes();
        }

        var prefixLength = attributes.FirstOrDefault(a => a is LengthPrefixedAttribute) as LengthPrefixedAttribute;
        var length = attributes.FirstOrDefault(a => a is LengthAttribute) as LengthAttribute;
        var padding = attributes.FirstOrDefault(a => a is PaddingAttribute) as PaddingAttribute;
        var exists = attributes.FirstOrDefault(a => a is ExistsPrefixAttribute) as ExistsPrefixAttribute;

        object ret = null;

        if (padding != null)
        {
            data = data[padding.Size..];
        }

        if (exists != null)
        {
            if (Read(ref data, exists.ExistsType) != exists.TrueValue)
            {
                return null;
            }

            data = data[Marshal.SizeOf(exists.ExistsType)..];
        }

        if (typeof(IEnumerable).IsAssignableFrom(type) && type.GenericTypeArguments != null && type.GenericTypeArguments.Length > 0)
        {
            var l = 0;
            if (prefixLength != null)
            {
                l = (int)Convert.ChangeType(Read(ref data, prefixLength.LengthType), typeof(int));
                data = data[Marshal.SizeOf(prefixLength.LengthType)..];
            }
            else if (length != null)
            {
                l = length.Length;
            }
            else
            {
                throw new Exception();
            }

            var tempRet = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(type.GenericTypeArguments));
            for (var i = 0; i < l; i++)
            {
                _ = tempRet.Add(Read(ref data, type.GenericTypeArguments[0]));
            }

            ret = tempRet;
        }
        else if (typeof(string) == type)
        {
            var l = 0;

            while (l < data.Length && data.Span[l] != 0x00)
            {
                l++;
            }

            l++; // null terminator

            ret = Encoding.ASCII.GetString(data[..(l - 1)].Span.ToArray());
            data = data[l..];
        }
        else if (type.IsClass)
        {
            ret = ReadClass(ref data, type);
        }
        else if (type.IsPrimitive || typeof(Half) == type)
        {
            ret = ReadPrimitive(ref data, type);
        }
        else if (type.IsValueType && type.BaseType == typeof(Enum))
        {
            ret = Enum.ToObject(type, Read(ref data, Enum.GetUnderlyingType(type)));
        }
        else if (type.IsValueType)
        {
            var size = Marshal.SizeOf(type);
            var memoryHandle = data[..size].Pin();

            ret = Marshal.PtrToStructure(new IntPtr(memoryHandle.Pointer), type);
            data = data[size..];

            memoryHandle.Dispose();
        }

        return ret;
    }

    public static object ReadPrimitive(ref ReadOnlyMemory<byte> data, Type type)
    {
        ReadOnlySpan<byte> span;

        if (typeof(byte) == type)
        {
            span = data[..1].Span;
            data = data[1..];
            return span[0];
        }

        if (typeof(char) == type)
        {
            span = data[..1].Span;
            data = data[1..];
            return Encoding.ASCII.GetChars(span.ToArray())[0];
        }

        if (typeof(short) == type)
        {
            span = data[..2].Span;
            data = data[2..];
            return BinaryPrimitives.ReadInt16LittleEndian(span);
        }

        if (typeof(ushort) == type)
        {
            span = data[..2].Span;
            data = data[2..];
            return BinaryPrimitives.ReadUInt16LittleEndian(span);
        }

        if (typeof(int) == type)
        {
            span = data[..4].Span;
            data = data[4..];
            return BinaryPrimitives.ReadInt32LittleEndian(span);
        }

        if (typeof(uint) == type)
        {
            span = data[..4].Span;
            data = data[4..];
            return BinaryPrimitives.ReadUInt32LittleEndian(span);
        }

        if (typeof(long) == type)
        {
            span = data[..8].Span;
            data = data[8..];
            return BinaryPrimitives.ReadInt64LittleEndian(span);
        }

        if (typeof(ulong) == type)
        {
            span = data[..8].Span;
            data = data[8..];
            return BinaryPrimitives.ReadUInt64LittleEndian(span);
        }

        if (typeof(Half) == type)
        {
            span = data[..2].Span;
            data = data[2..];
            return BinaryPrimitives.ReadUInt16LittleEndian(span);
        }

        if (typeof(float) == type)
        {
            span = data[..4].Span;
            data = data[4..];
            return MemoryMarshal.Cast<byte, float>(span)[0];
        }

        if (typeof(double) == type)
        {
            span = data[..8].Span;
            data = data[8..];
            return MemoryMarshal.Cast<byte, double>(span)[0];
        }

        throw new Exception();
    }

    private static object ReadClass(ref ReadOnlyMemory<byte> data, Type type)
    {
        var properties = from property in type.GetFields()
                         where Attribute.IsDefined(property, typeof(FieldAttribute))
                         orderby ((FieldAttribute)property
                                                  .GetCustomAttributes(typeof(FieldAttribute), false)
                                                  .Single()).Order
                         select property;

        var ret = Activator.CreateInstance(type);
        foreach (var property in properties)
        {
            var attrs = property.GetCustomAttributes();
            var value = Read(ref data, property.FieldType, attrs);

            property.SetValue(ret, value);
        }

        return ret;
    }
}