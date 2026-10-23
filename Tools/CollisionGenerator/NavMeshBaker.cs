using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities;
using BepuUtilities.Memory;
using DotRecast.Core;
using DotRecast.Core.Numerics;
using DotRecast.Detour;
using DotRecast.Detour.Io;
using FauFau.Formats;
using Serilog;
using Shared.Collision.Navigation;
using Shared.Collision.ZoneLoading;

namespace CollisionGenerator;

public static class NavMeshBaker
{
    private static readonly ILogger _logger = Log.ForContext(typeof(NavMeshBaker));

    public static void Bake(uint zoneId, string mapsPath, string cachePath, bool objYUp = false, bool exportInputObj = false)
    {
        var zoneFilePath = Path.Combine(mapsPath, $"{zoneId}.zone");
        if (!File.Exists(zoneFilePath))
        {
            _logger.Error("Zone file not found: {Path}", zoneFilePath);
            return;
        }

        var zone = new Zone();
        zone.Read(zoneFilePath);
        if (zone.Magic != "ZONE" || zone.Root?.Id != WorldLayerIds.ZoneRoot)
        {
            _logger.Error("Invalid zone root layer for zone {ZoneId}", zoneId);
            return;
        }

        var chunkRefs = ChunkOriginCalculator.ExtractChunks(zone, zoneId);
        _logger.Information("Zone {ZoneId}: {Count} chunks", zoneId, chunkRefs.Length);

        var navmeshDir = Path.Combine(cachePath, "navmesh");
        Directory.CreateDirectory(navmeshDir);

        var allTriangles = new List<NavTriangle>();
        var chunkTriangleRanges = new List<(string Name, int Start, int Count)>();
        var builderParams = new NavMeshBuildParams();
        var audit = new Dictionary<int, WindingCounts>();
        var pool = new BufferPool();
        var disp = new ThreadDispatcher(1);
        var sim = Simulation.Create(pool, new NarrowPhaseCallbacks(), new PoseIntegratorCallbacks(new Vector3(0, 0, -8)), new SolveDescription(8, 1));

        try
        {
            foreach (var chunkRef in chunkRefs)
            {
                var gtchunkPath = Path.Combine(mapsPath, "chunks", $"{chunkRef.Name}.gtchunk");
                if (!File.Exists(gtchunkPath))
                {
                    _logger.Warning("Chunk file not found: {Path}", gtchunkPath);
                    continue;
                }

                var statics = ChunkProcessor.ProcessChunk(gtchunkPath, cachePath, sim, pool, disp, true);
                _logger.Information("Chunk {Name}: {Count} statics", chunkRef.Name, statics.Length);

                var triangulator = new ShapeTriangulator(sim.Shapes, builderParams.PrimitiveRadialSegments);
                int startIdx = allTriangles.Count;
                for (int i = 0; i < statics.Length; i++)
                {
                    var s = statics[i];
                    s.Pose.Position += chunkRef.Origin;
                    var before = allTriangles.Count;
                    triangulator.Triangulate(in s, allTriangles);
                    AccumulateWindingAudit(audit, s.Shape.Type, allTriangles, before, allTriangles.Count);
                }

                int count = allTriangles.Count - startIdx;
                if (count > 0)
                {
                    chunkTriangleRanges.Add((chunkRef.Name, startIdx, count));
                }
            }
        }
        finally
        {
            sim.Dispose();
            pool.Clear();
            disp.Dispose();
        }

        _logger.Information("Total triangles: {Count}", allTriangles.Count);
        if (allTriangles.Count == 0)
        {
            _logger.Warning("No triangles to bake — aborting.");
            return;
        }

        var worldMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        var worldMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        for (int i = 0; i < allTriangles.Count; i++)
        {
            worldMin = Vector3.Min(worldMin, allTriangles[i].Min);
            worldMax = Vector3.Max(worldMax, allTriangles[i].Max);
        }

        _logger.Information(
            "World bounds: Min=({Mnx},{Mny},{Mnz}) Max=({Mxx},{Mxy},{Mxz})",
            worldMin.X,
            worldMin.Y,
            worldMin.Z,
            worldMax.X,
            worldMax.Y,
            worldMax.Z);

        if (exportInputObj)
        {
            var inputDir = Path.Combine(navmeshDir, "input");
            Directory.CreateDirectory(inputDir);
            foreach (var (name, start, count) in chunkTriangleRanges)
            {
                var slice = allTriangles.GetRange(start, count);
                ExportInputObj(slice, Path.Combine(inputDir, $"{name}.obj"), objYUp);
            }
        }

        var builder = new NavMeshTileBuilder(builderParams);
        var buildResult = builder.Build(allTriangles);
        var tiles = buildResult.Tiles;
        _logger.Information("Built {Count} navmesh tiles (grid: {Gw}x{Gh})", tiles.Count, buildResult.GridWidth, buildResult.GridHeight);

        LogWindingAudit(audit, builderParams.WalkableSlopeAngle, buildResult);
        if (tiles.Count == 0)
        {
            _logger.Error(
                "No navmesh tiles produced from {Triangles} triangles ({Walkable} walkable, {UpRejected} rejected for winding, {Steep} steep) - aborting.",
                allTriangles.Count,
                buildResult.WalkableTriangles,
                buildResult.UpFacingRejectedTriangles,
                buildResult.SteepTriangles);
            return;
        }

        int maxTiles = NextPow2(buildResult.GridWidth * buildResult.GridHeight);

        var navMesh = new DtNavMesh();
        var meshParams = new DtNavMeshParams
        {
            orig = buildResult.GridOrigin,
            tileWidth = 128f,
            tileHeight = 128f,
            maxTiles = maxTiles,
            maxPolys = buildResult.MaxPolys,
        };

        if (navMesh.Init(meshParams, 6).Failed())
        {
            _logger.Error("Failed to init DtNavMesh");
            return;
        }

        int totalPolys = 0;
        foreach (var (tx, tz, data) in tiles)
        {
            if (navMesh.AddTile(data, 0, 0, out _).Failed())
            {
                _logger.Error("Failed to add tile ({Tx},{Tz}) to DtNavMesh", tx, tz);
                return;
            }

            totalPolys += data.header.polyCount;
            _logger.Information("Tile ({Tx},{Tz}): {Polys} polys, {Verts} verts", tx, tz, data.header.polyCount, data.header.vertCount);
        }

        var navPath = Path.Combine(navmeshDir, $"{zoneId}.nav");
        using (var fs = File.Create(navPath))
        using (var bw = new BinaryWriter(fs))
        {
            new DtMeshSetWriter().Write(bw, navMesh, RcByteOrder.LITTLE_ENDIAN, false);
        }

        _logger.Information("Navmesh written: {Path} ({Tiles} tiles, {Polys} polys, {DetailTris} detail tris)", navPath, tiles.Count, totalPolys, buildResult.DetailTriCount);

        var metadata = new NavMeshMetadata
        {
            ParamsHash = builderParams.ComputeHash(),
            ZoneId = zoneId,
            TileCount = tiles.Count,
            PolyCount = totalPolys,
            DetailTriCount = buildResult.DetailTriCount,
            BuiltUtc = DateTime.UtcNow,
        };

        metadata.Save(navmeshDir, zoneId);
        _logger.Information("Navmesh metadata written: {Path} (params hash {Hash})", NavMeshMetadata.PathFor(navmeshDir, zoneId), metadata.ParamsHash);

        ExportObj(tiles.ToList(), Path.Combine(navmeshDir, $"{zoneId}.obj"), objYUp);
    }

