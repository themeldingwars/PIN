namespace Shared.Collision.Layers;

public class UnknownWorldLayer : WorldLayer
{
    public UnknownWorldLayer(uint typeId)
    {
        TypeId = typeId;
    }

    public override uint TypeId { get; }
    public byte[] RawData { get; set; } = [];

    public override void ParseData(BinaryReader reader, uint length, WorldParseContext ctx)
    {
        RawData = reader.ReadBytes((int)length);
        DataLength = length;
    }

    public override void WriteData(BinaryWriter writer)
    {
        writer.Write(RawData);
    }

    public override uint ComputeDataLength()
    {
        return (uint)RawData.Length;
    }
}
