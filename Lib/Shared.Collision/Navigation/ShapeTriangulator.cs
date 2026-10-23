using System.Numerics;
using BepuPhysics;
using BepuPhysics.Collidables;
using Matrix3x3 = BepuUtilities.Matrix3x3;

namespace Shared.Collision.Navigation;

/// <summary>
///     Converts Bepu collision shapes (looked up in a <see cref="Shapes"/> collection from the
///     static's <see cref="TypedIndex" />) into world-space triangles for navmesh baking.
///     Primitives (sphere/capsule/cylinder) are tessellated with a fixed radial segment count.
/// </summary>
public sealed class ShapeTriangulator
{
    private readonly Shapes _shapes;
    private readonly int _radialSegments;

    public ShapeTriangulator(Shapes shapes, int radialSegments = 16)
    {
        _shapes = shapes;
        _radialSegments = Math.Max(3, radialSegments);
    }

    /// <summary>Triangulates a single static (pose + shape) into world-space triangles.</summary>
    /// <param name="staticDescription">The static's pose and shape to triangulate.</param>
    /// <param name="output">Receives the generated world-space triangles.</param>
    public void Triangulate(in StaticDescription staticDescription, List<NavTriangle> output)
    {
        TriangulateShape(staticDescription.Shape, staticDescription.Pose, output);
    }

    private static Vector3 GetHullPoint(ConvexHull hull, HullVertexIndex index)
    {
        hull.GetPoint(index, out var point);
        return point;
    }

    private static Vector3 RadialDir(int i, int segments)
    {
        return new Vector3(MathF.Cos(Angle(i, segments)), 0, MathF.Sin(Angle(i, segments)));
    }

    private static float Angle(int i, int segments)
    {
        return (2f * MathF.PI * i) / segments;
    }

    private static Vector3 Transform(RigidPose pose, Matrix3x3 basis, Vector3 local)
    {
        Matrix3x3.Transform(local, basis, out var world);
        return pose.Position + world;
    }

    private static void Emit(List<NavTriangle> output, Vector3 a, Vector3 b, Vector3 c)
    {
        if (IsDegenerate(a, b, c))
        {
            return;
        }

        output.Add(new NavTriangle(a, b, c));
    }

    private static bool IsDegenerate(Vector3 a, Vector3 b, Vector3 c)
    {
        return Vector3.Cross(b - a, c - a).LengthSquared() < 1e-8f;
    }

    /// <summary>
    ///     Reverses the winding of every triangle appended since <paramref name="start" />.
    ///     The bake only marks a triangle walkable when its winding-derived normal points up in Recast
    ///     space, which for PIN's world Z-up → Recast Y-up axis swap means the world-space normal must
    ///     point DOWN (-Z). PIN's tessellators wind closed solids outward, so their winding is reversed
    ///     here; otherwise the bake puts navmesh on the underside of every primitive and leaves its top
    ///     unwalkable. See NavMeshTileBuilder.BuildVertexFaceArrays.
    /// </summary>
    private static void FlipWinding(List<NavTriangle> output, int start)
    {
        for (int i = start; i < output.Count; i++)
        {
            var tri = output[i];
            output[i] = new NavTriangle(tri.A, tri.C, tri.B);
        }
    }

    private void TriangulateShape(TypedIndex shape, RigidPose pose, List<NavTriangle> output)
    {
        if (!shape.Exists)
        {
            return;
        }

        switch (shape.Type)
        {
            // Mesh and Triangle carry Firefall's own winding, which already matches the convention
            // the bake requires, so they pass through untouched.
            case Mesh.Id:
                TriangulateMesh(_shapes.GetShape<Mesh>(shape.Index), pose, output);
                break;
            case ConvexHull.Id:
            {
                var start = output.Count;
                TriangulateConvexHull(_shapes.GetShape<ConvexHull>(shape.Index), pose, output);
                FlipWinding(output, start);
                break;
            }

            case Box.Id:
            {
                var start = output.Count;
                TriangulateBox(_shapes.GetShape<Box>(shape.Index), pose, output);
                FlipWinding(output, start);
                break;
            }

            case Triangle.Id:
                TriangulateTriangle(_shapes.GetShape<Triangle>(shape.Index), pose, output);
                break;
            case Sphere.Id:
            {
                var start = output.Count;
                TriangulateSphere(_shapes.GetShape<Sphere>(shape.Index), pose, output);
                FlipWinding(output, start);
                break;
            }

            case Cylinder.Id:
            {
                var start = output.Count;
                TriangulateCylinder(_shapes.GetShape<Cylinder>(shape.Index), pose, output);
                FlipWinding(output, start);
                break;
            }

            case Capsule.Id:
            {
                var start = output.Count;
                TriangulateCapsule(_shapes.GetShape<Capsule>(shape.Index), pose, output);
                FlipWinding(output, start);
                break;
            }

            case Compound.Id:
                TriangulateCompound(_shapes.GetShape<Compound>(shape.Index), pose, output);
                break;
            case BigCompound.Id:
                TriangulateBigCompound(_shapes.GetShape<BigCompound>(shape.Index), pose, output);
                break;
        }
    }

