using System.Numerics;
using DotRecast.Core.Numerics;
using DotRecast.Detour;
using DotRecast.Recast;
using DotRecast.Recast.Geom;

namespace Shared.Collision.Navigation;

/// <summary>
///     The result of a navmesh build: all non-empty tiles plus the grid metadata
///     needed to initialize the <see cref="DtNavMesh" />.
/// </summary>
public sealed record NavMeshBuildResult
{
    public required IReadOnlyList<(int TileX, int TileZ, DtMeshData Data)> Tiles { get; init; }
    public required RcVec3f GridOrigin { get; init; }
    public required int GridWidth { get; init; }
    public required int GridHeight { get; init; }
    public int MaxPolys { get; init; }

    /// <summary>Source triangles Recast accepted as walkable (their winding-derived normal clears the slope test).</summary>
    public int WalkableTriangles { get; init; }

    /// <summary>
    ///     Source triangles Recast rejected because their winding-derived normal points away from
    ///     the up axis. A large count here relative to <see cref="WalkableTriangles" /> means the
    ///     input winding convention does not match what the bake expects.
    /// </summary>
    public int UpFacingRejectedTriangles { get; init; }

    /// <summary>Source triangles rejected as too steep to walk on (walls, cliffs). Expected to be common.</summary>
    public int SteepTriangles { get; init; }

    /// <summary>Total detail-mesh triangles carried by the tiles, 0 when <c>BuildDetailMesh</c> is off.</summary>
    public int DetailTriCount { get; init; }
}

/// <summary>
///     Builds a tiled <see cref="DtNavMesh" /> from world-space <see cref="NavTriangle" />
///     using Recast's native tiled pipeline. All tiles are built in a single pass with
///     guaranteed border polygon matching between adjacent tiles.
/// </summary>
public sealed class NavMeshTileBuilder
{
    private readonly NavMeshBuildParams _params;
    private readonly RcBuilder _builder;
    private readonly RcAreaModification _walkableArea;

    public NavMeshTileBuilder(NavMeshBuildParams parameters)
    {
        _params = parameters;
        _builder = new RcBuilder();
        _walkableArea = new RcAreaModification(RcRecast.RC_WALKABLE_AREA);
    }

    /// <summary>
    ///     Builds all navmesh tiles from the given world-space triangles (Z-up).
    ///     Uses Recast's native tiled pipeline for correct border connectivity.
    /// </summary>
    /// <param name="triangles">The world-space source triangles (Z-up) to bake into the navmesh.</param>
    /// <returns>The built navmesh tiles together with the tile-grid origin and dimensions.</returns>
    public NavMeshBuildResult Build(IReadOnlyList<NavTriangle> triangles)
    {
        var (verts, faces, walkable, upFacingRejected, steep) = BuildVertexFaceArrays(triangles, _params.WalkableSlopeAngle);
        if (faces.Length == 0)
        {
            return new NavMeshBuildResult
            {
                Tiles = [],
                GridOrigin = new RcVec3f(0, 0, 0),
                GridWidth = 0,
                GridHeight = 0,
                MaxPolys = 0,
            };
        }

        var geom = new RcSampleInputGeomProvider(verts, faces);
        var cfg = CreateRcConfig();
        var results = _builder.BuildTiles(geom, cfg, false, true, Environment.ProcessorCount);

        var tiles = new List<(int, int, DtMeshData)>();
        int maxPolys = 0;
        int detailTris = 0;

        foreach (var result in results)
        {
            var pmesh = result.Mesh;
            if (pmesh.npolys == 0)
            {
                continue;
            }

            for (int i = 0; i < pmesh.npolys; i++)
            {
                pmesh.flags[i] = 1;
            }

            var createParams = new DtNavMeshCreateParams
            {
                verts = pmesh.verts,
                vertCount = pmesh.nverts,
                polys = pmesh.polys,
                polyFlags = pmesh.flags,
                polyAreas = pmesh.areas,
                polyCount = pmesh.npolys,
                nvp = pmesh.nvp,
                bmin = pmesh.bmin,
                bmax = pmesh.bmax,
                cs = pmesh.cs,
                ch = pmesh.ch,
                walkableHeight = _params.AgentHeight,
                walkableRadius = _params.AgentRadius,
                walkableClimb = _params.AgentMaxClimb,
                tileX = result.TileX,
                tileZ = result.TileZ,
                tileLayer = 0,
                userId = 0,
                buildBvTree = true,
            };

            ApplyDetailMesh(result.MeshDetail, createParams);

            var data = DtNavMeshBuilder.CreateNavMeshData(createParams);
            if (data != null)
            {
                tiles.Add((result.TileX, result.TileZ, data));
                maxPolys = Math.Max(maxPolys, data.header.polyCount);
                detailTris += createParams.detailTriCount;
            }
        }

        RcRecast.CalcTileCount(geom.GetMeshBoundsMin(), geom.GetMeshBoundsMax(), cfg.Cs, cfg.TileSizeX, cfg.TileSizeZ, out int gw, out int gh);

        return new NavMeshBuildResult
        {
            Tiles = tiles,
            GridOrigin = geom.GetMeshBoundsMin(),
            GridWidth = gw,
            GridHeight = gh,
            MaxPolys = maxPolys,
            WalkableTriangles = walkable,
            UpFacingRejectedTriangles = upFacingRejected,
            SteepTriangles = steep,
            DetailTriCount = detailTris,
        };
    }

