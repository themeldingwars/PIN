namespace Shared.Collision.Layers;

public class ZoneChunkRef2Layer : WorldLayer
{
    public override uint TypeId => 0x10100;
    public uint X { get; private set; }
    public uint Y { get; private set; }

    public override void ParseData(BinaryReader reader, uint length, WorldParseContext ctx)
    {
        X = reader.ReadUInt32();
        Y = reader.ReadUInt32();
        DataLength = length;
    }

    public override void WriteData(BinaryWriter writer)
    {
        writer.Write(X);
        writer.Write(Y);
    }

    public override uint ComputeDataLength()
    {
        if (DataLength > 0)
        {
            return DataLength;
        }

        return 8;
    }
}
