namespace Shared.Collision.Layers;

public abstract class WorldLayer
{
    public abstract uint TypeId { get; }
    public uint DataLength { get; set; }
    public long FileOffset { get; set; }
    public bool HasMarker { get; set; } = true;
    public List<WorldLayer> Children { get; } = [];

    public abstract void ParseData(BinaryReader reader, uint length, WorldParseContext ctx);
    public abstract void WriteData(BinaryWriter writer);
    public abstract uint ComputeDataLength();
}