    /// <summary>
    ///     Converts world-space (Z-up) triangles into Recast's vertex/face arrays (Y-up), and counts
    ///     how Recast's own walkability test classifies each triangle.
    ///     Recast derives the triangle normal from winding alone - <c>normalize(cross(v1-v0, v2-v0))</c> -
    ///     and requires <c>normal.Y &gt; cos(walkableSlope)</c>, so the input winding convention matters.
    /// </summary>
    private static (float[] Verts, int[] Faces, int Walkable, int UpFacingRejected, int Steep) BuildVertexFaceArrays(
        IReadOnlyList<NavTriangle> triangles,
        float walkableSlopeAngle)
    {
        int triCount = triangles.Count;
        var verts = new float[triCount * 9];
        var faces = new int[triCount * 3];

        // Same threshold RcRecast.MarkWalkableTriangles uses.
        var slopeThreshold = MathF.Cos(walkableSlopeAngle * MathF.PI / 180f);
        int walkable = 0;
        int upFacingRejected = 0;
        int steep = 0;

        for (int i = 0; i < triCount; i++)
        {
            var tri = triangles[i];
            int v = i * 9;
            int f = i * 3;

            // World Z-up → Recast Y-up: (x, y, z) → (x, z, y)
            // NOTE: this swap is a reflection (determinant -1), so it flips triangle winding. Recast
            // derives the normal from winding alone and requires normal.Y > cos(walkableSlope), which
            // means a surface an agent can walk on must arrive here with its world-space normal
            // pointing DOWN (-Z). Getting this backwards produces a heightfield full of spans and
            // zero polygons with no error, so the counts below are checked and logged by the baker.
            verts[v] = tri.A.X;
            verts[v + 1] = tri.A.Z;
            verts[v + 2] = tri.A.Y;
            verts[v + 3] = tri.B.X;
            verts[v + 4] = tri.B.Z;
            verts[v + 5] = tri.B.Y;
            verts[v + 6] = tri.C.X;
            verts[v + 7] = tri.C.Z;
            verts[v + 8] = tri.C.Y;

            // Face order is what sets the normal, so it must be emitted after the vertices.
            faces[f] = i * 3;
            faces[f + 1] = (i * 3) + 1;
            faces[f + 2] = (i * 3) + 2;

            var normalY = RecastNormalY(verts, v);
            if (normalY > slopeThreshold)
            {
                walkable++;
            }
            else if (normalY < -slopeThreshold)
            {
                upFacingRejected++;
            }
            else
            {
                steep++;
            }
        }

        return (verts, faces, walkable, upFacingRejected, steep);
    }

