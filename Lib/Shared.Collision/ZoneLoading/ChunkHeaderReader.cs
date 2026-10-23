using Bitter;
using FauFau.Formats;

namespace Shared.Collision.ZoneLoading;

/// <summary>
///     Reads only the gtchunk header structure (the root layer plus its LOD and subchunk layer headers)
///     without reading or decompressing any data block. The subchunk bounds live in those headers, so the
///     zone spatial index can be built without paying for the full <see cref="GtChunkV8.Load" /> path,
///     which pulls every compressed block into memory.
/// </summary>
public static class ChunkHeaderReader
{
    public static GtChunkV8.RootLayer ReadHeader(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        using var binary = new BinaryStream(stream);

        var root = new GtChunkV8.RootLayer();
        root.Read(binary);

        if (root.Marker != GtLayer.Marker)
        {
            throw new InvalidDataException($"Expected layer marker 0x{GtLayer.Marker:X}, got 0x{root.Marker:X}");
        }

        if (root.Id != WorldLayerIds.ChunkRoot)
        {
            throw new InvalidDataException($"Expected root layer type 0x{WorldLayerIds.ChunkRoot:X}, got 0x{root.Id:X}");
        }

        if (root.Version != GtChunkV8.VERSION)
        {
            throw new InvalidDataException($"Expected chunk version {GtChunkV8.VERSION}, got {root.Version}");
        }

        return root;
    }
}
