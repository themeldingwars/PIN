namespace Shared.Collision.Layers;

public class ZoneBoundsLayer : WorldLayer
{
    public override uint TypeId => 0x21000;
    public Vec3 Min { get; private set; }
    public Vec3 Max { get; private set; }

    public override void ParseData(BinaryReader reader, uint length, WorldParseContext ctx)
    {
        Min = reader.ReadVec3();
        Max = reader.ReadVec3();
        DataLength = length;
    }

    public override void WriteData(BinaryWriter writer)
    {
        writer.Write(Min);
        writer.Write(Max);
    }

    public override uint ComputeDataLength()
    {
        if (DataLength > 0)
        {
            return DataLength;
        }

        return 24;
    }
}