    /// <summary>
    ///     Y component of the winding-derived normal Recast computes for the triangle stored at
    ///     <paramref name="vertexOffset" />. Mirrors RcRecast.CalcTriNormal: normalize(cross(v1-v0, v2-v0)).
    /// </summary>
    private static float RecastNormalY(float[] verts, int vertexOffset)
    {
        var e0x = verts[vertexOffset + 3] - verts[vertexOffset];
        var e0y = verts[vertexOffset + 4] - verts[vertexOffset + 1];
        var e0z = verts[vertexOffset + 5] - verts[vertexOffset + 2];
        var e1x = verts[vertexOffset + 6] - verts[vertexOffset];
        var e1y = verts[vertexOffset + 7] - verts[vertexOffset + 1];
        var e1z = verts[vertexOffset + 8] - verts[vertexOffset + 2];

        var nx = (e0y * e1z) - (e0z * e1y);
        var ny = (e0z * e1x) - (e0x * e1z);
        var nz = (e0x * e1y) - (e0y * e1x);
        var length = MathF.Sqrt((nx * nx) + (ny * ny) + (nz * nz));
        return length < 1e-12f ? 0f : ny / length;
    }

    /// <summary>
    ///     Copies Recast's per-polygon detail mesh into the flattened arrays
    ///     <see cref="DtNavMeshCreateParams" /> expects. DotRecast stores detail vertices as
    ///     (x, y, z) rather than C++ recast's (x, y, z, area), which is already the stride the create
    ///     parameters want, so the arrays copy straight across. They are capacity-sized, so only the
    ///     first <c>nmeshes</c>/<c>nverts</c>/<c>ntris</c> records are taken. Without this the navmesh
    ///     carries only the coarse convex polygons, and <c>DtNavMesh.GetPolyHeight</c> falls back to a
    ///     flat approximation of each polygon.
    /// </summary>
    private static void ApplyDetailMesh(RcPolyMeshDetail detail, DtNavMeshCreateParams createParams)
    {
        if (detail?.meshes == null || detail.verts == null || detail.tris == null)
        {
            return;
        }

        // The builder indexes detailMeshes by polygon index, so a partial record set would read past
        // the end of it. Anything short of one record per polygon means no detail mesh at all.
        int meshCount = Math.Min(detail.nmeshes, detail.meshes.Length / 4);
        int vertCount = Math.Min(detail.nverts, detail.verts.Length / 3);
        int triCount = Math.Min(detail.ntris, detail.tris.Length / 4);
        if (meshCount != createParams.polyCount || vertCount == 0 || triCount == 0)
        {
            return;
        }

        createParams.detailMeshes = new int[meshCount * 4];
        Array.Copy(detail.meshes, 0, createParams.detailMeshes, 0, meshCount * 4);

        createParams.detailVerts = new float[vertCount * 3];
        Array.Copy(detail.verts, 0, createParams.detailVerts, 0, vertCount * 3);
        createParams.detailVertsCount = vertCount;

        createParams.detailTris = new int[triCount * 4];
        Array.Copy(detail.tris, 0, createParams.detailTris, 0, triCount * 4);
        createParams.detailTriCount = triCount;
    }

    private RcConfig CreateRcConfig()
    {
        int tileCells = (int)(_params.TileSize / _params.CellSize);
        int borderCells = RcConfig.CalcBorder(_params.AgentRadius, _params.CellSize);

        return new RcConfig(
            useTiles: true,
            tileSizeX: tileCells,
            tileSizeZ: tileCells,
            borderSize: borderCells,
            partition: RcPartition.WATERSHED,
            cellSize: _params.CellSize,
            cellHeight: _params.CellHeight,
            agentMaxSlope: _params.WalkableSlopeAngle,
            agentHeight: _params.AgentHeight,
            agentRadius: _params.AgentRadius,
            agentMaxClimb: _params.AgentMaxClimb,
            minRegionArea: _params.MinRegionArea,
            mergeRegionArea: _params.MergeRegionArea,
            edgeMaxLen: _params.EdgeMaxLen,
            edgeMaxError: _params.EdgeMaxError,
            vertsPerPoly: _params.MaxVertsPerPoly,
            detailSampleDist: _params.DetailSampleDist,
            detailSampleMaxError: _params.DetailSampleMaxError,
            filterLowHangingObstacles: true,
            filterLedgeSpans: true,
            filterWalkableLowHeightSpans: true,
            walkableAreaMod: _walkableArea,
            buildMeshDetail: _params.BuildDetailMesh);
    }
}