    public static void SelfTest(uint zoneId, string cachePath)
    {
        var navPath = Path.Combine(cachePath, "navmesh", $"{zoneId}.nav");
        if (!File.Exists(navPath))
        {
            _logger.Error("Navmesh file not found: {Path}", navPath);
            return;
        }

        using var fs = File.OpenRead(navPath);
        using var br = new BinaryReader(fs);
        var navMesh = new DtMeshSetReader().Read(br, 64);

        int tileCount = 0;
        for (int i = 0; i < navMesh.GetMaxTiles(); i++)
        {
            if (navMesh.GetTile(i)?.data != null)
            {
                tileCount++;
            }
        }

        _logger.Information("Self-test: loaded {Tiles} tiles from {Path}", tileCount, navPath);
        if (tileCount == 0)
        {
            _logger.Error("Self-test FAILED: no tiles loaded");
            return;
        }

        var query = new DtNavMeshQuery(navMesh);
        var filter = new DtQueryDefaultFilter();
        var ext = new RcVec3f(200, 200, 200);

        RcVec3f? startPt = null;
        long startRef = 0;
        for (int i = 0; i < navMesh.GetMaxTiles(); i++)
        {
            var tile = navMesh.GetTile(i);
            if (tile?.data?.header == null)
            {
                continue;
            }

            var center = new RcVec3f(
                (tile.data.header.bmin.X + tile.data.header.bmax.X) / 2f,
                (tile.data.header.bmin.Y + tile.data.header.bmax.Y) / 2f,
                (tile.data.header.bmin.Z + tile.data.header.bmax.Z) / 2f);

            if (!query.FindNearestPoly(center, ext, filter, out var ref_, out _, out _).Failed() && ref_ != 0)
            {
                startPt = center;
                startRef = ref_;
                break;
            }
        }

        if (startRef == 0)
        {
            _logger.Error("Self-test FAILED: no walkable polygons found");
            return;
        }

        _logger.Information("Start poly found near ({X:F1},{Y:F1},{Z:F1})", startPt!.Value.X, startPt.Value.Y, startPt.Value.Z);

        var target = new RcVec3f(startPt.Value.X + 50, startPt.Value.Y, startPt.Value.Z + 50);
        if (query.FindNearestPoly(target, ext, filter, out var endRef, out _, out _).Failed() || endRef == 0)
        {
            _logger.Information("Self-test PASSED (partial): navmesh loaded and queryable, but no target polygon found");
            return;
        }

        var path = new long[256];
        if (query.FindPath(startRef, endRef, startPt.Value, target, filter, path, out int pathCount, 256).Failed())
        {
            _logger.Error("Self-test FAILED: FindPath failed");
            return;
        }

        var straight = new DtStraightPath[256];
        if (query.FindStraightPath(startPt.Value, target, path, pathCount, straight, out int straightCount, 256, 0).Failed())
        {
            _logger.Error("Self-test FAILED: FindStraightPath failed");
            return;
        }

        _logger.Information("Self-test PASSED: path found with {Count} segments, {PathCount} polys", straightCount, pathCount);
    }

