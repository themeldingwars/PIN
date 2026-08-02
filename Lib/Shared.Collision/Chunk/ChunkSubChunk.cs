using Shared.Collision.Layers;

namespace Shared.Collision.Chunk;

public class ChunkSubChunk
{
    public string Name { get; set; } = string.Empty;
    public uint CompressedSize { get; set; }
    public uint UncompressedSize { get; set; }
    public Vec3 BoundsMin { get; set; }
    public Vec3 BoundsMax { get; set; }
    public byte[]? Decompressed { get; set; }
    public List<WorldLayer> Layers { get; set; } = [];
}
