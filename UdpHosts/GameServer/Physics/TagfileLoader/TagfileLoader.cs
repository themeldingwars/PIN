using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities;
using BepuUtilities.Memory;
using Serilog;
using static GameServer.Physics.ZoneLoader.BepuData;

namespace GameServer.Physics.TagfileLoader;

public class TagfileLoader
{
    private static readonly ILogger _logger = Log.ForContext<TagfileLoader>();

    private readonly JsonSerializerOptions _serializerOptions = new()
    {
        IncludeFields = true,
        PropertyNameCaseInsensitive = true
    };

    public TagfileLoader(Simulation simulation, BufferPool pool, ThreadDispatcher dispatcher)
    {
        Simulation = simulation;
        BufferPool = pool;
        ThreadDispatcher = dispatcher;

        _serializerOptions.Converters.Add(new TagfileObjectJsonConverter());
        _serializerOptions.Converters.Add(new Vector4Converter());
        _serializerOptions.Converters.Add(new Vector3Converter());
        _serializerOptions.Converters.Add(new StringBooleanConverter());

        PlaceholderBox = Simulation.Shapes.Add(new Box(0.5f * 2, 0.5f * 2, 0.5f * 2));
    }

    public Simulation Simulation { get; protected set; }
    public BufferPool BufferPool { get; private set; }
    public ThreadDispatcher ThreadDispatcher { get; private set; }
    public TypedIndex PlaceholderBox { get; private set; }

    public StaticDescription[] LoadRigidBody(Vector3 origin, string path)
    {
        _logger.Debug("LoadRigidBody {path}", path);
        try
        {
            string json = File.ReadAllText(path);

            TagfileAsset asset = JsonSerializer.Deserialize<TagfileAsset>(json, _serializerOptions);

            var myLayer = asset as ITagfileExternalStorage;
            var root = asset.GetTagfileObject("#0001");
            var statics = ProcessObject(root, ref myLayer);

            return statics;
        }
        catch (FileNotFoundException)
        {
            _logger.Warning("LoadRigidBody File Not Found: {path}", path);
            return [];
        }
        catch (Exception e)
        {
            _logger.Error("LoadRigidBody Failed {exceptionMessage} ({exceptionType}) on {path}\n{more}", e.Message, e.GetType().Name, path, e.StackTrace);
            return [];
        }
    }

    public StaticDescription[] ProcessObject(BaseTagfileObject obj, ref ITagfileExternalStorage layer)
    {
        return ProcessTagObject(obj, ref layer);
    }

    private StaticDescription[] ProcessTagObject(BaseTagfileObject obj, ref ITagfileExternalStorage layer)
    {
        switch (obj)
        {
            // Shapes
            case HkpBoxShapeObject box:
                return ProcessShape(box, ref layer);
            case HkpSphereShapeObject sphere:
                return ProcessShape(sphere, ref layer);
            case HkpCapsuleShapeObject capsule:
                return ProcessShape(capsule, ref layer);
            case HkpCylinderShapeObject cylinder:
                return ProcessShape(cylinder, ref layer);
            case HkpExtendedMeshShapeObject extendedMesh:
                return ProcessShape(extendedMesh, ref layer);
            case HkpConvexVerticesShapeObject convexVertices:
                return ProcessShape(convexVertices, ref layer);

            // Containers
            case HkpListShapeObject list:
                return ProcessContainer(list, ref layer);
            case HkpMoppBvTreeShapeObject moppBvTree:
                return ProcessContainer(moppBvTree, ref layer);
            case HkRootLevelContainerObject rootContainer:
                return ProcessContainer(rootContainer, ref layer);
            case HkpRigidBody rigidBody:
                return ProcessContainer(rigidBody, ref layer);

            // Modifiers
            case HkpConvexTranslateShapeObject convexTranslate:
                return ProcessModifier(convexTranslate, ref layer);
            case HkpTransformShapeObject transform:
                return ProcessModifier(transform, ref layer);
            case HkpConvexTransformShapeObject convexTransform:
                return ProcessModifier(convexTransform, ref layer);

            default:
                break;
        }

        throw new NotImplementedException($"ProcessTagObject could not process an object {obj}");
    }

