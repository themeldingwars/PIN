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

public static class Serializer
{
    public static unsafe void WriteFixed(byte* destination, byte[] source)
    {
        for (var i = 0; i < source.Length; i++)
        {
            *(destination + i) = source[i];
        }
    }

    public static unsafe void WriteFixed(byte* destination, Span<byte> source)
    {
        for (var i = 0; i < source.Length; i++)
        {
            *(destination + i) = source[i];
        }
    }

    public static Memory<byte> WritePrimitive<T>(T value)
        where T : struct
    {
        return WritePrimitive(value, typeof(T));
    }

    public static Memory<byte> WriteStruct<T>(T packet)
        where T : struct
    {
        if (packet is IWritable write)
        {
            return write.Write();
        }

        var size = Unsafe.SizeOf<T>();
        Memory<byte> memory = new byte[size];

        MemoryMarshal.Write(memory.Span, ref packet);

        return memory;
    }

    public static Memory<byte> WriteClass<T>(T packet)
        where T : class
    {
        return WriteClass(packet, typeof(T));
    }

    private static Memory<byte> Write(object obj, Type type, IEnumerable<Attribute> attributes = null)
    {
        attributes = attributes?.ToList();

        var prefixLength = attributes?.Where(a => a is LengthPrefixedAttribute).FirstOrDefault() as LengthPrefixedAttribute;
        var length = attributes?.Where(a => a is LengthAttribute).FirstOrDefault() as LengthAttribute;
        var padding = attributes?.Where(a => a is PaddingAttribute).FirstOrDefault() as PaddingAttribute;
        var exists = attributes?.Where(a => a is ExistsPrefixAttribute).FirstOrDefault() as ExistsPrefixAttribute;
        var memoryList = new List<Memory<byte>>();

        if (padding != null)
        {
            memoryList.Add(new byte[padding.Size].AsMemory());
        }

        if (exists != null)
        {
            if (obj != null)
            {
                memoryList.Add(Write(Convert.ChangeType(exists.TrueValue, exists.ExistsType), exists.ExistsType));
            }
            else
            {
                memoryList.Add(Write(Convert.ChangeType(0, exists.ExistsType), exists.ExistsType));

                return Utils.Combine(memoryList);
            }
        }

        switch (obj)
        {
            case null:
                {
                    if (prefixLength != null)
                    {
                        memoryList.Add(Write(Convert.ChangeType(0, prefixLength.LengthType), prefixLength.LengthType));
                    }

                    if (length != null)
                    {
                        if (typeof(IEnumerable).IsAssignableFrom(type) && type.GenericTypeArguments is { Length: > 0 })
                        {
                            var st = type.GenericTypeArguments[0];
                            memoryList.Add(new byte[Marshal.SizeOf(st) * length.Length]);
                        }
                        else
                        {
                            throw new Exception();
                        }
                    }

                    break;
                }
            case IList list:
                {
                    if (prefixLength != null)
                    {
                        memoryList.Add(Write(Convert.ChangeType(list.Count, prefixLength.LengthType), prefixLength.LengthType));
                    }

                    memoryList.Add(WriteList(list, length));
                    break;
                }
            case string str:
                memoryList.Add(Encoding.ASCII.GetBytes(str));
                memoryList.Add(new byte[1].AsMemory());
                break;
            default:
                {
                    if (type.IsClass)
                    {
                        memoryList.Add(WriteClass(obj, type));
                    }
                    else if (type.IsPrimitive || obj is Half)
                    {
                        memoryList.Add(WritePrimitive(obj, type));
                    }
                    else if (type.IsValueType)
                    {
                        throw new Exception();
                    }

                    break;
                }
        }

        return Utils.Combine(memoryList);
    }

    private static Memory<byte> WritePrimitive(object value, Type type)
    {
        byte[] span;

        switch (value)
        {
            case byte b:
                span = new byte[1];
                span[0] = b;
                break;
            case char c:
                span = new byte[1];
                span[0] = (byte)c;
                break;
            case short s:
                span = new byte[2];
                BinaryPrimitives.WriteInt16LittleEndian(span, s);
                break;
            case ushort us:
                span = new byte[2];
                BinaryPrimitives.WriteUInt16LittleEndian(span, us);
                break;
            case int i:
                span = new byte[4];
                BinaryPrimitives.WriteInt32LittleEndian(span, i);
                break;
            case uint ui:
                span = new byte[4];
                BinaryPrimitives.WriteUInt32LittleEndian(span, ui);
                break;
            case long l:
                span = new byte[8];
                BinaryPrimitives.WriteInt64LittleEndian(span, l);
                break;
            case ulong ul:
                span = new byte[8];
                BinaryPrimitives.WriteUInt64LittleEndian(span, ul);
                break;
            case Half h:
                span = new byte[2];
                BinaryPrimitives.WriteUInt16LittleEndian(span, (ushort)h);
                break;
            case float f:
                span = MemoryMarshal.Cast<float, byte>(new[] { f }).ToArray();
                break;
            case double d:
                span = MemoryMarshal.Cast<double, byte>(new[] { d }).ToArray();
                break;
            default:
                throw new Exception();
        }

        return span.AsMemory();
    }

    private static Memory<byte> WriteList(IList list, LengthAttribute lengthAttribute = null)
    {
        var memoryList = new List<Memory<byte>>();
        var totalSize = 0;
        var index = 0;
        var elementSize = 0;
        Type t;
        var listType = list?.GetType();
        if (listType?.IsArray == true || listType?.GenericTypeArguments?.Length != 1)
        {
            t = list[0].GetType();
        }
        else
        {
            t = list?.GetType().GenericTypeArguments[0] ?? list[0]?.GetType() ?? typeof(object);
        }

        foreach (var item in list)
        {
            var memory = Write(item, t);
            totalSize += memory.Length;
            memoryList.Add(memory);
            index++;
            elementSize = Math.Max(elementSize, memory.Length);
        }

        if (lengthAttribute != null && index < lengthAttribute.Length)
        {
            var length = (lengthAttribute.Length - index) * Marshal.SizeOf(t);
            totalSize += length;
            memoryList.Add(new byte[length].AsMemory());
        }

        return Utils.Combine(memoryList, totalSize);
    }

    private static Memory<byte> WriteClass(object packet, Type type)
    {
        var properties = from property in packet.GetType().GetFields()
                         where Attribute.IsDefined(property, typeof(FieldAttribute))
                         orderby ((FieldAttribute)property
                                                  .GetCustomAttributes(typeof(FieldAttribute), false)
                                                  .Single()).Order
                         select property;

        var memoryList = new List<Memory<byte>>();
        var totalSize = 0;
        foreach (var property in properties)
        {
            var attributes = property.GetCustomAttributes();
            var value = property.GetValue(packet);

            var valueMemory = Write(value, property.FieldType, attributes);

            totalSize += valueMemory.Length;
            memoryList.Add(valueMemory);
        }

        return Utils.Combine(memoryList, totalSize);
    }
}