using System;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities;
using BepuUtilities.Memory;

namespace GameServer.Physics.ZoneLoader;

/// <summary>
/// Serializes and deserializes the static collision shapes of a BepuPhysics Simulation
/// to/from a binary file, enabling fast cached loading on subsequent startups.
/// </summary>
public static class SimulationCache
{
    private static readonly byte[] Magic = "PZSC"u8.ToArray();
    private const int FormatVersion = 1;

    /// <summary>
    /// Returns the cache file path for a given zone.
    /// </summary>
    public static string GetCachePath(string mapsPath, uint zoneId)
    {
        return Path.Combine(mapsPath, $"{zoneId}.bincache");
    }

    /// <summary>
    /// Saves all statics and their shapes from the simulation to a binary cache file.
    /// </summary>
    public static void Save(Simulation simulation, BufferPool pool, string path)
    {
        var stopwatch = Stopwatch.StartNew();
        var staticCount = simulation.Statics.Count;

        using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 65536);
        using var writer = new BinaryWriter(fs);

        // Header
        writer.Write(Magic);
        writer.Write(FormatVersion);
        writer.Write(staticCount);

        for (int i = 0; i < staticCount; i++)
        {
            ref var stat = ref simulation.Statics[i];

            // Shape type
            writer.Write(stat.Shape.Type);

            // Pose (Position + Orientation = 7 floats = 28 bytes)
            writer.Write(stat.Pose.Position.X);
            writer.Write(stat.Pose.Position.Y);
            writer.Write(stat.Pose.Position.Z);
            writer.Write(stat.Pose.Orientation.X);
            writer.Write(stat.Pose.Orientation.Y);
            writer.Write(stat.Pose.Orientation.Z);
            writer.Write(stat.Pose.Orientation.W);

            // Shape data
            WriteShape(simulation, pool, writer, stat.Shape);
        }

