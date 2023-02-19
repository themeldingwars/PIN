﻿using System;
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
    protected ConcurrentDictionary<string, object> fields;

    public BasePacketView(BitEndianness e)
    {
        Endianness = e;

        if (Endianness == BitEndianness.LittleBigEndian || Endianness == BitEndianness.BigLittleEndian)
        {
            throw new NotImplementedException("LittleBigEndian and BigLittleEndian NYI");
        }
    }

    public BitEndianness Endianness { get; protected set; }

    protected void SetupFields()
    {
        fields = new ConcurrentDictionary<string, object>();
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