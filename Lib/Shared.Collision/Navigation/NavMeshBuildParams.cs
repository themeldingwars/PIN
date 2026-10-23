namespace Shared.Collision.Navigation;

/// <summary>
///     Parameters controlling the navmesh bake (Recast settings) and the
///     tessellation of primitive collision shapes. World units are the
///     physics simulation's units (~meters).
/// </summary>
public sealed record NavMeshBuildParams
{
    /// <summary>Navmesh tile size in world units. Must divide the 512-unit chunk size evenly (16 sub-chunk tiles per chunk).</summary>
    public float TileSize { get; init; } = 128f;

    /// <summary>Size of xz-plane cells in the Recast heightfield.</summary>
    public float CellSize { get; init; } = 0.25f;

    /// <summary>Height of each cell along the y-axis.</summary>
    public float CellHeight { get; init; } = 0.25f;

    /// <summary>Maximum walkable slope in degrees.</summary>
    public float WalkableSlopeAngle { get; init; } = 45f;

    /// <summary>Agent height.</summary>
    public float AgentHeight { get; init; } = 2.0f;

    /// <summary>Agent radius; the walkable area is eroded by this amount.</summary>
    public float AgentRadius { get; init; } = 0.5f;

    /// <summary>Maximum ledge height the agent can climb.</summary>
    public float AgentMaxClimb { get; init; } = 0.9f;

    /// <summary>Minimum region area (world units squared) allowed to form isolated islands.</summary>
    public float MinRegionArea { get; init; } = 10f;

    /// <summary>Regions smaller than this (world units squared) are merged into larger regions.</summary>
    public float MergeRegionArea { get; init; } = 20f;

    /// <summary>Maximum contour edge length along the mesh border (world units).</summary>
    public float EdgeMaxLen { get; init; } = 12f;

    /// <summary>Maximum deviation of a simplified contour from the raw contour (world units).</summary>
    public float EdgeMaxError { get; init; } = 1.3f;

    /// <summary>Maximum number of vertices per navigation polygon.</summary>
    public int MaxVertsPerPoly { get; init; } = 6;

    /// <summary>Detail mesh sampling distance (multiple of cell size).</summary>
    public float DetailSampleDist { get; init; } = 6.5f;

    /// <summary>Detail mesh maximum sampling error (multiple of cell height).</summary>
    public float DetailSampleMaxError { get; init; } = 1f;

    /// <summary>
    ///     Builds Recast's per-polygon detail mesh. Without it the navmesh carries only the coarse
    ///     convex polygons, so <c>DtNavMesh.GetPolyHeight</c> and anything rendering the mesh falls
    ///     back to a flat approximation of each polygon instead of the real surface.
    /// </summary>
    public bool BuildDetailMesh { get; init; } = true;

    /// <summary>Radial segment count used when tessellating cylinder/capsule/sphere shapes.</summary>
    public int PrimitiveRadialSegments { get; init; } = 16;

    /// <summary>
    ///     Builds a stable identity for the parameters. <see cref="NavMeshMetadata" /> stores it next
    ///     to each baked <c>.nav</c> so the game server can tell whether the file was produced by
    ///     these settings and refuse to use a stale one.
    /// </summary>
    /// <returns>A stable 64-bit hash of all walkability and tessellation settings.</returns>
    public long ComputeHash()
    {
        unchecked
        {
            long hash = 17;
            hash = Mix(hash, CellSize);
            hash = Mix(hash, CellHeight);
            hash = Mix(hash, WalkableSlopeAngle);
            hash = Mix(hash, AgentHeight);
            hash = Mix(hash, AgentRadius);
            hash = Mix(hash, AgentMaxClimb);
            hash = Mix(hash, MinRegionArea);
            hash = Mix(hash, MergeRegionArea);
            hash = Mix(hash, EdgeMaxLen);
            hash = Mix(hash, EdgeMaxError);
            hash = Mix(hash, TileSize);
            hash = Mix(hash, DetailSampleDist);
            hash = Mix(hash, DetailSampleMaxError);
            hash = (hash * 31) + MaxVertsPerPoly;
            hash = (hash * 31) + PrimitiveRadialSegments;
            hash = (hash * 31) + (BuildDetailMesh ? 1 : 0);
            return hash;
        }
    }

    private static long Mix(long hash, float value)
    {
        return (hash * 31) + BitConverter.SingleToInt32Bits(value);
    }
}
