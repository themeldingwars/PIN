namespace Shared.Collision.Layers;

public class ChunkSubZoneGridLayer : WorldLayer
{
    public override uint TypeId => 0x40102;
    public uint UnknownField { get; private set; }
    public int GridSize { get; private set; }
    public uint[] SubZoneIds { get; private set; } = [];
    public uint GridCount { get; private set; }
    public byte[] GridData { get; private set; } = [];

    public override void ParseData(BinaryReader reader, uint length, WorldParseContext ctx)
    {
        UnknownField = reader.ReadUInt32();
        GridSize = reader.ReadInt32();
        uint subzoneIdsCount = reader.ReadUInt32();

        SubZoneIds = new uint[subzoneIdsCount];
        for (uint i = 0; i < subzoneIdsCount; i++)
        {
            SubZoneIds[i] = reader.ReadUInt32();
        }

        GridCount = reader.ReadUInt32();
        uint totalGridBytes = GridCount * (uint)GridSize * (uint)GridSize;
        GridData = reader.ReadBytes((int)totalGridBytes);

        DataLength = length;
    }

    public override void WriteData(BinaryWriter writer)
    {
        writer.Write(UnknownField);
        writer.Write(GridSize);
        writer.Write((uint)SubZoneIds.Length);
        foreach (var id in SubZoneIds)
        {
            writer.Write(id);
        }

        writer.Write(GridCount);
        writer.Write(GridData);
    }

    public override uint ComputeDataLength()
    {
        if (DataLength > 0)
        {
            return DataLength;
        }

        return 12 + (uint)(SubZoneIds.Length * 4) + 4 + (uint)GridData.Length;
    }
}
