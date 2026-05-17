using System.Collections.Generic;
using System.Numerics;

namespace GameServer.Physics.TagfileLoader;

public interface ITagfileExternalStorage
{
    VertBlockContent[] VertBlocks { get; }
    IndiceBlockContent[] IndiceBlocks { get; }
    Dictionary<string, BaseTagfileObject> TagfileObjects { get; }
    BaseTagfileObject GetTagfileObject(string query);
}

public struct VertBlockContent
{
    public Vector3[] Verts;
}

public struct IndiceBlockContent
{
    public uint[][] Indices;
}