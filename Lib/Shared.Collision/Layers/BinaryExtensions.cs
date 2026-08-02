using System.Text;
namespace Shared.Collision.Layers;

public static class BinaryReaderExtensions
{
    public static Vec3 ReadVec3(this BinaryReader reader)
    {
        var x = reader.ReadSingle();
        var y = reader.ReadSingle();
        var z = reader.ReadSingle();
        return new Vec3(x, y, z);
    }

    public static Vec4 ReadVec4(this BinaryReader reader)
    {
        var x = reader.ReadSingle();
        var y = reader.ReadSingle();
        var z = reader.ReadSingle();
        var w = reader.ReadSingle();
        return new Vec4(x, y, z, w);
    }

    public static string ReadLengthPrefixedString(this BinaryReader reader)
    {
        var len = reader.ReadUInt32();
        if (len == 0)
        {
            return string.Empty;
        }

        var bytes = reader.ReadBytes((int)len);
        return Encoding.UTF8.GetString(bytes);
    }

    public static string ReadNullTerminatedString(this BinaryReader reader, int maxBytes)
    {
        var bytes = new List<byte>();
        for (int i = 0; i < maxBytes; i++)
        {
            var b = reader.ReadByte();
            if (b == 0)
            {
                break;
            }

            bytes.Add(b);
        }

        return Encoding.UTF8.GetString([.. bytes]);
    }
}

public static class BinaryWriterExtensions
{
    public static void Write(this BinaryWriter writer, Vec3 v)
    {
        writer.Write(v.X);
        writer.Write(v.Y);
        writer.Write(v.Z);
    }

    public static void Write(this BinaryWriter writer, Vec4 v)
    {
        writer.Write(v.X);
        writer.Write(v.Y);
        writer.Write(v.Z);
        writer.Write(v.W);
    }

    public static void WriteLengthPrefixedString(this BinaryWriter writer, string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        writer.Write((uint)bytes.Length);
        writer.Write(bytes);
    }
}
