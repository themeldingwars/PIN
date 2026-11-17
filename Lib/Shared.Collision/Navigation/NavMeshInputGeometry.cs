using DotRecast.Core.Numerics;
using DotRecast.Recast;
using DotRecast.Recast.Geom;

namespace Shared.Collision.Navigation;

/// <summary>
///     Recast's input geometry for a whole zone, one triangle per three vertices.
///     <see cref="RcSampleInputGeomProvider" /> builds the same thing, but its spatial index allocates an object per
///     triangle and sorts on one thread, which takes minutes with the hundred million triangles of a zone. This builds the
///     identical index with structs and sorts the halves of the tree in parallel.
/// </summary>
public sealed class NavMeshInputGeometry : IRcInputGeomProvider
{
    /// <summary>Triangles per leaf of the spatial index, what <see cref="RcTriMesh" /> uses.</summary>
    private const int _trisPerChunk = 32;

    /// <summary>Below this many triangles a subtree is built on the calling thread.</summary>
    private const int _parallelThreshold = 1 << 16;

    private readonly RcTriMesh _mesh;
    private readonly RcVec3f _bmin;
    private readonly RcVec3f _bmax;
    private readonly List<RcConvexVolume> _convexVolumes = [];
    private readonly List<RcOffMeshConnection> _offMeshConnections = [];

    /// <summary>Initializes a new instance of the <see cref="NavMeshInputGeometry"/> class.</summary>
    /// <param name="vertices">The triangle corners in Recast space, nine floats per triangle.</param>
    /// <param name="maxDegreeOfParallelism">How many threads build the spatial index.</param>
    public NavMeshInputGeometry(float[] vertices, int maxDegreeOfParallelism)
    {
        Vertices = vertices;
        TriangleCount = vertices.Length / 9;

        _bmin = new RcVec3f(vertices[0], vertices[1], vertices[2]);
        _bmax = _bmin;
        for (int i = 3; i < vertices.Length; i += 3)
        {
            var v = new RcVec3f(vertices[i], vertices[i + 1], vertices[i + 2]);
            _bmin = RcVec3f.Min(_bmin, v);
            _bmax = RcVec3f.Max(_bmax, v);
        }

        // RcTriMesh always indexes the faces it's given, so it gets a single triangle and the index of all of them
        // replaces that one. Rasterization only reads the vertices and the index.
        _mesh = new RcTriMesh(vertices, [0, 1, 2]);
        BuildChunkyTriMesh(vertices, TriangleCount, maxDegreeOfParallelism, _mesh.chunkyTriMesh);
    }

    /// <summary>Gets the triangle corners in Recast space, nine floats per triangle.</summary>
    public float[] Vertices { get; }

    /// <summary>Gets the number of triangles.</summary>
    public int TriangleCount { get; }

    public RcTriMesh GetMesh()
    {
        return _mesh;
    }

    public RcVec3f GetMeshBoundsMin()
    {
        return _bmin;
    }

    public RcVec3f GetMeshBoundsMax()
    {
        return _bmax;
    }

    public IEnumerable<RcTriMesh> Meshes()
    {
        return [_mesh];
    }

    public void AddConvexVolume(RcConvexVolume convexVolume)
    {
        _convexVolumes.Add(convexVolume);
    }

    public IList<RcConvexVolume> ConvexVolumes()
    {
        return _convexVolumes;
    }

    public List<RcOffMeshConnection> GetOffMeshConnections()
    {
        return _offMeshConnections;
    }

    public void AddOffMeshConnection(RcVec3f start, RcVec3f end, float radius, bool bidir, int area, int flags)
    {
        _offMeshConnections.Add(new RcOffMeshConnection(start, end, radius, bidir, area, flags));
    }

    public void RemoveOffMeshConnections(Predicate<RcOffMeshConnection> filter)
    {
        _offMeshConnections.RemoveAll(filter);
    }

    /// <summary>
    ///     Same tree as RcChunkyTriMeshs.CreateChunkyTriMesh: the triangles' XZ bounds are split at the median of the
    ///     longer axis until a node holds at most <see cref="_trisPerChunk" />, the nodes stored depth first. The sort is
    ///     the same introsort on the same keys, so ties end up in the same order. The size of a subtree only depends on
    ///     its triangle count, so both halves know where their nodes go and are built at the same time.
    /// </summary>
    private static void BuildChunkyTriMesh(float[] verts, int ntris, int maxDegreeOfParallelism, RcChunkyTriMesh cm)
    {
        var items = new BoundsItem[ntris];
        Parallel.For(0, ntris, new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism }, i =>
        {
            // The faces are 0, 1, 2, ..., so the vertex of corner j of triangle i is i * 3 + j
            int v = i * 9;
            ref var it = ref items[i];
            it.Index = i;
            it.MinX = it.MaxX = verts[v];
            it.MinY = it.MaxY = verts[v + 2];
            for (int j = 1; j < 3; ++j)
            {
                int c = v + (j * 3);
                if (verts[c] < it.MinX)
                {
                    it.MinX = verts[c];
                }

                if (verts[c + 2] < it.MinY)
                {
                    it.MinY = verts[c + 2];
                }

                if (verts[c] > it.MaxX)
                {
                    it.MaxX = verts[c];
                }

                if (verts[c + 2] > it.MaxY)
                {
                    it.MaxY = verts[c + 2];
                }
            }
        });