    private void TriangulateCompound(Compound compound, RigidPose parent, List<NavTriangle> output)
    {
        for (int i = 0; i < compound.Children.Length; i++)
        {
            ref var child = ref compound.Children[i];
            RigidPose.MultiplyWithoutOverlap(child.AsPose(), parent, out var composed);
            TriangulateShape(child.ShapeIndex, composed, output);
        }
    }

    private void TriangulateBigCompound(BigCompound compound, RigidPose parent, List<NavTriangle> output)
    {
        for (int i = 0; i < compound.Children.Length; i++)
        {
            ref var child = ref compound.Children[i];
            RigidPose.MultiplyWithoutOverlap(child.AsPose(), parent, out var composed);
            TriangulateShape(child.ShapeIndex, composed, output);
        }
    }

    private void TriangulateMesh(Mesh mesh, RigidPose pose, List<NavTriangle> output)
    {
        Matrix3x3.CreateFromQuaternion(pose.Orientation, out var basis);
        for (int i = 0; i < mesh.Triangles.Length; i++)
        {
            mesh.GetLocalChild(i, out var tri);
            Emit(output, Transform(pose, basis, tri.A), Transform(pose, basis, tri.B), Transform(pose, basis, tri.C));
        }
    }

    private void TriangulateConvexHull(ConvexHull hull, RigidPose pose, List<NavTriangle> output)
    {
        Matrix3x3.CreateFromQuaternion(pose.Orientation, out var basis);
        int faceCount = hull.FaceToVertexIndicesStart.Length;
        for (int face = 0; face < faceCount; face++)
        {
            hull.GetVertexIndicesForFace(face, out var faceVerts);
            int count = faceVerts.Length;
            if (count < 3)
            {
                continue;
            }

            Vector3 first = Transform(pose, basis, GetHullPoint(hull, faceVerts[0]));
            for (int i = 1; i + 1 < count; i++)
            {
                Emit(output, first, Transform(pose, basis, GetHullPoint(hull, faceVerts[i])), Transform(pose, basis, GetHullPoint(hull, faceVerts[i + 1])));
            }
        }
    }

    private void TriangulateBox(Box box, RigidPose pose, List<NavTriangle> output)
    {
        Matrix3x3.CreateFromQuaternion(pose.Orientation, out var basis);
        var hw = box.HalfWidth;
        var hh = box.HalfHeight;
        var hl = box.HalfLength;
        var c = new[]
        {
            new Vector3(hw, hh, hl), new Vector3(hw, hh, -hl), new Vector3(hw, -hh, hl), new Vector3(hw, -hh, -hl),
            new Vector3(-hw, hh, hl), new Vector3(-hw, hh, -hl), new Vector3(-hw, -hh, hl), new Vector3(-hw, -hh, -hl),
        };
        var f = new[]
        {
            0, 2, 3, 0, 3, 1,   // +X
            4, 5, 7, 4, 7, 6,   // -X
            0, 4, 6, 0, 6, 2,   // +Z
            1, 3, 7, 1, 7, 5,   // -Z
            0, 1, 5, 0, 5, 4,   // +Y (top)
            2, 6, 7, 2, 7, 3,   // -Y (bottom)
        };
        for (int i = 0; i < f.Length; i += 3)
        {
            Emit(output, Transform(pose, basis, c[f[i]]), Transform(pose, basis, c[f[i + 1]]), Transform(pose, basis, c[f[i + 2]]));
        }
    }

    private void TriangulateTriangle(Triangle tri, RigidPose pose, List<NavTriangle> output)
    {
        Matrix3x3.CreateFromQuaternion(pose.Orientation, out var basis);
        Emit(output, Transform(pose, basis, tri.A), Transform(pose, basis, tri.B), Transform(pose, basis, tri.C));
    }

