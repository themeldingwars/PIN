namespace Shared.Collision.Layers;

public class ZonePathLayer : WorldLayer
{
    public override uint TypeId => 0x20800;
    public uint CceId { get; private set; }
    public uint Unk1 { get; private set; }
    public uint StepCount { get; private set; }
    public List<PathStep> Steps { get; } = [];

    public override void ParseData(BinaryReader reader, uint length, WorldParseContext ctx)
    {
        var dataEnd = reader.BaseStream.Position + length;
        DataLength = length;

        CceId = reader.ReadUInt32();
        Unk1 = reader.ReadUInt32();
        StepCount = reader.ReadUInt32();

        for (uint i = 0; i < StepCount; i++)
        {
            var step = new PathStep
            {
                Position = reader.ReadVec3(),
                Orientation = reader.ReadVec4()
            };

            var actionLen = reader.ReadUInt32();
            step.ActionBytes = actionLen > 0 ? reader.ReadBytes((int)actionLen) : Array.Empty<byte>();

            Steps.Add(step);
        }
    }

    public override void WriteData(BinaryWriter writer)
    {
        writer.Write(CceId);
        writer.Write(Unk1);
        writer.Write(StepCount);
        foreach (var step in Steps)
        {
            writer.Write(step.Position);
            writer.Write(step.Orientation);
            writer.Write((uint)step.ActionBytes.Length);
            writer.Write(step.ActionBytes);
        }
    }

    public override uint ComputeDataLength()
    {
        if (DataLength > 0)
        {
            return DataLength;
        }

        uint len = 12;
        foreach (var step in Steps)
        {
            len += 28;
            len += 4 + (uint)step.ActionBytes.Length;
        }

        return len;
    }
}

public record PathStep
{
    public Vec3 Position;
    public Vec4 Orientation;
    public byte[] ActionBytes = [];
}