    private StaticDescription[] ProcessContainer(HkpListShapeObject obj, ref ITagfileExternalStorage layer)
    {
        List<StaticDescription> result = new();

        foreach (var childInfo in obj.ChildInfo)
        {
            var childObj = layer.GetTagfileObject(childInfo.Shape);
            try
            {
                var childStaticArr = ProcessTagObject(childObj, ref layer);
                result.AddRange(childStaticArr);
            }
            catch (NotImplementedException)
            {
                _logger.Warning("Ignoring child {childObject} of {parentObject} because support is not implemented", childObj, obj);
            }
        }

        return result.ToArray();
    }

    private StaticDescription[] ProcessContainer(HkpMoppBvTreeShapeObject obj, ref ITagfileExternalStorage layer)
    {
        var childObj = layer.GetTagfileObject(obj.Child);
        return ProcessTagObject(childObj, ref layer);
    }

    private StaticDescription[] ProcessContainer(HkRootLevelContainerObject obj, ref ITagfileExternalStorage layer)
    {
        List<StaticDescription> result = new();

        foreach (var namedVariant in obj.NamedVariants)
        {
            var childObj = layer.GetTagfileObject(namedVariant.Variant);

            if (childObj.Class != "hkpRigidBody")
            {
                _logger.Warning("Ignoring named variant {name} {class}", childObj.Name, childObj.Class);
                continue;
            }

            try
            {
                var childStaticArr = ProcessTagObject(childObj, ref layer);
                result.AddRange(childStaticArr);
            }
            catch (NotImplementedException)
            {
                _logger.Warning("Ignoring child {childObject} of {parentObject} because support is not implemented", childObj, obj);
            }
        }

        return result.ToArray();
    }

    private StaticDescription[] ProcessContainer(HkpRigidBody obj, ref ITagfileExternalStorage layer)
    {
        var childShapeObj = layer.GetTagfileObject(obj.Collidable.Shape);
        var childShapeStaticArr = ProcessTagObject(childShapeObj, ref layer);
        var transformObj = obj.Motion.Transform;

        var pos = new Vector3(transformObj[3][0], transformObj[3][1], transformObj[3][2]);

        _logger.Debug("HkpRigidBody pos to {x}, {y}, {z}", pos.X, pos.Y, pos.Z);
        var matrix = new Matrix4x4(
            transformObj[0][0],
            transformObj[0][1],
            transformObj[0][2],
            0,
            transformObj[1][0],
            transformObj[1][1],
            transformObj[1][2],
            0,
            transformObj[2][0],
            transformObj[2][1],
            transformObj[2][2],
            0,
            0,
            0,
            0,
            0);
        var rot = Quaternion.Normalize(Quaternion.CreateFromRotationMatrix(matrix));
        var parentPose = new RigidPose(pos, rot);

        return childShapeStaticArr.Select((StaticDescription childShapeStatic) =>
        {
            // Apply the transform of hkpRigidBody directly to the children without losing their local poses
            RigidPose.MultiplyWithoutOverlap(childShapeStatic.Pose, parentPose, out var transformedPose);
            childShapeStatic.Pose = transformedPose;
            return childShapeStatic;
        }).ToArray();
    }

    private StaticDescription[] ProcessModifier(HkpConvexTranslateShapeObject obj, ref ITagfileExternalStorage layer)
    {
        var childShapeObj = layer.GetTagfileObject(obj.ChildShape);
        var childShapeStaticArr = ProcessTagObject(childShapeObj, ref layer);

        var pos = new Vector3(obj.Translation[0], obj.Translation[1], obj.Translation[2]);

        return childShapeStaticArr.Select((StaticDescription childShapeStatic) =>
        {
            childShapeStatic.Pose.Position = pos;
            return childShapeStatic;
        }).ToArray();
    }

