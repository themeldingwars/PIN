using System;
using System.Collections.Concurrent;

namespace Shared.Udp.Packets;

public enum BitEndianness
{
    Unknown = 0,
    LittleEndian,
    LittleBigEndian,
    BigLittleEndian,
    BigEndian
}

public class BasePacketView : IPacketView
{
    protected ConcurrentDictionary<string, object> Fields;

    public BasePacketView(BitEndianness e)
    {
        Endianness = e;

        if (Endianness is BitEndianness.LittleBigEndian or BitEndianness.BigLittleEndian)
        {
            throw new NotImplementedException("LittleBigEndian and BigLittleEndian NYI");
        }
    }

    public BitEndianness Endianness { get; }

    protected void SetupFields()
    {
        Fields = new ConcurrentDictionary<string, object>();
    }

    public T Get<T>(Memory<byte> data, string name)
    {
        return default;
    }

    public T Get<T>(Memory<byte> data, int offset, int length = -1)
    {
        return default;
    }
}