using System.Numerics;
using FauFau.Formats;

namespace Shared.Collision.ZoneLoading;

/// <summary>
///     Read-mostly, thread-safe index mapping a world position to the zone chunk + subchunk that
///     contain it. Built incrementally on the shard thread during zone load (one <see cref="AddChunk" />
///     per chunk) and published atomically via <see cref="Publish" /> so the physics debug renderer
///     (running on its own thread) can resolve the camera position without any locking. The
///     <see cref="TryResolve" /> path is allocation-free and lock-free: it reads a volatile snapshot
///     of the published lists and returns either the old or the new index, never a torn one.
/// </summary>
public sealed class ZoneSpatialIndex
{
    private readonly List<SubChunkEntry> _pendingSubChunks = [];
    private readonly List<ChunkCell> _pendingChunkCells = [];

    // Snapshot arrays, swapped atomically on publish (volatile gives the cross-thread memory barrier so
    // a reader on the render thread sees the fully-populated array only after the shard thread finished
    // writing it). Readers see a consistent pair, never mid-build.
    private volatile SubChunkEntry[] _subChunks = Array.Empty<SubChunkEntry>();
    private volatile ChunkCell[] _chunkCells = Array.Empty<ChunkCell>();

    public int SubChunkCount => _subChunks.Length;
    public int ChunkCount => _chunkCells.Length;

    /// <summary>
    ///     Records every subchunk (with its chunk origin applied, so bounds are world-space) for one
    ///     chunk. The finest LOD level is preferred; when a level has a single subchunk it is shared by
    ///     all levels, so only one entry is recorded for it. Takes the header-only root read by
    ///     <see cref="ChunkHeaderReader" /> — no collision data is needed for the bounds.
    /// </summary>
    /// <param name="chunkRef">The chunk reference (name + world origin) being indexed.</param>
    /// <param name="root">The chunk's header root layer, providing the LOD/subchunk bounds.</param>
    public void AddChunk(ZoneChunkRef chunkRef, GtChunkV8.RootLayer root)
    {
        var lods = root.LodLayers;
        if (lods == null || lods.Length == 0)
        {
            return;
        }

        var origin = chunkRef.Origin;
        var finest = lods[^1];
        var subChunks = finest.SubChunkLayers;
        if (subChunks == null)
        {
            return;
        }

        // Chunk-level fallback cell (full 512x512 X-Y footprint) so a position that falls in the gaps
        // between tight subchunk bounds still resolves to its chunk.
        _pendingChunkCells.Add(new ChunkCell
        {
            ChunkName = chunkRef.Name,
            MinX = origin.X,
            MinY = origin.Y,
            MaxX = origin.X + 512f,
            MaxY = origin.Y + 512f,
        });

        var lodLevel = (byte)(lods.Length - 1);
        for (var i = 0; i < subChunks.Length; i++)
        {
            var sub = subChunks[i];
            _pendingSubChunks.Add(new SubChunkEntry
            {
                ChunkName = chunkRef.Name,
                SubChunkName = SubChunkName(lodLevel, (byte)i),
                Min = origin + sub.BoundsMin,
                Max = origin + sub.BoundsMax,
            });
        }
    }

    /// <summary>
    ///     Atomically publishes the accumulated chunk/subchunk data for concurrent readers.
    /// </summary>
    public void Publish()
    {
        // Publish the larger array first so a reader that sees the new subchunks always also sees the
        // matching chunk cells (a stale-but-consistent pair is safe in either order).
        _chunkCells = _pendingChunkCells.ToArray();
        _subChunks = _pendingSubChunks.ToArray();
        _pendingSubChunks.Clear();
        _pendingChunkCells.Clear();
    }

    /// <summary>
    ///     Resolves a world position to the chunk + subchunk that contain it. Subchunk is the finest
    ///     level of detail that covers the point; when the point is inside the chunk cell but outside
    ///     every tight subchunk bound, the subchunk is reported as <c>?</c>. Returns false (and leaves
    ///     both out values null) when the position is outside every loaded chunk cell.
    /// </summary>
    /// <param name="world">The world position to resolve.</param>
    /// <param name="chunkName">Receives the containing chunk name, or null when the position is unresolved.</param>
    /// <param name="subChunkName">Receives the finest covering subchunk label (<c>?</c> when only the chunk cell matches), or null when unresolved.</param>
    /// <returns>True when the position falls inside a loaded chunk; false otherwise.</returns>
    public bool TryResolve(Vector3 world, out string? chunkName, out string? subChunkName)
    {
        var subChunks = _subChunks;
        for (var i = 0; i < subChunks.Length; ++i)
        {
            ref var e = ref subChunks[i];
            if (world.X >= e.Min.X && world.X <= e.Max.X &&
                world.Y >= e.Min.Y && world.Y <= e.Max.Y &&
                world.Z >= e.Min.Z && world.Z <= e.Max.Z)
            {
                chunkName = e.ChunkName;
                subChunkName = e.SubChunkName;
                return true;
            }
        }

        var cells = _chunkCells;
        for (var i = 0; i < cells.Length; ++i)
        {
            ref var c = ref cells[i];
            if (world.X >= c.MinX && world.X <= c.MaxX &&
                world.Y >= c.MinY && world.Y <= c.MaxY)
            {
                chunkName = c.ChunkName;
                subChunkName = "?";
                return true;
            }
        }

        chunkName = null;
        subChunkName = null;
        return false;
    }

    /// <summary>
    ///     Display name for a subchunk: the gtchunk subchunk blocks are stored in row-major order for a
    ///     (2^(lod-1))^2 grid, so the index maps back to a "columnxrow" label. LOD 0/1 have a single
    ///     shared subchunk.
    /// </summary>
    /// <param name="lod">The LOD level the subchunk belongs to.</param>
    /// <param name="subIndex">The row-major subchunk index within that LOD level.</param>
    /// <returns>The "columnxrow" display label for the subchunk.</returns>
    private static string SubChunkName(byte lod, byte subIndex)
    {
        if (lod == 0 || lod == 1)
        {
            return "0x0";
        }

        var grid = 1 << (lod - 1);
        return $"{subIndex % grid}x{subIndex / grid}";
    }

    private struct SubChunkEntry
    {
        public string ChunkName;
        public string SubChunkName;

        // world-space
        public Vector3 Min;

        // world-space
        public Vector3 Max;
    }

    private struct ChunkCell
    {
        public string ChunkName;
        public float MinX;
        public float MinY;
        public float MaxX;
        public float MaxY;
    }
}
