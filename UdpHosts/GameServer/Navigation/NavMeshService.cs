#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using DotRecast.Core;
using DotRecast.Core.Numerics;
using DotRecast.Detour;
using DotRecast.Detour.Io;
using Serilog;
using Shared.Collision.Navigation;

namespace GameServer.Navigation;

public sealed class NavMeshService
{
    private readonly ILogger _logger;
    private readonly DtQueryDefaultFilter _filter = new();
    private readonly RcVec3f _searchExt = new(200, 200, 200);
    private readonly RcRand _rand = new();
    private readonly object _lock = new();
    private DtNavMesh? _navMesh;
    private DtNavMeshQuery? _query;

    public NavMeshService(ILogger logger)
    {
        _logger = logger;
    }

    public bool IsLoaded { get; private set; }

    /// <summary>
    ///     The loaded mesh, for collaborators that must build their own query on it (the crowd owns its own
    ///     <see cref="DtNavMeshQuery" />). Null when no navmesh is loaded. Reads outside <see cref="_lock" /> are safe
    ///     because the mesh is only assigned during <see cref="Load" />, which runs once at shard construction.
    /// </summary>
    public DtNavMesh? NavMesh => _navMesh;

    // World Z-up → Nav Y-up: (x, y, z) → (x, z, y)
    public static RcVec3f ToRc(Vector3 v)
    {
        return new RcVec3f(v.X, v.Z, v.Y);
    }

    // Nav Y-up → World Z-up: (x, y, z) → (x, z, y)
    public static Vector3 FromRc(RcVec3f v)
    {
        return new Vector3(v.X, v.Z, v.Y);
    }

