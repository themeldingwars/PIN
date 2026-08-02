namespace Shared.Collision.Layers;

public class ZoneSubZoneRegionLayer : WorldLayer
{
    public override uint TypeId => 0x21700;
    public uint RegionId { get; private set; }
    public byte[] BboxRaw { get; private set; } = [];
    public uint Width { get; private set; }
    public uint Height { get; private set; }
    public uint BitCount { get; private set; }
    public byte[] Bitmap { get; private set; } = [];

    public override void ParseData(BinaryReader reader, uint length, WorldParseContext ctx)
    {
        RegionId = reader.ReadUInt32();
        BboxRaw = reader.ReadBytes(8);
        Width = reader.ReadUInt32();
        Height = reader.ReadUInt32();
        BitCount = reader.ReadUInt32();

        uint expectedBytes = ((Width * Height) + 7) / 8;
        if (BitCount != expectedBytes)
        {
            throw new InvalidDataException(
                $"Subzone region bitmap size mismatch: expected {expectedBytes}, got {BitCount}");
        }

        Bitmap = reader.ReadBytes((int)BitCount);

        DataLength = length;
    }

    public override void WriteData(BinaryWriter writer)
    {
        writer.Write(RegionId);
        writer.Write(BboxRaw);
        writer.Write(Width);
        writer.Write(Height);
        writer.Write(BitCount);
        writer.Write(Bitmap);
    }

    public override uint ComputeDataLength()
    {
        if (DataLength > 0)
        {
            return DataLength;
        }

        return 28 + (uint)Bitmap.Length;
    }
}
