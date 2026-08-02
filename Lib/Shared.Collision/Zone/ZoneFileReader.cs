using System.Text;
using Shared.Collision.Layers;

namespace Shared.Collision.Zone;

public static class ZoneFileReader
{
    public static ZoneFile Read(byte[] data)
    {
        using var stream = new MemoryStream(data);
        using var reader = new BinaryReader(stream);

        var zone = new ZoneFile();

        var magicBytes = reader.ReadBytes(4);
        zone.Magic = Encoding.ASCII.GetString(magicBytes);
        if (zone.Magic != "ZONE")
        {
            throw new InvalidDataException($"Invalid zone magic: {zone.Magic}");
        }

        zone.Version = reader.ReadInt32();
        zone.Timestamp = reader.ReadInt64();

        var nameLen = reader.ReadInt32();
        var nameBytes = reader.ReadBytes(nameLen);
        zone.Name = Encoding.UTF8.GetString(nameBytes).TrimEnd('\0');
        zone.NameBytes = nameBytes;

        if (!GTLayerHeader.TryRead(reader, out var rootType, out var rootLength))
        {
            throw new InvalidDataException("Failed to read root layer header");
        }

        if (rootType != 0x30000)
        {
            throw new InvalidDataException($"Expected root layer type 0x30000, got 0x{rootType:X}");
        }

        var rootCtx = new WorldParseContext(0x30000);
        var root = new ZoneRootLayer();
        root.FileOffset = (int)(stream.Position - GTLayerHeader.HeaderSize);
        root.ParseData(reader, rootLength, rootCtx);
        zone.Root = root;

        if (reader.BaseStream.Position != reader.BaseStream.Length)
        {
            var remaining = reader.BaseStream.Length - reader.BaseStream.Position;
            throw new InvalidDataException($"{remaining} bytes remaining after parsing zone file");
        }

        return zone;
    }

    public static ZoneFile Read(string filePath)
    {
        var data = File.ReadAllBytes(filePath);
        return Read(data);
    }
}