    public bool Load(uint zoneId, string cachePath)
    {
        var navmeshDir = Path.Combine(cachePath, "navmesh");
        var navPath = Path.Combine(navmeshDir, $"{zoneId}.nav");
        if (!File.Exists(navPath))
        {
            _logger.Information("Navmesh not found at {Path} — navmesh disabled", navPath);
            return false;
        }

        // A navmesh is only valid for the settings it was baked with. Without the sidecar there is no
        // way to know, and a stale mesh silently produces wrong navigation, so it is rejected.
        var parameters = new NavMeshBuildParams();
        var rebakeHint = FormattableString.Invariant(
            $"CollisionGenerator -c {cachePath} -m <maps-path> -z {zoneId} --navmesh --force");
        var metadata = NavMeshMetadata.Load(navmeshDir, zoneId);
        if (metadata == null)
        {
            _logger.Error(
                "Navmesh {Path} has no {Suffix} sidecar, so it cannot be verified against the current bake settings — navmesh disabled. Re-bake with: {Rebake}",
                navPath,
                NavMeshMetadata.Suffix,
                rebakeHint);
            return false;
        }

        if (!metadata.IsCurrentFor(parameters))
        {
            _logger.Error(
                "Navmesh {Path} was baked with different settings (baked params hash {Baked}, current {Current}) — navmesh disabled. Re-bake with: {Rebake}",
                navPath,
                metadata.ParamsHash,
                parameters.ComputeHash(),
                rebakeHint);
            return false;
        }

        try
        {
            using var fs = File.OpenRead(navPath);
            using var br = new BinaryReader(fs);
            _navMesh = new DtMeshSetReader().Read(br);
            _query = new DtNavMeshQuery(_navMesh);

            int tileCount = 0;
            for (int i = 0; i < _navMesh.GetMaxTiles(); i++)
            {
                if (_navMesh.GetTile(i)?.data != null)
                {
                    tileCount++;
                }
            }

            IsLoaded = true;
            _logger.Information(
                "NavMesh loaded: zone={ZoneId} tiles={Tiles} detailTris={DetailTris} from {Path}",
                zoneId,
                tileCount,
                metadata.DetailTriCount,
                navPath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to load navmesh from {Path}", navPath);
            _navMesh = null;
            _query = null;
            IsLoaded = false;
            return false;
        }
    }

    /// <summary>
    ///     Extracts the navmesh's walkable-surface triangles (each poly fan-triangulated) in world Z-up
    ///     space, plus the world-space AABB center and the bounding radius (distance from the center to the
    ///     farthest vertex) used for frustum culling. Returns false when no navmesh is loaded. Used only by
    ///     the physics debug renderer to draw the navmesh overlay; it does not add any shape to the Bepu
    ///     simulation.
    /// </summary>
    /// <param name="triangles">Receives the world-space (Z-up) walkable-surface triangles, fan-triangulated per poly.</param>
    /// <param name="center">Receives the world-space AABB center of the navmesh.</param>
    /// <param name="radius">Receives the bounding radius (center to farthest vertex) used for frustum culling.</param>
    /// <returns>True when a navmesh is loaded and its triangles were extracted; false otherwise.</returns>
    public bool TryExtractTriangles(out List<Vector3> triangles, out Vector3 center, out float radius)
    {
        triangles = new List<Vector3>();
        center = default;
        radius = 0f;
        if (!IsLoaded || _navMesh == null)
        {
            return false;
        }

        var min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        var max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        lock (_lock)
        {
            for (int i = 0; i < _navMesh.GetMaxTiles(); i++)
            {
                var data = _navMesh.GetTile(i)?.data;
                if (data?.header == null)
                {
                    continue;
                }

                var verts = data.verts;
                foreach (var poly in data.polys)
                {
                    if (poly.vertCount < 3)
                    {
                        continue;
                    }

                    for (int j = 2; j < poly.vertCount; j++)
                    {
                        var ia = poly.verts[0] * 3;
                        var ib = poly.verts[j - 1] * 3;
                        var ic = poly.verts[j] * 3;

                        // Recast Y-up → world Z-up: (rx, ry, rz) → (rx, rz, ry), matching FromRc.
                        var a = new Vector3(verts[ia], verts[ia + 2], verts[ia + 1]);
                        var b = new Vector3(verts[ib], verts[ib + 2], verts[ib + 1]);
                        var c = new Vector3(verts[ic], verts[ic + 2], verts[ic + 1]);
                        triangles.Add(a);
                        triangles.Add(b);
                        triangles.Add(c);
                        min = Vector3.Min(min, a);
                        min = Vector3.Min(min, b);
                        min = Vector3.Min(min, c);
                        max = Vector3.Max(max, a);
                        max = Vector3.Max(max, b);
                        max = Vector3.Max(max, c);
                    }
                }
            }
        }

        if (triangles.Count == 0)
        {
            return false;
        }

        center = (min + max) * 0.5f;
        var half = (max - min) * 0.5f;
        radius = half.Length();
        return true;
    }

    public bool FindNearestPoly(Vector3 position, float maxRadius, out long polyRef, out Vector3 closestPoint)
    {
        polyRef = 0;
        closestPoint = position;
        if (!IsLoaded)
        {
            return false;
        }

        lock (_lock)
        {
            var rc = ToRc(position);
            var ext = new RcVec3f(maxRadius, maxRadius, maxRadius);
            if (_query!.FindNearestPoly(rc, ext, _filter, out polyRef, out var nearest, out _).Failed() || polyRef == 0)
            {
                return false;
            }

            closestPoint = FromRc(nearest);
            return true;
        }
    }

    /// <summary>
    ///     Distance from a world position to the nearest point on the walkable mesh, or null when the mesh is not
    ///     loaded. Diagnostic: an NPC far from the mesh can still get a route (the search extent is wide) but that
    ///     route starts from a polygon nowhere near it, which is worth being able to see rather than infer.
    /// </summary>
    /// <param name="position">The world-space position to measure from.</param>
    /// <returns>The distance in world units, or null when no walkable point was found.</returns>
    public float? NearestMeshDistance(Vector3 position)
    {
        return FindNearestPoly(position, 400f, out _, out var closest)
            ? (closest - position).Length()
            : null;
    }

    public List<Vector3>? FindPath(Vector3 from, Vector3 to)
    {
        if (!IsLoaded)
        {
            return null;
        }

        lock (_lock)
        {
            var rcFrom = ToRc(from);
            var rcTo = ToRc(to);

            if (_query!.FindNearestPoly(rcFrom, _searchExt, _filter, out var startRef, out _, out _).Failed() || startRef == 0)
            {
                return null;
            }

            if (_query.FindNearestPoly(rcTo, _searchExt, _filter, out var endRef, out _, out _).Failed() || endRef == 0)
            {
                return null;
            }

            var path = new long[256];
            if (_query.FindPath(startRef, endRef, rcFrom, rcTo, _filter, path, out int pathCount, 256).Failed())
            {
                return null;
            }

            var straight = new DtStraightPath[256];
            if (_query.FindStraightPath(rcFrom, rcTo, path, pathCount, straight, out int straightCount, 256, 0).Failed())
            {
                return null;
            }

            var result = new List<Vector3>(straightCount);
            for (int i = 0; i < straightCount; i++)
            {
                result.Add(FromRc(straight[i].pos));
            }

            return result;
        }
    }

    public bool IsWalkable(Vector3 position)
    {
        return FindNearestPoly(position, 5f, out _, out _);
    }

    /// <summary>
    ///     Picks a random walkable point on the navmesh within <paramref name="radius" /> of <paramref name="center" />
    ///     (polygons chosen weighted by area, strictly constrained to the circle, reachable from the center's poly).
    ///     Used by wander mode to choose a roam target. Returns false when no navmesh is loaded or no reachable
    ///     polygon lies in the circle.
    /// </summary>
    /// <param name="center">The circle center to search around.</param>
    /// <param name="radius">The circle radius the chosen point must lie within.</param>
    /// <param name="point">Receives the chosen random walkable point.</param>
    /// <returns>True when a reachable walkable point in the circle was found; false otherwise.</returns>
    public bool TryFindRandomPointWithinCircle(Vector3 center, float radius, out Vector3 point)
    {
        point = center;
        if (!IsLoaded || radius <= 0f)
        {
            return false;
        }

        lock (_lock)
        {
            var rc = ToRc(center);
            var ext = new RcVec3f(radius, radius, radius);
            if (_query!.FindNearestPoly(rc, ext, _filter, out var startRef, out _, out _).Failed() || startRef == 0)
            {
                return false;
            }

            if (_query.FindRandomPointWithinCircle(startRef, rc, radius, _filter, _rand, out var randRef, out var randPt).Failed() || randRef == 0)
            {
                return false;
            }

            point = FromRc(randPt);
            return true;
        }
    }

    public bool Raycast(Vector3 from, Vector3 to, out Vector3 hitPoint, out float hitT)
    {
        hitPoint = to;
        hitT = 1f;
        if (!IsLoaded)
        {
            return false;
        }

        lock (_lock)
        {
            var rcFrom = ToRc(from);
            var rcTo = ToRc(to);
            if (_query!.FindNearestPoly(rcFrom, _searchExt, _filter, out var startRef, out _, out _).Failed() || startRef == 0)
            {
                return false;
            }

            var path = new long[64];
            if (_query.Raycast(startRef, rcFrom, rcTo, _filter, out var t, out var hitPos, path, out _, 64).Failed())
            {
                return false;
            }

            hitT = t;
            hitPoint = FromRc(hitPos);
            return t < 1f;
        }
    }

    public float? GetPolyHeight(long polyRef, Vector3 position)
    {
        if (!IsLoaded || !_navMesh!.IsValidPolyRef(polyRef))
        {
            return null;
        }

        lock (_lock)
        {
            var rc = ToRc(position);
            if (_navMesh.GetTileAndPolyByRef(polyRef, out var tile, out var poly).Succeeded())
            {
                if (_navMesh.GetPolyHeight(tile, poly, rc, out var y))
                {
                    return y;
                }
            }

            return null;
        }
    }
}