        stopwatch.Stop();
        Console.WriteLine($"SimulationCache: Saved {staticCount} statics to {path} in {stopwatch.ElapsedMilliseconds}ms");
    }

    /// <summary>
    /// Attempts to load statics and shapes from a binary cache file into the simulation.
    /// </summary>
    /// <returns>True if the cache was loaded successfully, false otherwise.</returns>
    public static bool TryLoad(Simulation simulation, BufferPool pool, ThreadDispatcher dispatcher, string path)
    {
        if (!File.Exists(path))
        {
            return false;
        }

        try
        {
            var stopwatch = Stopwatch.StartNew();

            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 65536);
            using var reader = new BinaryReader(fs);

            // Validate header
            var magic = reader.ReadBytes(4);
            if (magic.Length < 4 || magic[0] != Magic[0] || magic[1] != Magic[1] || magic[2] != Magic[2] || magic[3] != Magic[3])
            {
                Console.WriteLine("SimulationCache: Invalid magic header, skipping cache.");
                return false;
            }

            var version = reader.ReadInt32();
            if (version != FormatVersion)
            {
                Console.WriteLine($"SimulationCache: Version mismatch (expected {FormatVersion}, got {version}), skipping cache.");
                return false;
            }

            var staticCount = reader.ReadInt32();

            for (int i = 0; i < staticCount; i++)
            {
                var shapeTypeId = reader.ReadInt32();

                // Read pose
                var pose = new RigidPose
                {
                    Position = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()),
                    Orientation = new Quaternion(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle())
                };

                // Read shape and add to simulation
                var shapeIndex = ReadShape(simulation, pool, dispatcher, reader, shapeTypeId);
                simulation.Statics.Add(new StaticDescription(pose, shapeIndex));
            }

            stopwatch.Stop();
            Console.WriteLine($"SimulationCache: Loaded {staticCount} statics from cache in {stopwatch.ElapsedMilliseconds}ms");
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine($"SimulationCache: Failed to load cache: {e.Message} ({e.GetType().Name})");
            return false;
        }
    }

    private static void WriteShape(Simulation simulation, BufferPool pool, BinaryWriter writer, TypedIndex shapeIndex)
    {
        switch (shapeIndex.Type)
        {
            case Sphere.Id:
            {
                ref var shape = ref simulation.Shapes.GetShape<Sphere>(shapeIndex.Index);
                writer.Write(shape.Radius);
                break;
            }
            case Capsule.Id:
            {
                ref var shape = ref simulation.Shapes.GetShape<Capsule>(shapeIndex.Index);
                writer.Write(shape.Radius);
                writer.Write(shape.HalfLength);
                break;
            }
            case Box.Id:
            {
                ref var shape = ref simulation.Shapes.GetShape<Box>(shapeIndex.Index);
                writer.Write(shape.HalfWidth);
                writer.Write(shape.HalfHeight);
                writer.Write(shape.HalfLength);
                break;
            }
            case Cylinder.Id:
            {
                ref var shape = ref simulation.Shapes.GetShape<Cylinder>(shapeIndex.Index);
                writer.Write(shape.Radius);
                writer.Write(shape.HalfLength);
                break;
            }
            case Mesh.Id:
            {
                ref var mesh = ref simulation.Shapes.GetShape<Mesh>(shapeIndex.Index);
                var byteCount = mesh.GetSerializedByteCount();
                writer.Write(byteCount);
                var buffer = new byte[byteCount];
                mesh.Serialize(buffer);
                writer.Write(buffer);
                break;
            }
            case ConvexHull.Id:
            {
                ref var hull = ref simulation.Shapes.GetShape<ConvexHull>(shapeIndex.Index);

                // Points
                writer.Write(hull.Points.Length);

                for (int i = 0; i < hull.Points.Length; i++)
                {
                    ref var bundle = ref hull.Points[i];

                    for (int lane = 0; lane < Vector<float>.Count; lane++)
                    {
                        writer.Write(bundle.X[lane]);
                        writer.Write(bundle.Y[lane]);
                        writer.Write(bundle.Z[lane]);
                    }
                }

                // Bounding planes
                writer.Write(hull.BoundingPlanes.Length);

                for (int i = 0; i < hull.BoundingPlanes.Length; i++)
                {
                    ref var plane = ref hull.BoundingPlanes[i];

                    for (int lane = 0; lane < Vector<float>.Count; lane++)
                    {
                        writer.Write(plane.Normal.X[lane]);
                        writer.Write(plane.Normal.Y[lane]);
                        writer.Write(plane.Normal.Z[lane]);
                        writer.Write(plane.Offset[lane]);
                    }
                }

                // FaceVertexIndices
                writer.Write(hull.FaceVertexIndices.Length);

                for (int i = 0; i < hull.FaceVertexIndices.Length; i++)
                {
                    writer.Write(hull.FaceVertexIndices[i].BundleIndex);
                    writer.Write(hull.FaceVertexIndices[i].InnerIndex);
                }

                // FaceToVertexIndicesStart
                writer.Write(hull.FaceToVertexIndicesStart.Length);

                for (int i = 0; i < hull.FaceToVertexIndicesStart.Length; i++)
                {
                    writer.Write(hull.FaceToVertexIndicesStart[i]);
                }

                break;
            }
            default:
                throw new NotSupportedException($"SimulationCache: Unsupported shape type {shapeIndex.Type}");
        }
    }

    private static TypedIndex ReadShape(Simulation simulation, BufferPool pool, ThreadDispatcher dispatcher, BinaryReader reader, int shapeTypeId)
    {
        switch (shapeTypeId)
        {
            case Sphere.Id:
            {
                var radius = reader.ReadSingle();
                return simulation.Shapes.Add(new Sphere(radius));
            }
            case Capsule.Id:
            {
                var radius = reader.ReadSingle();
                var halfLength = reader.ReadSingle();
                var capsule = new Capsule { Radius = radius, HalfLength = halfLength };
                return simulation.Shapes.Add(capsule);
            }
            case Box.Id:
            {
                var halfWidth = reader.ReadSingle();
                var halfHeight = reader.ReadSingle();
                var halfLength = reader.ReadSingle();
                var box = new Box { HalfWidth = halfWidth, HalfHeight = halfHeight, HalfLength = halfLength };
                return simulation.Shapes.Add(box);
            }
            case Cylinder.Id:
            {
                var radius = reader.ReadSingle();
                var halfLength = reader.ReadSingle();
                var cylinder = new Cylinder { Radius = radius, HalfLength = halfLength };
                return simulation.Shapes.Add(cylinder);
            }
            case Mesh.Id:
            {
                var byteCount = reader.ReadInt32();
                var buffer = reader.ReadBytes(byteCount);
                var mesh = new Mesh(buffer, pool);
                return simulation.Shapes.Add(mesh);
            }
            case ConvexHull.Id:
            {
                // Points
                var pointBundleCount = reader.ReadInt32();

                pool.Take<Vector3Wide>(pointBundleCount, out var points);

                for (int i = 0; i < pointBundleCount; i++)
                {
                    float[] xs = new float[Vector<float>.Count];
                    float[] ys = new float[Vector<float>.Count];
                    float[] zs = new float[Vector<float>.Count];

                    for (int lane = 0; lane < Vector<float>.Count; lane++)
                    {
                        xs[lane] = reader.ReadSingle();
                        ys[lane] = reader.ReadSingle();
                        zs[lane] = reader.ReadSingle();
                    }

                    points[i] = new Vector3Wide
                    {
                        X = new Vector<float>(xs),
                        Y = new Vector<float>(ys),
                        Z = new Vector<float>(zs)
                    };
                }

                // Bounding planes
                var planeCount = reader.ReadInt32();

                pool.Take<HullBoundingPlanes>(planeCount, out var planes);

                for (int i = 0; i < planeCount; i++)
                {
                    float[] nx = new float[Vector<float>.Count];
                    float[] ny = new float[Vector<float>.Count];
                    float[] nz = new float[Vector<float>.Count];
                    float[] offsets = new float[Vector<float>.Count];

                    for (int lane = 0; lane < Vector<float>.Count; lane++)
                    {
                        nx[lane] = reader.ReadSingle();
                        ny[lane] = reader.ReadSingle();
                        nz[lane] = reader.ReadSingle();
                        offsets[lane] = reader.ReadSingle();
                    }

                    planes[i] = new HullBoundingPlanes
                    {
                        Normal = new Vector3Wide
                        {
                            X = new Vector<float>(nx),
                            Y = new Vector<float>(ny),
                            Z = new Vector<float>(nz)
                        },
                        Offset = new Vector<float>(offsets)
                    };
                }

                // FaceVertexIndices
                var faceVertexIndexCount = reader.ReadInt32();

                pool.Take<HullVertexIndex>(faceVertexIndexCount, out var faceVertexIndices);

                for (int i = 0; i < faceVertexIndexCount; i++)
                {
                    faceVertexIndices[i] = new HullVertexIndex
                    {
                        BundleIndex = reader.ReadUInt16(),
                        InnerIndex = reader.ReadUInt16()
                    };
                }

                // FaceToVertexIndicesStart
                var startCount = reader.ReadInt32();

                pool.Take<int>(startCount, out var faceStarts);

                for (int i = 0; i < startCount; i++)
                {
                    faceStarts[i] = reader.ReadInt32();
                }

                var hull = new ConvexHull
                {
                    Points = points,
                    BoundingPlanes = planes,
                    FaceVertexIndices = faceVertexIndices,
                    FaceToVertexIndicesStart = faceStarts
                };

                return simulation.Shapes.Add(hull);
            }
            default:
                throw new NotSupportedException($"SimulationCache: Unsupported shape type {shapeTypeId}");
        }
    }
}
