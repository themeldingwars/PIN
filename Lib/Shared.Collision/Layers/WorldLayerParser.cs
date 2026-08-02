using Shared.Collision.Layers.Collision;

namespace Shared.Collision.Layers;

public static class WorldLayerParser
{
    public static WorldLayer? ParseChild(BinaryReader reader, WorldParseContext ctx, long dataEnd)
    {
        var pos = reader.BaseStream.Position;
        if (pos >= dataEnd)
        {
            return null;
        }

        var startOffset = pos;

        var buffer = reader.ReadBytes(8);
        if (buffer.Length < 8)
        {
            return null;
        }

        bool hasMarker = false;
        uint typeId, length;

        if (GTLayerHeader.SequenceEqualMarker(buffer))
        {
            hasMarker = true;
            var typeBytes = reader.ReadBytes(4);
            var lenBytes = reader.ReadBytes(4);
            typeId = (uint)(typeBytes[0] | (typeBytes[1] << 8) | (typeBytes[2] << 16) | (typeBytes[3] << 24));
            length = (uint)(lenBytes[0] | (lenBytes[1] << 8) | (lenBytes[2] << 16) | (lenBytes[3] << 24));
        }
        else
        {
            typeId = (uint)(buffer[0] | (buffer[1] << 8) | (buffer[2] << 16) | (buffer[3] << 24));
            length = (uint)(buffer[4] | (buffer[5] << 8) | (buffer[6] << 16) | (buffer[7] << 24));
        }

        var expectedEnd = reader.BaseStream.Position + length;
        if (expectedEnd > dataEnd)
        {
            return null;
        }

        var rawData = reader.ReadBytes((int)length);

        var layer = CreateLayer(ctx.ParentTypeId, typeId);
        layer.FileOffset = startOffset;
        layer.HasMarker = hasMarker;

        try
        {
            using var dataStream = new MemoryStream(rawData);
            using var dataReader = new BinaryReader(dataStream);
            layer.ParseData(dataReader, length, ctx);

            if (dataReader.BaseStream.Position != dataReader.BaseStream.Length)
            {
                var unconsumed = (int)(dataReader.BaseStream.Length - dataReader.BaseStream.Position);
                System.Diagnostics.Trace.WriteLine(
                    $"WARN: Layer 0x{typeId:X} at offset 0x{startOffset:X} did not consume {unconsumed} bytes — falling back to raw");
                var fallback = new UnknownWorldLayer(typeId);
                fallback.FileOffset = startOffset;
                fallback.HasMarker = hasMarker;
                fallback.RawData = rawData;
                fallback.DataLength = length;
                return fallback;
            }
        }
        catch
        {
            var fallback = new UnknownWorldLayer(typeId);
            fallback.FileOffset = startOffset;
            fallback.HasMarker = hasMarker;
            fallback.RawData = rawData;
            fallback.DataLength = length;
            return fallback;
        }

        return layer;
    }

    public static List<WorldLayer> ParseLayers(byte[] data, uint parentId)
    {
        var layers = new List<WorldLayer>();
        var dataEnd = data.Length;

        using var stream = new MemoryStream(data);
        using var reader = new BinaryReader(stream);

        while (reader.BaseStream.Position < dataEnd)
        {
            var ctx = new WorldParseContext(parentId);
            var layer = ParseChild(reader, ctx, dataEnd);
            if (layer == null)
            {
                break;
            }

            layers.Add(layer);
        }

        return layers;
    }

    private static WorldLayer CreateLayer(uint parentId, uint typeId)
    {
        if (parentId == 0x30000)
        {
            return CreateRootLevelLayer(typeId);
        }

        if (parentId == 0x20100)
        {
            return CreateDefaultEnvChild(typeId);
        }

        if (parentId == 0x20200)
        {
            return CreateMeldingChild(typeId);
        }

        if (parentId == 0x20400)
        {
            return CreateChunkInfoChild(typeId);
        }

        if (parentId == 0x50001)
        {
            return CreatePropEnvChild(typeId);
        }

        if (parentId == 0x40001 || parentId == 0x40002)
        {
            return CreateChunkLayer(typeId);
        }

        return new UnknownWorldLayer(typeId);
    }

    private static WorldLayer CreateRootLevelLayer(uint typeId) => typeId switch
    {
        0x20000 => new ZoneSkyboxLayer(),
        0x20100 => new ZoneDefaultEnvironmentLayer(),
        0x20200 => new ZoneMeldingLayer(),
        0x20300 => new ZoneWaterLayer(),
        0x20400 => new ZoneChunkInfoLayer(),
        0x20500 => new SkippedWorldLayer(typeId),
        0x20600 => new SkippedWorldLayer(typeId),
        0x20700 => new ZoneMeldingHeightMapLayer(),
        0x20800 => new ZonePathLayer(),
        0x20900 => new ZoneWorldChunkImportLayer(),
        0x21000 => new ZoneBoundsLayer(),
        0x21100 => new SkippedWorldLayer(typeId),
        0x21200 => new ZonePropEncounterNameRegistryLayer(),
        0x21300 => new ZonePropDoodadLayer(typeId),
        0x21400 => new ZonePropDoodadLayer(typeId),
        0x21500 => new ZoneCameraSequenceLayer(),
        0x21600 => new ZoneTransferBoundsLayer(),
        0x21700 => new ZoneSubZoneRegionLayer(),
        _ => new UnknownWorldLayer(typeId)
    };

    private static WorldLayer CreateDefaultEnvChild(uint typeId) => typeId switch
    {
        0x2710 => new Env10000Layer(),
        0x50001 => new PropEnvironmentLayer(),
        _ => new EnvSubLayer(typeId)
    };

    private static WorldLayer CreateMeldingChild(uint typeId) => typeId switch
    {
        5 => new MeldingPerimeterLayer(),
        _ => new UnknownWorldLayer(typeId)
    };

    private static WorldLayer CreateChunkInfoChild(uint typeId) => typeId switch
    {
        0x10000 => new ZoneChunkRangeLayer(),
        0x10100 => new ZoneChunkRef2Layer(),
        0x10101 => new ZoneChunkRefLayer(),
        _ => new UnknownWorldLayer(typeId)
    };

    private static WorldLayer CreatePropEnvChild(uint typeId) => typeId switch
    {
        _ => new EnvSubLayer(typeId)
    };

    private static WorldLayer CreateChunkLayer(uint typeId) => typeId switch
    {
        0x40101 => new Collision.ChunkStaticGeometryCollisionLayer(),
        0x40102 => new ChunkSubZoneGridLayer(),
        0x40103 => new Collision.ChunkMovementBlockerCollisionLayer(),
        0x40104 => new ChunkPropEncounterNameRegistryLayer(),
        0x40105 => new Collision.ChunkWaterCollisionLayer(),
        0x40204 => new ChunkPropEncounterNameRegistryLayer(),
        _ => new UnknownWorldLayer(typeId)
    };
}
