namespace Shared.Collision.Layers;

public abstract class ContainerWorldLayer : WorldLayer
{
    public override void ParseData(BinaryReader reader, uint length, WorldParseContext ctx)
    {
        var dataEnd = reader.BaseStream.Position + length;
        DataLength = length;

        while (reader.BaseStream.Position < dataEnd)
        {
            var childCtx = new WorldParseContext(TypeId);
            var child = WorldLayerParser.ParseChild(reader, childCtx, dataEnd);
            if (child == null)
            {
                break;
            }

            Children.Add(child);
        }

        var remaining = (int)(dataEnd - reader.BaseStream.Position);
        if (remaining > 0)
        {
            throw new InvalidDataException(
                $"Container 0x{TypeId:X} has {remaining} unconsumed bytes after parsing children");
        }
    }

    public override void WriteData(BinaryWriter writer)
    {
        foreach (var child in Children)
        {
            var childDataLen = child.ComputeDataLength();
            if (child.HasMarker)
            {
                GTLayerHeader.WriteHeader(writer, child.TypeId, childDataLen);
            }
            else
            {
                writer.Write(child.TypeId);
                writer.Write(childDataLen);
            }

            child.WriteData(writer);
        }
    }

    public override uint ComputeDataLength()
    {
        if (DataLength > 0)
        {
            return DataLength;
        }

        uint total = 0;
        foreach (var child in Children)
        {
            total += child.HasMarker ? (uint)GTLayerHeader.HeaderSize : 8;
            total += child.ComputeDataLength();
        }

        return total;
    }
}