    private StaticDescription[] ProcessModifier(HkpTransformShapeObject obj, ref ITagfileExternalStorage layer)
    {
        var childShapeObj = layer.GetTagfileObject(obj.ChildShape);
        var childShapeStaticArr = ProcessTagObject(childShapeObj, ref layer);

        var rot = new Quaternion(obj.Rotation[0], obj.Rotation[1], obj.Rotation[2], obj.Rotation[3]);
        var pos = new Vector3(obj.Transform[3][0], obj.Transform[3][1], obj.Transform[3][2]);

        return childShapeStaticArr.Select((StaticDescription childShapeStatic) =>
        {
            if (childShapeStatic.Shape.Type == Mesh.Id)
            {
                /*
                Spent a day testing before landing on this fix, hopefully it lasts...
                The issue occurs when hkpExtendedMeshShape triangle subpart has translation, and then somewhere in the lineage there is a parent hkpTransformShape to position the whole shape in the world.
                Calling Recenter was the only thing that seemed to help, I suspect there may be some relation to these meshes having scaling as well.
                However, we still have other hkpExtendedMeshShapes with triangle subparts and translation that are not supposed to be repositioned, so I landed on calling it before we reposition it with the transform.
                */
                ref var mesh = ref Simulation.Shapes.GetShape<Mesh>(childShapeStatic.Shape.Index);
                mesh.Recenter(-childShapeStatic.Pose.Position);
            }

            childShapeStatic.Pose.Orientation = rot;
            childShapeStatic.Pose.Position = pos;
            return childShapeStatic;
        }).ToArray();
    }

    private StaticDescription[] ProcessModifier(HkpConvexTransformShapeObject obj, ref ITagfileExternalStorage layer)
    {
        var childShapeObj = layer.GetTagfileObject(obj.ChildShape);
        var childShapeStaticArr = ProcessTagObject(childShapeObj, ref layer);

        var pos = new Vector3(obj.Transform[3][0], obj.Transform[3][1], obj.Transform[3][2]);
        var matrix = new Matrix4x4(
            obj.Transform[0][0],
            obj.Transform[0][1],
            obj.Transform[0][2],
            0,
            obj.Transform[1][0],
            obj.Transform[1][1],
            obj.Transform[1][2],
            0,
            obj.Transform[2][0],
            obj.Transform[2][1],
            obj.Transform[2][2],
            0,
            0,
            0,
            0,
            0);
        var rot = Quaternion.Normalize(Quaternion.CreateFromRotationMatrix(matrix));

        return childShapeStaticArr.Select((StaticDescription childShapeStatic) =>
        {
            childShapeStatic.Pose.Orientation = rot;
            childShapeStatic.Pose.Position = pos;
            return childShapeStatic;
        }).ToArray();
    }

    private StaticDescription[] ProcessShape(HkpBoxShapeObject obj, ref ITagfileExternalStorage layer)
    {
        var box = new Box(obj.HalfExtents[0] * 2, obj.HalfExtents[1] * 2, obj.HalfExtents[2] * 2);
        var stat = new StaticDescription(RigidPose.Identity, Simulation.Shapes.Add(box));
        return [stat];
    }

    private StaticDescription[] ProcessShape(HkpSphereShapeObject obj, ref ITagfileExternalStorage layer)
    {
        var sphere = new Sphere(obj.Radius);
        var stat = new StaticDescription(RigidPose.Identity, Simulation.Shapes.Add(sphere));
        return [stat];
    }

