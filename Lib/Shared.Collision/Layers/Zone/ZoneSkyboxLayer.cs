namespace Shared.Collision.Layers;

public class ZoneSkyboxLayer : WorldLayer
{
    public override uint TypeId => 0x20000;
    public uint SkyboxRecordId { get; private set; }

    public override void ParseData(BinaryReader reader, uint length, WorldParseContext ctx)
    {
        SkyboxRecordId = reader.ReadUInt32();
        DataLength = length;
    }

    public override void WriteData(BinaryWriter writer)
    {
        writer.Write(SkyboxRecordId);
    }

    public override uint ComputeDataLength()
    {
        if (DataLength > 0)
        {
            return DataLength;
        }

        return 4;
    }
}
