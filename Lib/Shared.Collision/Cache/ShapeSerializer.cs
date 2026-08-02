using System.Numerics;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities;
using BepuUtilities.Memory;

namespace Shared.Collision.Cache;

public static class ShapeSerializer
{
    public static void WriteShape(Simulation simulation, BufferPool pool, BinaryWriter writer, TypedIndex shapeIndex)
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

                writer.Write(hull.FaceVertexIndices.Length);
                for (int i = 0; i < hull.FaceVertexIndices.Length; i++)
                {
                    writer.Write(hull.FaceVertexIndices[i].BundleIndex);
                    writer.Write(hull.FaceVertexIndices[i].InnerIndex);
                }

                writer.Write(hull.FaceToVertexIndicesStart.Length);
                for (int i = 0; i < hull.FaceToVertexIndicesStart.Length; i++)
                {
                    writer.Write(hull.FaceToVertexIndicesStart[i]);
                }

                break;
            }

            default:
                throw new NotSupportedException($"ShapeSerializer: Unsupported shape type {shapeIndex.Type}");
        }
    }

    public static TypedIndex ReadShape(Simulation simulation, BufferPool pool, ThreadDispatcher dispatcher, BinaryReader reader, int shapeTypeId)
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
                throw new NotSupportedException($"ShapeSerializer: Unsupported shape type {shapeTypeId}");
        }
    }
}