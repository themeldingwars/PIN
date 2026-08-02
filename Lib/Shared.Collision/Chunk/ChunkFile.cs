namespace Shared.Collision.Chunk;

public class ChunkFile
{
    public string Name { get; set; } = string.Empty;
    public uint Version { get; set; }
    public long Timestamp { get; set; }
    public List<ChunkLod> Lod { get; set; } = [];
}