    private StaticDescription[] ProcessShape(HkpCapsuleShapeObject obj, ref ITagfileExternalStorage layer)
    {
        var rad = obj.Radius;
        var top = new Vector3(obj.VertexA[0], obj.VertexA[1], obj.VertexA[2]);
        var bot = new Vector3(obj.VertexB[0], obj.VertexB[1], obj.VertexB[2]);
        var mid = Vector3.Multiply(Vector3.Add(top, bot), 0.5f);
        var len = Vector3.Distance(top, bot);
        var dir = Vector3.Normalize(Vector3.Subtract(bot, top));
        var up = new Vector3(0, 1, 0); // Since the Capsule is Y-aligned in BepuPhysics
        QuaternionEx.GetQuaternionBetweenNormalizedVectors(up, dir, out Quaternion rot); // Rotate from Y-aligned to the Z-aligned based direction
        var capsule = new Capsule(rad, len);

        var pose = new RigidPose(mid, rot);
        var stat = new StaticDescription(pose, Simulation.Shapes.Add(capsule));
        return [stat];
    }

    private StaticDescription[] ProcessShape(HkpCylinderShapeObject obj, ref ITagfileExternalStorage layer)
    {
        var top = new Vector3(obj.VertexA[0], obj.VertexA[1], obj.VertexA[2]);
        var bot = new Vector3(obj.VertexB[0], obj.VertexB[1], obj.VertexB[2]);
        var mid = Vector3.Multiply(Vector3.Add(top, bot), 0.5f);
        var len = Vector3.Distance(top, bot);
        var dir = Vector3.Normalize(Vector3.Subtract(bot, top));
        var up = new Vector3(0, 1, 0); // Since the Capsule is Y-aligned in BepuPhysics
        QuaternionEx.GetQuaternionBetweenNormalizedVectors(up, dir, out Quaternion rot); // Rotate from Y-aligned to the Z-aligned based direction
        var rad = obj.CylRadius;
        var cylinder = new Cylinder(rad, len);

        var pose = new RigidPose(mid, rot);
        var stat = new StaticDescription(pose, Simulation.Shapes.Add(cylinder));
        return [stat];
    }

    private StaticDescription[] ProcessShape(HkpExtendedMeshShapeObject obj, ref ITagfileExternalStorage layer)
    {
        List<StaticDescription> result = new();

        foreach (var tripart in obj.TrianglesSubparts)
        {
            ushort blockIndicator = (ushort)(tripart.UserData & 0xFFFF);
            ref var vertices = ref layer.VertBlocks[blockIndicator].Verts;
            ref var indices = ref layer.IndiceBlocks[blockIndicator].Indices;
            var triangles = new TriangleContent[indices.Length];
            for (uint indiceIdx = 0; indiceIdx < indices.Length; indiceIdx++)
            {
                ref var indice = ref indices[indiceIdx];
                ref var triangle = ref triangles[indiceIdx];
                triangle.A = vertices[indice[2]];
                triangle.B = vertices[indice[1]];
                triangle.C = vertices[indice[0]];
            }

            var meshContent = new MeshContent(triangles);

            var transform = tripart.Transform;
            var rot = Quaternion.Normalize(new Quaternion(transform[1][0], transform[1][1], transform[1][2], transform[1][3]));
            var scale = new Vector3(transform[2][0], transform[2][1], transform[2][2]);
            var pos = new Vector3(transform[0][0], transform[0][1], transform[0][2]);

            var mesh = LoadMeshContent(meshContent, BufferPool, scale, ThreadDispatcher);

            var pose = new RigidPose(pos, rot);
            result.Add(new StaticDescription(pose, Simulation.Shapes.Add(mesh)));
        }

        foreach (var shapepart in obj.ShapesSubparts)
        {
            foreach (var childShape in shapepart.ChildShapes)
            {
                // TODO: Consider rotation and translation of subpart shape?
                var childShapeObj = layer.GetTagfileObject(childShape);
                try
                {
                    var childShapeStaticArr = ProcessTagObject(childShapeObj, ref layer);
                    result.AddRange(childShapeStaticArr);
                }
                catch (NotImplementedException)
                {
                    _logger.Warning("Ignoring child {childShapeObj} of {shapepart} because support is not implemented", childShapeObj, shapepart);
                }
            }
        }

        return result.ToArray();
    }

