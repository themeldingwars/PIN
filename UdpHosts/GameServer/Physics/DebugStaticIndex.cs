#nullable enable
using System;
using System.Collections.Generic;
using System.Numerics;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities.Memory;

namespace GameServer.Physics;

/// <summary>
///     A lightweight, read-mostly spatial index over a zone's static collidables, used by the
///     physics debug renderer to stream only the statics near the camera. It stores each static's
///     world position, its shape reference, and a local bounding radius, but NONE of the mesh vertex
///     data (which is what is too large to cache whole-zone). A uniform 3-D grid maps world cells to
///     lists of static indices so a radius query around the camera is fast.
///
///     Built once on the shard thread at zone load and read on the shard thread during
///     (re)extraction; no locking is required because both the build and the reads happen on the
///     shard thread (the only thread that owns the simulation the shape references point into).
/// </summary>
public sealed class DebugStaticIndex
{
    private const int _cellSize = 32;
    private const float _cellInv = 1f / _cellSize;

    private readonly StaticEntry[] _entries;
    private readonly Dictionary<(int, int, int), List<int>> _grid;

    private DebugStaticIndex(StaticEntry[] entries, Dictionary<(int, int, int), List<int>> grid, int count)
    {
        _entries = entries;
        _grid = grid;
        Count = count;
    }

    public int Count { get; }

    public static DebugStaticIndex Build(Simulation simulation, BufferPool pool)
    {
        var statics = simulation.Statics;
        var count = statics.Count;
        var shapes = simulation.Shapes;

        var entries = new StaticEntry[Math.Max(1, count)];
        var grid = new Dictionary<(int, int, int), List<int>>();
        var written = 0;

        for (var i = 0; i < count; ++i)
        {
            ref var s = ref statics[i];
            var shape = s.Shape;
            if (!shape.Exists)
            {
                continue;
            }

            var position = s.Pose.Position;
            var radius = ComputeShapeRadius(shapes, shape);
            entries[written] = new StaticEntry
            {
                Position = position,
                Orientation = s.Pose.Orientation,
                Shape = shape,
                BoundingRadius = radius,
            };

            GetOrCreateCell(grid, position).Add(written);
            ++written;
        }

        // Trim to the actual count so the read path never sees uninitialized slots.
        if (written < entries.Length)
        {
            Array.Resize(ref entries, Math.Max(1, written));
        }

        return new DebugStaticIndex(entries, grid, written);
    }

    public void Query(Vector3 center, float radius, Action<Vector3, Quaternion, TypedIndex, float> callback)
    {
        var minCell = FloorCell(center - new Vector3(radius, radius, radius));
        var maxCell = FloorCell(center + new Vector3(radius, radius, radius));

        for (var x = minCell.x; x <= maxCell.x; ++x)
        {
            for (var y = minCell.y; y <= maxCell.y; ++y)
            {
                for (var z = minCell.z; z <= maxCell.z; ++z)
                {
                    if (!_grid.TryGetValue((x, y, z), out var list))
                    {
                        continue;
                    }

                    for (var k = 0; k < list.Count; ++k)
                    {
                        var entry = _entries[list[k]];
                        var delta = entry.Position - center;
                        var reach = radius + entry.BoundingRadius;
                        if (delta.LengthSquared() <= (reach * reach))
                        {
                            callback(entry.Position, entry.Orientation, entry.Shape, entry.BoundingRadius);
                        }
                    }
                }
            }
        }
    }

    private static List<int> GetOrCreateCell(Dictionary<(int, int, int), List<int>> grid, Vector3 position)
    {
        var key = (
            (int)MathF.Floor(position.X * _cellInv),
            (int)MathF.Floor(position.Y * _cellInv),
            (int)MathF.Floor(position.Z * _cellInv));
        if (!grid.TryGetValue(key, out var list))
        {
            list = new List<int>();
            grid[key] = list;
        }

        return list;
    }

    private static (int x, int y, int z) FloorCell(Vector3 p)
    {
        return ((int)MathF.Floor(p.X * _cellInv), (int)MathF.Floor(p.Y * _cellInv), (int)MathF.Floor(p.Z * _cellInv));
    }

    private static float ComputeShapeRadius(Shapes shapes, TypedIndex shape)
    {
        // A conservative local bounding radius (distance from the shape's local origin to its
        // farthest extent), good enough for sphere-based streaming culling. Exactness is not
        // required - a slightly over- or under-estimate only shifts the streaming radius a little.
        switch (shape.Type)
        {
            case Sphere.Id:
            {
                ref var s = ref shapes.GetShape<Sphere>(shape.Index);
                return s.Radius;
            }

            case Capsule.Id:
            {
                ref var c = ref shapes.GetShape<Capsule>(shape.Index);
                return c.HalfLength + c.Radius;
            }

            case Cylinder.Id:
            {
                ref var c = ref shapes.GetShape<Cylinder>(shape.Index);
                return MathF.Max(c.Radius, c.HalfLength) * 1.4142135f;
            }

            case Box.Id:
            {
                ref var b = ref shapes.GetShape<Box>(shape.Index);
                return MathF.Sqrt(
                    (b.HalfWidth * b.HalfWidth) + (b.HalfHeight * b.HalfHeight) + (b.HalfLength * b.HalfLength));
            }

            case Mesh.Id:
            {
                ref var m = ref shapes.GetShape<Mesh>(shape.Index);
                return EstimateMeshRadius(m);
            }

            case ConvexHull.Id:
            {
                ref var h = ref shapes.GetShape<ConvexHull>(shape.Index);
                return EstimateConvexHullRadius(h);
            }

            default:
                // Unknown / compound: use a generous default so nothing is silently dropped.
                return 64f;
        }
    }

    private static float EstimateMeshRadius(Mesh mesh)
    {
        // Sample the mesh's triangle vertices (the same approach the renderer's frustum culling uses
        // for its per-instance radius) to get a local bounding radius.
        var maxSq = 0f;
        var triangles = mesh.Triangles;
        for (var i = 0; i < triangles.Length; ++i)
        {
            ref var t = ref triangles[i];
            maxSq = MathF.Max(maxSq, t.A.LengthSquared());
            maxSq = MathF.Max(maxSq, t.B.LengthSquared());
            maxSq = MathF.Max(maxSq, t.C.LengthSquared());
        }

        return MathF.Sqrt(maxSq);
    }

    private static float EstimateConvexHullRadius(ConvexHull hull)
    {
        var maxSq = 0f;
        for (var i = 0; i < hull.FaceToVertexIndicesStart.Length; ++i)
        {
            hull.GetVertexIndicesForFace(i, out var faceVertexIndices);
            for (var j = 0; j < faceVertexIndices.Length; ++j)
            {
                hull.GetPoint(faceVertexIndices[j], out var p);
                maxSq = MathF.Max(maxSq, p.LengthSquared());
            }
        }

        return MathF.Sqrt(maxSq);
    }

    private struct StaticEntry
    {
        public Vector3 Position;
        public Quaternion Orientation;
        public TypedIndex Shape;
        public float BoundingRadius;
    }
}
