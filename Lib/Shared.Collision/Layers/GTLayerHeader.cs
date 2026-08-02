namespace Shared.Collision.Layers;

public static class GTLayerHeader
{
    public const int HeaderSize = 16;
    public static readonly byte[] MarkerBytes =
    [
        0xED, 0x12, 0x5B, 0xED,
        0x12, 0x5A, 0xED, 0x12
    ];

    public static bool TryRead(BinaryReader reader, out uint typeId, out uint dataLength)
    {
        var pos = reader.BaseStream.Position;
        typeId = 0;
        dataLength = 0;

        var buffer = reader.ReadBytes(8);
        if (buffer.Length < 8)
        {
            reader.BaseStream.Position = pos;
            return false;
        }

        if (SequenceEqual(buffer, MarkerBytes))
        {
            ReadFourBytes(reader, out typeId);
            ReadFourBytes(reader, out dataLength);
            return true;
        }

        var low = (uint)(buffer[0] | (buffer[1] << 8) | (buffer[2] << 16) | (buffer[3] << 24));
        var high = (uint)(buffer[4] | (buffer[5] << 8) | (buffer[6] << 16) | (buffer[7] << 24));
        typeId = low;
        dataLength = high;
        return true;
    }

    public static void WriteHeader(BinaryWriter writer, uint typeId, uint dataLength)
    {
        writer.Write(MarkerBytes);
        writer.Write(typeId);
        writer.Write(dataLength);
    }

    public static bool SequenceEqualMarker(ReadOnlySpan<byte> a) => SequenceEqual(a, MarkerBytes);

    private static void ReadFourBytes(BinaryReader reader, out uint value)
    {
        var b = reader.ReadBytes(4);
        value = (uint)(b[0] | (b[1] << 8) | (b[2] << 16) | (b[3] << 24));
    }

    private static bool SequenceEqual(ReadOnlySpan<byte> a, ReadOnlySpan<byte> b)
    {
        if (a.Length != b.Length)
        {
            return false;
        }

        for (var i = 0; i < a.Length; i++)
        {
            if (a[i] != b[i])
            {
                return false;
            }
        }

        return true;
    }
}