    private StaticDescription[] ProcessShape(HkpConvexVerticesShapeObject obj, ref ITagfileExternalStorage layer)
    {
        try
        {
            /*
            Some of the data is not compatible with bepu (or not correctly parsed/prepared by us).
            CreateShape has an assert "Convex hull procedure should not output degenerate faces.", but since its an assert we can't easily deal with it.
            We work with the helper directly so that we can use our own validation function.
            */
            Vector3[] vertices = UnrotateRotatedVertices(obj.RotatedVertices, obj.NumVertices);
            ConvexHullHelper.ComputeHull(vertices, BufferPool, out HullData hullData);
            if (IsHullFaceValid(vertices, hullData))
            {
                ConvexHullHelper.CreateShape(vertices, hullData, BufferPool, out Vector3 center, out ConvexHull convexHull);
                var pose = new RigidPose(center);
                var stat = new StaticDescription(pose, Simulation.Shapes.Add(convexHull));
                return [stat];
            }
            else
            {
                _logger.Error("Failed to process hkpConvexVerticesShape {pointer}. IsHullFaceValid reports false.", obj.Name);
                return [new StaticDescription(RigidPose.Identity, PlaceholderBox)];
            }
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to process hkpConvexVerticesShape {pointer}. Exception: {exceptionMessage} ({exceptionType}) \n{more}", obj.Name, ex.Message, ex.GetType().Name, ex.StackTrace);
            return [new StaticDescription(RigidPose.Identity, PlaceholderBox)];
        }
    }

    private Vector3[] UnrotateRotatedVertices(Vector4[][] rotatedVertices, uint numVertices)
    {
        Vector3[] vertices = new Vector3[numVertices];
        var vert = 0;
        for (int i = 0; i < rotatedVertices.Length; i++)
        {
            vertices[vert++] = new Vector3(rotatedVertices[i][0].X, rotatedVertices[i][1].X, rotatedVertices[i][2].X);
            if (vert == numVertices)
            {
                break;
            }

            vertices[vert++] = new Vector3(rotatedVertices[i][0].Y, rotatedVertices[i][1].Y, rotatedVertices[i][2].Y);
            if (vert == numVertices)
            {
                break;
            }

            vertices[vert++] = new Vector3(rotatedVertices[i][0].Z, rotatedVertices[i][1].Z, rotatedVertices[i][2].Z);
            if (vert == numVertices)
            {
                break;
            }

            vertices[vert++] = new Vector3(rotatedVertices[i][0].W, rotatedVertices[i][1].W, rotatedVertices[i][2].W);
            if (vert == numVertices)
            {
                break;
            }
        }

        return vertices;
    }

    private bool IsHullFaceValid(
    Span<Vector3> points,
    HullData hullData)
    {
        for (int faceIndex = 0; faceIndex < hullData.FaceStartIndices.Length; ++faceIndex)
        {
            hullData.GetFace(faceIndex, out var face);

            Vector3 faceNormal = default;

            hullData.GetFace(faceIndex, out face);

            var a = points[face[0]];
            var b = points[face[1]];
            var prev = b - a;

            for (int i = 2; i < face.VertexCount; ++i)
            {
                var c = points[face[i]];
                var curr = c - a;

                faceNormal += Vector3.Cross(prev, curr);
                prev = curr;
            }

            if (faceNormal.LengthSquared() <= 1e-20f)
            {
                return false;
            }
        }

        return true;
    }

    [StructLayout(LayoutKind.Sequential)]
    public class TagfileAsset : ITagfileExternalStorage
    {
        public VertBlockContent[] VertBlocks { get; set; }
        public IndiceBlockContent[] IndiceBlocks { get; set; }

        [JsonConverter(typeof(TagfileObjectDictionaryConverter))]
        public Dictionary<string, BaseTagfileObject> TagfileObjects { get; set; }

        public BaseTagfileObject GetTagfileObject(string query)
        {
            TagfileObjects.TryGetValue(query, out BaseTagfileObject result);

            if (result != null)
            {
                return result;
            }

            _logger.Error("Failed to find TagfileObject with query {query}", query);
            return null;
        }
    }
}