        var nodeCounts = new Dictionary<int, int>();
        var nodes = new RcChunkyTriMeshNode[NodeCount(ntris, nodeCounts)];
        new TreeBuilder(items, nodes, nodeCounts, maxDegreeOfParallelism).Subdivide(0, ntris, 0);

        int maxTrisPerChunk = 0;
        foreach (var node in nodes)
        {
            if (node.i >= 0)
            {
                maxTrisPerChunk = Math.Max(maxTrisPerChunk, node.tris.Length / 3);
            }
        }

        cm.nodes = [.. nodes];
        cm.ntris = ntris;
        cm.maxTrisPerChunk = maxTrisPerChunk;
    }

    private static int NodeCount(int count, Dictionary<int, int> memo)
    {
        if (count <= _trisPerChunk)
        {
            return 1;
        }

        if (!memo.TryGetValue(count, out var nodeCount))
        {
            nodeCount = 1 + NodeCount(count / 2, memo) + NodeCount(count - (count / 2), memo);
            memo[count] = nodeCount;
        }

        return nodeCount;
    }

    private struct BoundsItem
    {
        public float MinX;
        public float MinY;
        public float MaxX;
        public float MaxY;
        public int Index;
    }

    private sealed class XComparer : IComparer<BoundsItem>
    {
        public static readonly XComparer Shared = new();

        public int Compare(BoundsItem a, BoundsItem b)
        {
            return a.MinX.CompareTo(b.MinX);
        }
    }

    private sealed class YComparer : IComparer<BoundsItem>
    {
        public static readonly YComparer Shared = new();

        public int Compare(BoundsItem a, BoundsItem b)
        {
            return a.MinY.CompareTo(b.MinY);
        }
    }

    private sealed class TreeBuilder(BoundsItem[] items, RcChunkyTriMeshNode[] nodes, Dictionary<int, int> nodeCounts, int maxDegreeOfParallelism)
    {
        // Every level splits in two, so this many levels keep the threads busy
        private readonly int _parallelDepth = (int)Math.Floor(Math.Log2(Math.Max(1, maxDegreeOfParallelism)));

        public void Subdivide(int imin, int imax, int nodeIndex, int depth = 0)
        {
            int inum = imax - imin;
            var node = new RcChunkyTriMeshNode();
            nodes[nodeIndex] = node;
            CalcExtends(imin, imax, node);

            if (inum <= _trisPerChunk)
            {
                node.i = nodeIndex + 1;
                node.tris = new int[inum * 3];
                int dst = 0;
                for (int i = imin; i < imax; ++i)
                {
                    int src = items[i].Index * 3;
                    node.tris[dst++] = src;
                    node.tris[dst++] = src + 1;
                    node.tris[dst++] = src + 2;
                }

                return;
            }

            if (node.bmax.Y - node.bmin.Y > node.bmax.X - node.bmin.X)
            {
                Array.Sort(items, imin, inum, YComparer.Shared);
            }
            else
            {
                Array.Sort(items, imin, inum, XComparer.Shared);
            }

            int isplit = imin + (inum / 2);
            int rightIndex = nodeIndex + 1 + CountNodes(isplit - imin);
            if (depth < _parallelDepth && inum >= _parallelThreshold)
            {
                Parallel.Invoke(
                    () => Subdivide(imin, isplit, nodeIndex + 1, depth + 1),
                    () => Subdivide(isplit, imax, rightIndex, depth + 1));
            }
            else
            {
                Subdivide(imin, isplit, nodeIndex + 1, depth + 1);
                Subdivide(isplit, imax, rightIndex, depth + 1);
            }

            // Negative index means escape
            node.i = -(nodeIndex + CountNodes(inum));
        }

        private int CountNodes(int count)
        {
            if (count <= _trisPerChunk)
            {
                return 1;
            }

            // Filled up front, only read from here on
            return nodeCounts[count];
        }

        private void CalcExtends(int imin, int imax, RcChunkyTriMeshNode node)
        {
            ref var first = ref items[imin];
            float minX = first.MinX;
            float minY = first.MinY;
            float maxX = first.MaxX;
            float maxY = first.MaxY;
            for (int i = imin + 1; i < imax; ++i)
            {
                ref var it = ref items[i];
                if (it.MinX < minX)
                {
                    minX = it.MinX;
                }

                if (it.MinY < minY)
                {
                    minY = it.MinY;
                }

                if (it.MaxX > maxX)
                {
                    maxX = it.MaxX;
                }

                if (it.MaxY > maxY)
                {
                    maxY = it.MaxY;
                }
            }

            node.bmin = new RcVec2f(minX, minY);
            node.bmax = new RcVec2f(maxX, maxY);
        }
    }
}