    /// <summary>
    ///     Writes the navmesh as an OBJ. Prefers Recast's detail mesh, which follows the source
    ///     surface, so the result looks like what Recast's editor draws; falls back to fan-triangulating
    ///     the coarse polygons for tiles that have no detail triangles.
    /// </summary>
    private static void ExportObj(List<(int TileX, int TileZ, DtMeshData Data)> tiles, string objPath, bool yUp)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine(yUp ? "# Navmesh OBJ export (Recast space, Y-up)" : "# Navmesh OBJ export (world space, Z-up)");
        int vertexOffset = 0;
        int detailTriangles = 0;
        int polyTriangles = 0;
        foreach (var (_, _, data) in tiles)
        {
            var verts = data.verts;
            for (int i = 0; i < data.header.vertCount; i++)
            {
                if (yUp)
                {
                    sb.AppendLine($"v {verts[i * 3]:F3} {verts[(i * 3) + 1]:F3} {verts[(i * 3) + 2]:F3}");
                }
                else
                {
                    // Recast Y-up → World Z-up: (x, y, z) → (x, z, y)
                    float x = verts[i * 3];
                    float worldZ = verts[(i * 3) + 1];
                    float worldY = verts[(i * 3) + 2];
                    sb.AppendLine($"v {x:F3} {worldY:F3} {worldZ:F3}");
                }
            }

            // Detail vertices are stored after the polygon vertices, and are absolute positions.
            var detailVertBase = vertexOffset + data.header.vertCount;
            for (int i = 0; i < data.header.detailVertCount; i++)
            {
                var dv = data.detailVerts;
                if (yUp)
                {
                    sb.AppendLine($"v {dv[i * 3]:F3} {dv[(i * 3) + 1]:F3} {dv[(i * 3) + 2]:F3}");
                }
                else
                {
                    float x = dv[i * 3];
                    float worldZ = dv[(i * 3) + 1];
                    float worldY = dv[(i * 3) + 2];
                    sb.AppendLine($"v {x:F3} {worldY:F3} {worldZ:F3}");
                }
            }

            for (int p = 0; p < data.header.polyCount; p++)
            {
                var poly = data.polys[p];
                var hasDetail = data.detailMeshes != null && p < data.detailMeshes.Length && data.detailMeshes[p].triCount > 0;
                if (hasDetail)
                {
                    var detail = data.detailMeshes[p];
                    for (int j = 0; j < detail.triCount; j++)
                    {
                        int t = (detail.triBase + j) * 4;

                        // Detail triangle corners below the polygon vertex count refer to the polygon's
                        // own vertices; the rest index the tile's extra detail vertices.
                        var indices = new int[3];
                        for (int c = 0; c < 3; c++)
                        {
                            int corner = data.detailTris[t + c];
                            indices[c] = corner < poly.vertCount
                                ? vertexOffset + poly.verts[corner]
                                : detailVertBase + detail.vertBase + (corner - poly.vertCount);
                        }

                        // OBJ vertex indices are 1-based
                        sb.AppendLine($"f {indices[0] + 1} {indices[1] + 1} {indices[2] + 1}");
                        detailTriangles++;
                    }

                    continue;
                }

                for (int j = 2; j < poly.vertCount; j++)
                {
                    sb.AppendLine($"f {poly.verts[0] + vertexOffset + 1} {poly.verts[j - 1] + vertexOffset + 1} {poly.verts[j] + vertexOffset + 1}");
                    polyTriangles++;
                }
            }

            vertexOffset += data.header.vertCount + data.header.detailVertCount;
        }

