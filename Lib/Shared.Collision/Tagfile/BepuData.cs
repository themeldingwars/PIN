using System.Numerics;
using BepuPhysics.Collidables;
using BepuUtilities;
using BepuUtilities.Memory;

namespace Shared.Collision.Tagfile;

public class BepuData
{
    public const float DegenerateCrossLengthSquared = 1e-8f;

    public static bool IsDegenerate(Vector3 a, Vector3 b, Vector3 c)
    {
        return !(Vector3.Cross(b - a, c - a).LengthSquared() >= DegenerateCrossLengthSquared);
    }

    public static bool TryLoadMeshContent(MeshContent meshContent, BufferPool pool, Vector3 scaling, ThreadDispatcher dispatcher, out Mesh mesh, out int skipped)
    {
        var source = meshContent.Triangles;
        var kept = 0;
        for (int i = 0; i < source.Length; ++i)
        {
            if (!IsDegenerate(source[i].A * scaling, source[i].B * scaling, source[i].C * scaling))
            {
                kept++;
            }
        }

        skipped = source.Length - kept;
        if (kept == 0)
        {
            mesh = default;
            return false;
        }

        pool.Take<Triangle>(kept, out var triangles);
        var index = 0;
        for (int i = 0; i < source.Length; ++i)
        {
            if (!IsDegenerate(source[i].A * scaling, source[i].B * scaling, source[i].C * scaling))
            {
                triangles[index++] = new Triangle(source[i].A, source[i].B, source[i].C);
            }
        }

        mesh = new Mesh(triangles, scaling, pool, dispatcher);
        return true;
    }

    public struct TriangleContent
    {
        public Vector3 A;
        public Vector3 B;
        public Vector3 C;
    }

    public class MeshContent
    {
        public TriangleContent[] Triangles;

        public MeshContent(TriangleContent[] triangles)
        {
            Triangles = triangles;
        }
    }
}