using Shared.Collision.Layers;

namespace Shared.Collision.Chunk;

public class ChunkLod
{
    public byte Level { get; set; }
    public byte Subdiv { get; set; }
    public uint CompressedSharedSize { get; set; }
    public uint UncompressedSharedSize { get; set; }
    public byte[]? SharedDecompressed { get; set; }
    public List<WorldLayer> SharedLayers { get; set; } = [];
    public List<ChunkSubChunk> SubChunks { get; set; } = [];
}