        File.WriteAllText(objPath, sb.ToString());
        _logger.Information("OBJ export: {Path} ({Detail} detail tris, {Polys} polygon tris)", objPath, detailTriangles, polyTriangles);
    }

    private static void ExportInputObj(IReadOnlyList<NavTriangle> triangles, string objPath, bool yUp)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine(yUp ? "# Input geometry OBJ (Recast space, Y-up)" : "# Input geometry OBJ (world space, Z-up)");
        int idx = 1;
        foreach (var tri in triangles)
        {
            if (yUp)
            {
                sb.AppendLine($"v {tri.A.X:F3} {tri.A.Z:F3} {tri.A.Y:F3}");
                sb.AppendLine($"v {tri.B.X:F3} {tri.B.Z:F3} {tri.B.Y:F3}");
                sb.AppendLine($"v {tri.C.X:F3} {tri.C.Z:F3} {tri.C.Y:F3}");
            }
            else
            {
                sb.AppendLine($"v {tri.A.X:F3} {tri.A.Y:F3} {tri.A.Z:F3}");
                sb.AppendLine($"v {tri.B.X:F3} {tri.B.Y:F3} {tri.B.Z:F3}");
                sb.AppendLine($"v {tri.C.X:F3} {tri.C.Y:F3} {tri.C.Z:F3}");
            }

            sb.AppendLine($"f {idx} {idx + 1} {idx + 2}");
            idx += 3;
        }

        File.WriteAllText(objPath, sb.ToString());
        _logger.Information("Input OBJ export: {Path} ({Count} tris)", objPath, triangles.Count);
    }

    /// <summary>
    ///     Buckets the triangles a static contributed by the sign of their world-space normal's Z
    ///     component. The bake accepts FacingDown and rejects FacingUp (see NavMeshTileBuilder's
    ///     world Z-up → Recast Y-up axis swap), so FacingUp is the count of surfaces that should be
    ///     walkable but are not.
    /// </summary>
    private static void AccumulateWindingAudit(
        Dictionary<int, WindingCounts> audit,
        int shapeTypeId,
        List<NavTriangle> triangles,
        int start,
        int end)
    {
        if (!audit.TryGetValue(shapeTypeId, out var counts))
        {
            counts = new WindingCounts();
            audit[shapeTypeId] = counts;
        }

        for (int i = start; i < end; i++)
        {
            var tri = triangles[i];
            var normal = Vector3.Cross(tri.B - tri.A, tri.C - tri.A);
            var length = normal.Length();
            if (length < 1e-9f)
            {
                continue;
            }

            counts.Total++;
            var upness = normal.Z / length;

            // Only classify surfaces flat enough to walk on; walls and cliffs are neither up nor down.
            if (MathF.Abs(upness) <= 0.7071f)
            {
                counts.Steep++;
            }
            else if (upness > 0f)
            {
                counts.FacingUp++;
            }
            else
            {
                counts.FacingDown++;
            }
        }
    }

    private static void LogWindingAudit(Dictionary<int, WindingCounts> audit, float slopeThreshold, NavMeshBuildResult result)
    {
        _logger.Information(
            "Recast walkability of the {Total} input triangles: {Walkable} accepted, {UpRejected} rejected with an upward-facing normal, {Steep} rejected as too steep",
            result.WalkableTriangles + result.UpFacingRejectedTriangles + result.SteepTriangles,
            result.WalkableTriangles,
            result.UpFacingRejectedTriangles,
            result.SteepTriangles);

        if (result.WalkableTriangles == 0)
        {
            _logger.Error(
                "ZERO walkable triangles — the input winding convention does not match the bake's. No navmesh will be produced.");
        }

        if (result.UpFacingRejectedTriangles > result.WalkableTriangles)
        {
            _logger.Warning(
                "More surfaces were rejected for facing up ({Up}) than accepted ({Accepted}) — the bake is generating navmesh on undersides rather than tops.",
                result.UpFacingRejectedTriangles,
                result.WalkableTriangles);
        }

        _logger.Information("Winding audit by shape type (walkable slope limit {Slope:F0} degrees):", slopeThreshold);
        foreach (var (typeId, counts) in audit.OrderByDescending(kv => kv.Value.FacingUp))
        {
            _logger.Information(
                "  {Shape,-12} total {Total,9}  facing-up {Up,9} (LOST)  facing-down {Down,9} (baked)  steep {Steep,9}",
                ShapeTypeName(typeId),
                counts.Total,
                counts.FacingUp,
                counts.FacingDown,
                counts.Steep);
        }
    }

    private static string ShapeTypeName(int typeId)
    {
        if (typeId == Mesh.Id)
        {
            return "Mesh";
        }

        if (typeId == Triangle.Id)
        {
            return "Triangle";
        }

        if (typeId == Box.Id)
        {
            return "Box";
        }

        if (typeId == Sphere.Id)
        {
            return "Sphere";
        }

        if (typeId == Cylinder.Id)
        {
            return "Cylinder";
        }

        if (typeId == Capsule.Id)
        {
            return "Capsule";
        }

        if (typeId == ConvexHull.Id)
        {
            return "ConvexHull";
        }

        if (typeId == Compound.Id)
        {
            return "Compound";
        }

        if (typeId == BigCompound.Id)
        {
            return "BigCompound";
        }

        return $"type#{typeId}";
    }

    private static int NextPow2(int value)
    {
        int p = 1;
        while (p < value)
        {
            p <<= 1;
        }

        return p;
    }

    private sealed class WindingCounts
    {
        public int Total;
        public int FacingUp;
        public int FacingDown;
        public int Steep;
    }
}
