namespace Shared.Collision.Layers;

public class ZoneChunkRangeLayer : WorldLayer
{
    public override uint TypeId => 0x10000;
    public uint CubeFaceId { get; private set; }
    public uint MinX { get; private set; }
    public uint MaxX { get; private set; }
    public uint MinY { get; private set; }
    public uint MaxY { get; private set; }

    public override void ParseData(BinaryReader reader, uint length, WorldParseContext ctx)
    {
        CubeFaceId = reader.ReadUInt32();
        MinX = reader.ReadUInt32();
        MaxX = reader.ReadUInt32();
        MinY = reader.ReadUInt32();
        MaxY = reader.ReadUInt32();
        DataLength = length;
    }

    public override void WriteData(BinaryWriter writer)
    {
        writer.Write(CubeFaceId);
        writer.Write(MinX);
        writer.Write(MaxX);
        writer.Write(MinY);
        writer.Write(MaxY);
    }

    public override uint ComputeDataLength()
    {
        if (DataLength > 0)
        {
            return DataLength;
        }

        return 20;
    }
}
