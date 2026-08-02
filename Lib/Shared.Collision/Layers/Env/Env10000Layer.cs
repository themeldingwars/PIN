namespace Shared.Collision.Layers;

public class Env10000Layer : WorldLayer
{
    public override uint TypeId => 0x2710;
    public Vec3 Data1 { get; private set; }
    public Vec3? Data2 { get; private set; }

    public override void ParseData(BinaryReader reader, uint length, WorldParseContext ctx)
    {
        Data1 = reader.ReadVec3();
        DataLength = length;

        if (length > 12)
        {
            Data2 = reader.ReadVec3();
        }
    }

    public override void WriteData(BinaryWriter writer)
    {
        writer.Write(Data1);
        if (Data2.HasValue)
        {
            writer.Write(Data2.Value);
        }
    }

    public override uint ComputeDataLength()
    {
        if (DataLength > 0)
        {
            return DataLength;
        }

        return Data2.HasValue ? 24u : 12u;
    }
}
