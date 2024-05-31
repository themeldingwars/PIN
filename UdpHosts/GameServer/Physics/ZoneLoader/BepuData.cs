using System.Numerics;
using BepuPhysics.Collidables;
using BepuUtilities.Memory;

namespace GameServer.Physics.ZoneLoader;

public class BepuData
{
    public static Mesh LoadMeshContent(MeshContent meshContent, BufferPool pool, Vector3 scaling)
    {
        pool.Take<Triangle>(meshContent.Triangles.Length, out var triangles);
        for (int i = 0; i < meshContent.Triangles.Length; ++i)
        {
            triangles[i] = new Triangle(meshContent.Triangles[i].A, meshContent.Triangles[i].B, meshContent.Triangles[i].C);
        }

        return new Mesh(triangles, scaling, pool);
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