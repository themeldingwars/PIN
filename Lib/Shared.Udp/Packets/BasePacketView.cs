using System;
using System.Collections.Concurrent;

namespace Shared.Udp.Packets;

public class BasePacketView : IPacketView
{
    protected ConcurrentDictionary<string, object> _fields;

    public BasePacketView(BitEndianness e)
    {
        Endianness = e;

        if (Endianness is BitEndianness.LittleBigEndian or BitEndianness.BigLittleEndian)
        {
            throw new NotImplementedException("LittleBigEndian and BigLittleEndian NYI");
        }
    }

    public BitEndianness Endianness { get; }

    public T Get<T>(Memory<byte> data, string name)
    {
        return default;
    }

    public T Get<T>(Memory<byte> data, int offset, int length = -1)
    {
        return default;
    }

    protected void SetupFields()
    {
        _fields = new ConcurrentDictionary<string, object>();
    }
}