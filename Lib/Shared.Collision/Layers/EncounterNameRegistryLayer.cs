namespace Shared.Collision.Layers;

public abstract class EncounterNameRegistryLayer : WorldLayer
{
    public string[] Names { get; private set; } = [];

    public override void ParseData(BinaryReader reader, uint length, WorldParseContext ctx)
    {
        uint count = reader.ReadUInt32();
        Names = new string[count];

        for (uint i = 0; i < count; i++)
        {
            uint strLen = reader.ReadUInt32();
            byte[] bytes = reader.ReadBytes((int)strLen);
            Names[i] = System.Text.Encoding.ASCII.GetString(bytes);
        }

        DataLength = length;
    }

    public override void WriteData(BinaryWriter writer)
    {
        writer.Write((uint)Names.Length);
        foreach (var name in Names)
        {
            byte[] bytes = System.Text.Encoding.ASCII.GetBytes(name);
            writer.Write((uint)bytes.Length);
            writer.Write(bytes);
        }
    }

    public override uint ComputeDataLength()
    {
        if (DataLength > 0)
        {
            return DataLength;
        }

        uint total = 4; // count
        foreach (var name in Names)
        {
            total += 4; // length
            total += (uint)System.Text.Encoding.ASCII.GetByteCount(name);
        }

        return total;
    }
}