    private void TriangulateSphere(Sphere sphere, RigidPose pose, List<NavTriangle> output)
    {
        int radial = _radialSegments;
        int rings = Math.Max(2, radial / 2);
        var ringY = new Vector3[rings + 1];
        var ringCos = new float[rings + 1];
        var ringSin = new float[rings + 1];
        for (int j = 0; j <= rings; j++)
        {
            var phi = (MathF.PI * j) / rings;
            ringY[j] = new Vector3(0, MathF.Cos(phi), 0) * sphere.Radius;
            ringCos[j] = MathF.Cos(phi) * sphere.Radius;
            ringSin[j] = MathF.Sin(phi) * sphere.Radius;
        }

        Matrix3x3.CreateFromQuaternion(pose.Orientation, out var basis);
        for (int j = 0; j < rings; j++)
        {
            for (int i = 0; i < radial; i++)
            {
                var a0 = Angle(i, radial);
                var a1 = Angle(i + 1, radial);
                var t0 = new Vector3(MathF.Cos(a0), 0, MathF.Sin(a0));
                var t1 = new Vector3(MathF.Cos(a1), 0, MathF.Sin(a1));
                Emit(output,
                    Transform(pose, basis, ringY[j] + (t0 * ringSin[j])),
                    Transform(pose, basis, ringY[j] + (t1 * ringSin[j])),
                    Transform(pose, basis, ringY[j + 1] + (t1 * ringSin[j + 1])));
                Emit(output,
                    Transform(pose, basis, ringY[j] + (t0 * ringSin[j])),
                    Transform(pose, basis, ringY[j + 1] + (t1 * ringSin[j + 1])),
                    Transform(pose, basis, ringY[j + 1] + (t0 * ringSin[j + 1])));
            }
        }
    }

    private void TriangulateCylinder(Cylinder cylinder, RigidPose pose, List<NavTriangle> output)
    {
        Matrix3x3.CreateFromQuaternion(pose.Orientation, out var basis);
        var r = cylinder.Radius;
        var hl = cylinder.HalfLength;
        var top = new Vector3(0, hl, 0);
        var bottom = new Vector3(0, -hl, 0);
        for (int i = 0; i < _radialSegments; i++)
        {
            var d0 = RadialDir(i, _radialSegments);
            var d1 = RadialDir(i + 1, _radialSegments);

            // side
            Emit(output, Transform(pose, basis, bottom + (d0 * r)), Transform(pose, basis, bottom + (d1 * r)), Transform(pose, basis, top + (d1 * r)));
            Emit(output, Transform(pose, basis, bottom + (d0 * r)), Transform(pose, basis, top + (d1 * r)), Transform(pose, basis, top + (d0 * r)));

            // top cap (normal +Y)
            Emit(output, Transform(pose, basis, Vector3.Zero), Transform(pose, basis, top + (d0 * r)), Transform(pose, basis, top + (d1 * r)));

            // bottom cap (normal -Y)
            Emit(output, Transform(pose, basis, Vector3.Zero), Transform(pose, basis, bottom + (d1 * r)), Transform(pose, basis, bottom + (d0 * r)));
        }
    }

    private void TriangulateCapsule(Capsule capsule, RigidPose pose, List<NavTriangle> output)
    {
        Matrix3x3.CreateFromQuaternion(pose.Orientation, out var basis);
        var r = capsule.Radius;
        var hl = capsule.HalfLength;
        var topPole = new Vector3(0, hl + r, 0);
        var bottomPole = new Vector3(0, -hl - r, 0);
        for (int i = 0; i < _radialSegments; i++)
        {
            var d0 = RadialDir(i, _radialSegments);
            var d1 = RadialDir(i + 1, _radialSegments);

            // cylindrical side (bottom ring -> top ring)
            Emit(output, Transform(pose, basis, (-hl * Vector3.UnitY) + (d0 * r)), Transform(pose, basis, (-hl * Vector3.UnitY) + (d1 * r)), Transform(pose, basis, (hl * Vector3.UnitY) + (d1 * r)));
            Emit(output, Transform(pose, basis, (-hl * Vector3.UnitY) + (d0 * r)), Transform(pose, basis, (hl * Vector3.UnitY) + (d1 * r)), Transform(pose, basis, (hl * Vector3.UnitY) + (d0 * r)));

            // top hemisphere approximated as a cone fan from the top pole
            Emit(output, Transform(pose, basis, topPole), Transform(pose, basis, (hl * Vector3.UnitY) + (d1 * r)), Transform(pose, basis, (hl * Vector3.UnitY) + (d0 * r)));

            // bottom hemisphere
            Emit(output, Transform(pose, basis, bottomPole), Transform(pose, basis, (-hl * Vector3.UnitY) + (d0 * r)), Transform(pose, basis, (-hl * Vector3.UnitY) + (d1 * r)));
        }
    }
}
