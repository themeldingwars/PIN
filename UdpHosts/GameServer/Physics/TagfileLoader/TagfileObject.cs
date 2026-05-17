using System.Numerics;

namespace GameServer.Physics.TagfileLoader;

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
    public uint UserData;
    public float Radius;
    public uint NumVertices;
    public Vector4[][] RotatedVertices;
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

public class HkRootLevelContainerObject : BaseTagfileObject
{
    public NamedVariant[] NamedVariants;

    public struct NamedVariant
    {
        public string Variant;
    }
}

public class HkpRigidBody : BaseTagfileObject
{
    public CollidableData Collidable;
    public MotionStateData Motion;

    public struct CollidableData
    {
        public string Shape;
    }

    public struct MotionData
    {
        public MotionStateData MotionState;
    }

    public struct MotionStateData
    {
        public Vector4[] Transform;
    }
}

public class HkpStorageExtendedMeshShape : HkpExtendedMeshShapeObject
{
    public string[] Meshstorage;
    public string[] Shapestorage;
}

public class HkpStorageExtendedMeshShapeMeshSubpartStorage : BaseTagfileObject
{
    public Vector4[] Vertices;
    public uint[] Indices8;
    public uint[] Indices16;
    public uint[] Indices32;
    public uint[] MaterialIndices;
}

public class HkpSimpleMeshShape : BaseTagfileObject
{
    public uint UserData;
    public string DisableWelding;
    public string CollectionType;
    public string WeldingType;
    public float Radius;
    public uint[] MaterialIndices;
    public Vector4[] Vertices;
    public TriangleData[] Triangles;

    public struct TriangleData
    {
        public uint A;
        public uint B;
        public uint C;
        public uint WeldingInfo;
    }
}

public class HkaRagdollInstance : BaseTagfileObject
{
    public string[] RigidBodies;
    public string[] Constraints;
    public uint[] BoneToRigidBodyMap;
    public string Skeleton;
}
