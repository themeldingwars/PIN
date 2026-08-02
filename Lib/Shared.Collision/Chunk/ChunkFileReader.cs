using Shared.Collision.Layers;
using SharpCompress.Compressors.LZMA;

namespace Shared.Collision.Chunk;

public static class ChunkFileReader
{
    public const uint ChunkRootType = 0x40000;
    public const uint ChunkLodType = 0x40001;
    public const uint ChunkSubChunkType = 0x40002;

    public static ChunkFile Read(string filePath)
    {
        var data = File.ReadAllBytes(filePath);
        var chunkName = Path.GetFileNameWithoutExtension(filePath);
        var chunk = Read(data);
        chunk.Name = chunkName;
        return chunk;
    }

    public static ChunkFile Read(byte[] data)
    {
        var chunk = new ChunkFile();

        using var stream = new MemoryStream(data);
        using var reader = new BinaryReader(stream);

        if (!GTLayerHeader.TryRead(reader, out var rootType, out var rootLength))
        {
            throw new InvalidDataException("Failed to read root layer header");
        }

        if (rootType != ChunkRootType)
        {
            throw new InvalidDataException($"Expected root layer type 0x{ChunkRootType:X}, got 0x{rootType:X}");
        }

        var rootEnd = reader.BaseStream.Position + rootLength;

        chunk.Version = reader.ReadUInt32();
        chunk.Timestamp = reader.ReadInt64();
        var numLods = reader.ReadUInt32();

        for (uint i = 0; i < numLods; i++)
        {
            if (!GTLayerHeader.TryRead(reader, out var typeId, out var length))
            {
                throw new InvalidDataException("Failed to read LOD layer header");
            }

            if (typeId != ChunkLodType)
            {
                throw new InvalidDataException($"Expected LOD type 0x{ChunkLodType:X}, got 0x{typeId:X}");
            }

            var lodEnd = reader.BaseStream.Position + length;

            var lod = new ChunkLod
            {
                Level = (byte)reader.ReadUInt32(),
                Subdiv = (byte)reader.ReadUInt32()
            };
            reader.ReadUInt32();
            lod.CompressedSharedSize = reader.ReadUInt32();
            lod.UncompressedSharedSize = reader.ReadUInt32();

            while (reader.BaseStream.Position < lodEnd)
            {
                if (!GTLayerHeader.TryRead(reader, out var scTypeId, out var scLength))
                {
                    throw new InvalidDataException("Failed to read subchunk layer header");
                }

                if (scTypeId != ChunkSubChunkType)
                {
                    throw new InvalidDataException($"Expected subchunk type 0x{ChunkSubChunkType:X}, got 0x{scTypeId:X}");
                }

                var sc = new ChunkSubChunk();
                reader.ReadUInt32();
                sc.CompressedSize = reader.ReadUInt32();
                sc.UncompressedSize = reader.ReadUInt32();
                sc.BoundsMin = reader.ReadVec3();
                sc.BoundsMax = reader.ReadVec3();
                sc.Name = SubChunkName((byte)chunk.Lod.Count, (byte)lod.SubChunks.Count);
                lod.SubChunks.Add(sc);
            }

            chunk.Lod.Add(lod);
        }

        var dataStart = 16 + (int)rootLength;
        var offset = dataStart;

        foreach (var lod in chunk.Lod)
        {
            var sharedBlock = new byte[lod.CompressedSharedSize];
            Array.Copy(data, offset, sharedBlock, 0, (int)lod.CompressedSharedSize);
            lod.SharedDecompressed = DecompressBlock(sharedBlock, (int)lod.UncompressedSharedSize);
            lod.SharedLayers = WorldLayerParser.ParseLayers(lod.SharedDecompressed, ChunkLodType);
            offset += (int)lod.CompressedSharedSize;

            foreach (var sc in lod.SubChunks)
            {
                var scBlock = new byte[sc.CompressedSize];
                Array.Copy(data, offset, scBlock, 0, (int)sc.CompressedSize);
                sc.Decompressed = DecompressBlock(scBlock, (int)sc.UncompressedSize);
                sc.Layers = WorldLayerParser.ParseLayers(sc.Decompressed, ChunkSubChunkType);
                offset += (int)sc.CompressedSize;
            }
        }

        if (offset != data.Length)
        {
            throw new InvalidDataException(
                $"Data mismatch: expected 0x{data.Length:X}, got 0x{offset:X}");
        }

        return chunk;
    }

    private static byte[] DecompressBlock(byte[] block, int uncompressedSize)
    {
        if (block.Length < 4)
        {
            throw new InvalidDataException("Truncated compressed block");
        }

        var magic = System.Text.Encoding.ASCII.GetString(block[..4]);

        if (magic == "DATA")
        {
            var payload = new byte[block.Length - 4];
            Array.Copy(block, 4, payload, 0, payload.Length);
            return DecompressZlib(payload);
        }
        else if (magic == "DAT2")
        {
            return DecompressDat2Block(block, uncompressedSize);
        }
        else
        {
            throw new InvalidDataException($"Unknown compression magic: {magic}");
        }
    }

    private static byte[] DecompressZlib(byte[] data)
    {
        using var input = new MemoryStream(data);
        using var zlib = new ICSharpCode.SharpZipLib.Zip.Compression.Streams.InflaterInputStream(input);
        using var output = new MemoryStream();
        zlib.CopyTo(output);
        return output.ToArray();
    }

    private static byte[] DecompressDat2Block(byte[] block, int uncompressedSize)
    {
        if (block.Length < 4 + 5)
        {
            throw new InvalidDataException("DAT2 block too small");
        }

        var properties = new byte[] { block[4], block[5], block[6], block[7], block[8] };
        var payloadStart = 4 + 5;

        using var payloadStream = new MemoryStream(block, payloadStart, block.Length - payloadStart);
        using var lzmaStream = LzmaStream.Create(properties, payloadStream, payloadStream.Length, uncompressedSize, true);

        var decompressed = new byte[uncompressedSize];
        lzmaStream.ReadExactly(decompressed);
        return decompressed;
    }

    private static string SubChunkName(byte lod, byte subIndex)
    {
        if (lod == 0 || lod == 1)
        {
            return "0x0";
        }

        if (lod == 2)
        {
            return $"{subIndex % 2}x{subIndex / 2}";
        }

        if (lod == 3)
        {
            return $"{subIndex % 4}x{subIndex / 4}";
        }

        if (lod == 4)
        {
            return $"{subIndex % 8}x{subIndex / 8}";
        }

        var grid = 1 << (lod - 1);
        return $"{subIndex % grid}x{subIndex / grid}";
    }
}
