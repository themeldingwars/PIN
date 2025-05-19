using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.Json.Nodes;

namespace GameServer.Physics.ZoneLoader;

public class ENWFData
{
    public struct VertBlockContent
    {
        public Vector3[] Verts;
    }

    public struct IndiceBlockContent
    {
        public uint[][] Indices;
    }

    public struct TagfileObjectContent
    {
        public string Name;
        public string Class;
        public JsonObject Data;
    }

    public class ENWFLayer
    {
        public ulong Id;
        public uint NumPhysicsMatIds;
        public uint[] PhysicsMatIds;
        public uint NumVertBlocks;
        public VertBlockContent[] VertBlocks;
        public uint NumIndiceBlocks;
        public IndiceBlockContent[] IndiceBlocks;
        public uint NumMatItems;
        public uint NumMoppBlocks;
        public List<BaseTagfileObject> TagfileObjects;

        public BaseTagfileObject GetTagfileObject(string query)
        {
            var result = TagfileObjects.Where((obj) => obj.Name == query).First();
            if (result != null)
            {
                return result;
            }

            Console.WriteLine($"Failed to find TagfileObject with query {query}");
            return null;
        }
    }

    public class BaseTagfileObject
    {
        public string Name;
        public string Class;
    }

    public class HkpListShapeObject : BaseTagfileObject
    {
        public uint UserData;
        public bool DisableWelding;
        public string CollectionType;
        public ChildInfoData[] ChildInfo;
        public uint Flags;
        public uint NumDisabledChildren;
        public Vector4 AabbHalfExtents;
        public Vector4 AabbCenter;
        public uint[] EnabledChildren;

        public struct ChildInfoData
        {
            public string Shape;
            public uint CollisionFilterInfo;
        }
    }

    public class HkpMoppBvTreeShapeObject : BaseTagfileObject
    {
        public uint UserData;
        public string BvTreeType;
        public string Code; 
        public string Child; 
    }

    public class HkpConvexTranslateShapeObject : BaseTagfileObject
    {
        public uint UserData;
        public float Radius;
        public string ChildShape;
        public Vector4 Translation;
    }

    public class HkpBoxShapeObject : BaseTagfileObject
    {
        public uint UserData;
        public float Radius;
        public Vector4 HalfExtents;
    }

    public class HkpSphereShapeObject : BaseTagfileObject
    {
        public uint UserData;
        public float Radius;
    }

    public class HkpCapsuleShapeObject : BaseTagfileObject
    {
        public uint UserData;
        public float Radius;
        public Vector4 VertexA;
        public Vector4 VertexB;
    }

    public class HkpCylinderShapeObject : BaseTagfileObject
    {
        public uint UserData;
        public float Radius;
        public float CylRadius;
        public float CylBaseRadiusFactorForHeightFieldCollisions;
        public Vector4 VertexA;
        public Vector4 VertexB;
        public Vector4 Perpendicular1;
        public Vector4 Perpendicular2;
    }

    public class HkpTransformShapeObject : BaseTagfileObject
    {
        public uint UserData;
        public string ChildShape;
        public Vector4 Rotation;
        public Vector4[] Transform;
    }

    public class HkpConvexTransformShapeObject : BaseTagfileObject
    {
        public uint UserData;
        public float Radius;
        public string ChildShape;
        public Vector4[] Transform;
    }

    public class HkpConvexVerticesShapeObject : BaseTagfileObject
    {
    }

    public class HkpExtendedMeshShapeObject : BaseTagfileObject
    {
        public uint UserData;
        public string DisableWelding;
        public string CollectionType;
        public TrianglesSubpart EmbeddedTrianglesSubpart;
        public Vector4 AabbHalfExtents;
        public Vector4 AabbCenter;
        public byte NumBitsForSubpartIndex;
        public TrianglesSubpart[] TrianglesSubparts;
        public ShapesSubpart[] ShapesSubparts;
        public uint[] WeldingInfo;
        public string WeldingType;
        public uint DefaultCollisionFilterInfo;
        public uint CachedNumChildShapes;
        public float TriangleRadius;

        public struct TrianglesSubpart
        {
            public string Type;
            public string MaterialIndexStridingType;
            public string MaterialIndexStriding;
            public uint NumMaterials;
            public uint UserData;
            public uint NumTriangleShapes;
            public uint NumVertices;
            public uint VertexStriding;
            public uint TriangleOffset;
            public uint IndexStriding;
            public string StridingType;
            public uint FlipAlternateTriangles;
            public Vector4 Extrusion;
            public Vector4[] Transform;
        }

        public struct ShapesSubpart
        {
            public string Type;
            public string MaterialIndexStridingType;
            public string MaterialIndexStriding;
            public uint NumMaterials;
            public uint UserData;
            public string[] ChildShapes;
            public Vector4 Rotation;
            public Vector4 Translation;
        }
    }
}