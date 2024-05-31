using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using System.Threading.Tasks;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities;
using BepuUtilities.Memory;
using static GameServer.Physics.ZoneLoader.BepuData;
using static GameServer.Physics.ZoneLoader.ENWFData;

namespace GameServer.Physics.ZoneLoader;

public class ZoneLoader
{
    private readonly JsonSerializerOptions SerializerOptions = new()
    {
        IncludeFields = true,
        PropertyNameCaseInsensitive = true
    };

    public ZoneLoader(Simulation simulation, BufferPool pool)
    {
        Simulation = simulation;
        BufferPool = pool;

        SerializerOptions.Converters.Add(new TagfileObjectJsonConverter());
        SerializerOptions.Converters.Add(new Vector4Converter());
        SerializerOptions.Converters.Add(new Vector3Converter());
        SerializerOptions.Converters.Add(new StringBooleanConverter());
    }

    public Simulation Simulation { get; protected set; }
    public BufferPool BufferPool { get; private set; }

    public void LoadCollision(uint zoneId)
    {
        var chunks = GetZoneChunkMap();
        foreach (var chunk in chunks)
        {
            LoadChunkJSON(chunk.Origin, chunk.Path);
        }

        Console.WriteLine($"ZoneLoader Finished Loading Collision");
    }

    private void LoadChunkJSON(Vector3 origin, string path)
    {
        Console.WriteLine($"ZoneLoader Loading {path}");
        string jsonString = File.ReadAllText(path);

        var content = JsonSerializer.Deserialize<ChunkCGContent>(jsonString, SerializerOptions);

        foreach (var subChunk in content.SubChunks) 
        {
            foreach (var layer in new List<LayerContent>()
            { 
                subChunk.Cg,

                // subChunk.Cg2,
                // subChunk.Cg3
            })
            {
                if (layer == null)
                {
                    continue;
                }

                var myLayer = layer;
                var root = layer.GetTagfileObject("#0001");
                var statics = ProcessChunkObject(root, ref myLayer);

                statics = statics.Select((StaticDescription stat) =>
                {
                    stat.Pose.Position += origin;
                    return stat;
                }).ToArray();

                foreach(var stat in statics)
                {
                    Simulation.Statics.Add(stat);
                }
            }
        }
    }

    private List<ChunkRef> GetZoneChunkMap()
    {
        var list = new List<ChunkRef>
        {
            new ChunkRef { Origin = new Vector3(-256, -256, 0), Path = @"./chunkcg/1_0237_0893.gtchunk.cg.json" },
            new ChunkRef { Origin = new Vector3(256, -256, 0), Path = @"./chunkcg/1_0238_0893.gtchunk.cg.json" },
            new ChunkRef { Origin = new Vector3(-256, 256, 0), Path = @"./chunkcg/1_0237_0894.gtchunk.cg.json" },
            new ChunkRef { Origin = new Vector3(256, 256, 0), Path = @"./chunkcg/1_0238_0894.gtchunk.cg.json" }
        };
        return list;
    }

    private StaticDescription[] ProcessChunkObject(BaseTagfileObject obj, ref LayerContent layer)
    {
        // Containers
        switch (obj)
        {
            case HkpListShapeObject list:
                return ProcessContainer(list, ref layer);
            case HkpMoppBvTreeShapeObject moppBvTree:
                return ProcessContainer(moppBvTree, ref layer);
        }

        // Modifiers
        switch (obj)
        {
            case HkpConvexTranslateShapeObject convexTranslate:
                return ProcessModifier(convexTranslate, ref layer);
            case HkpTransformShapeObject transform:
                return ProcessModifier(transform, ref layer);
            case HkpConvexTransformShapeObject convexTransform:
                return ProcessModifier(convexTransform, ref layer);
        }

        // Shapes
        switch (obj)
        {
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
            
            /*
            case HkpConvexVerticesShapeObject convexVertices:
                return ProcessShape(convexVertices, ref layer);
            */
        }

        // Console.WriteLine($"Failed to ProcessChunkObject with {obj}");
        throw new NotImplementedException($"ProcessChunkObject could not process an object {obj}");
    }

    private StaticDescription[] ProcessContainer(HkpListShapeObject obj, ref LayerContent layer)
    {
        List<StaticDescription> result = new();

        foreach (var childInfo in obj.ChildInfo)
        {
            var childObj = layer.GetTagfileObject(childInfo.Shape);
            try
            {
                var childStaticArr = ProcessChunkObject(childObj, ref layer);
                result.AddRange(childStaticArr);
            }
            catch (NotImplementedException)
            {
                Console.WriteLine($"Ignoring child {childObj} of {obj} because support is not implemented");
            }
        }

        return result.ToArray();
    }

    private StaticDescription[] ProcessContainer(HkpMoppBvTreeShapeObject obj, ref LayerContent layer)
    {
        var childObj = layer.GetTagfileObject(obj.Child);
        return ProcessChunkObject(childObj, ref layer);
    }

    private StaticDescription[] ProcessModifier(HkpConvexTranslateShapeObject obj, ref LayerContent layer)
    {
        var childShapeObj = layer.GetTagfileObject(obj.ChildShape);
        var childShapeStaticArr = ProcessChunkObject(childShapeObj, ref layer);

        var pos = new Vector3(obj.Translation[0], obj.Translation[1], obj.Translation[2]);

        return childShapeStaticArr.Select((StaticDescription childShapeStatic) =>
        {
            childShapeStatic.Pose.Position = pos;
            return childShapeStatic;
        }).ToArray();
    }

    private StaticDescription[] ProcessModifier(HkpTransformShapeObject obj, ref LayerContent layer)
    {
        var childShapeObj = layer.GetTagfileObject(obj.ChildShape);
        var childShapeStaticArr = ProcessChunkObject(childShapeObj, ref layer);
    
        var rot = new Quaternion(obj.Rotation[0], obj.Rotation[1], obj.Rotation[2], obj.Rotation[3]);
        var pos = new Vector3(obj.Transform[3][0], obj.Transform[3][1], obj.Transform[3][2]);

        return childShapeStaticArr.Select((StaticDescription childShapeStatic) =>
        {
            childShapeStatic.Pose.Orientation = rot;
            childShapeStatic.Pose.Position = pos;
            return childShapeStatic;
        }).ToArray();
    }

    private StaticDescription[] ProcessModifier(HkpConvexTransformShapeObject obj, ref LayerContent layer)
    {
        var childShapeObj = layer.GetTagfileObject(obj.ChildShape);
        var childShapeStaticArr = ProcessChunkObject(childShapeObj, ref layer);

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

    private StaticDescription[] ProcessShape(HkpBoxShapeObject obj, ref LayerContent layer)
    {
        var box = new Box(obj.HalfExtents[0] * 2, obj.HalfExtents[1] * 2, obj.HalfExtents[2] * 2);
        var stat = new StaticDescription(RigidPose.Identity,  Simulation.Shapes.Add(box));
        return[stat];
    }

    private StaticDescription[] ProcessShape(HkpSphereShapeObject obj, ref LayerContent layer)
    {
        var sphere = new Sphere(obj.Radius);
        var stat = new StaticDescription(RigidPose.Identity,  Simulation.Shapes.Add(sphere));
        return[stat];
    }

    private StaticDescription[] ProcessShape(HkpCapsuleShapeObject obj, ref LayerContent layer)
    {
        var top = new Vector3(obj.VertexA[0], obj.VertexA[1], obj.VertexA[2]);
        var bot = new Vector3(obj.VertexB[0], obj.VertexB[1], obj.VertexB[2]);
        var mid = Vector3.Multiply(Vector3.Add(top, bot), 0.5f);
        var len = Vector3.Distance(top, bot);
        var dir = Vector3.Normalize(Vector3.Subtract(bot, top));
        var up = new Vector3(0, 1, 0); // yes... idk why

        QuaternionEx.GetQuaternionBetweenNormalizedVectors(up, dir, out Quaternion rot);
        var rad = obj.Radius;
        var capsule = new Capsule(rad, len);

        var pose = RigidPose.Identity;
        pose.Orientation = rot;
        pose.Position = mid;

        var stat = new StaticDescription(pose, Simulation.Shapes.Add(capsule));

        // TEMP
        if (float.IsNaN(stat.Pose.Orientation.X))
        {
            Console.WriteLine($"CAPSULE {obj.Name} {rot}");
            Console.WriteLine($"CAPSULE {obj.Name} STAT {stat.Pose.Orientation}");
            throw new Exception();
        }

        return[stat];
    }

    private StaticDescription[] ProcessShape(HkpCylinderShapeObject obj, ref LayerContent layer)
    {
        var top = new Vector3(obj.VertexA[0], obj.VertexA[1], obj.VertexA[2]);
        var bot = new Vector3(obj.VertexB[0], obj.VertexB[1], obj.VertexB[2]);
        var mid = Vector3.Multiply(Vector3.Add(top, bot), 0.5f);
        var len = Vector3.Distance(top, bot);
        var dir = Vector3.Normalize(Vector3.Subtract(bot, top));
        var up = new Vector3(0, 1, 0); // yes... idk why
        QuaternionEx.GetQuaternionBetweenNormalizedVectors(up, dir, out Quaternion rot);
        var rad = obj.CylRadius;
        var cylinder = new Cylinder(rad, len);

        var pose = RigidPose.Identity;
        pose.Orientation = rot;
        pose.Position = mid;

        var stat = new StaticDescription(pose, Simulation.Shapes.Add(cylinder));
        return[stat];
    }

    private StaticDescription[] ProcessShape(HkpExtendedMeshShapeObject obj, ref LayerContent layer)
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

            var mesh = BepuData.LoadMeshContent(meshContent, BufferPool, scale);
            
            var pose = RigidPose.Identity;
            pose.Orientation = rot;
            pose.Position = pos;

            result.Add(new StaticDescription(pose, Simulation.Shapes.Add(mesh)));
        }

        foreach (var shapepart in obj.ShapesSubparts)
        {
            foreach (var childShape in shapepart.ChildShapes)
            {
                // TODO: Consider rotation and translation of subpart shape
                var childShapeObj = layer.GetTagfileObject(childShape);
                try
                {
                    var childShapeStaticArr = ProcessChunkObject(childShapeObj, ref layer);
                    result.AddRange(childShapeStaticArr);
                }
                catch (NotImplementedException)
                {
                    // Console.WriteLine($"Ignoring child {childShapeObj} of {shapepart} because support is not implemented");
                }
            }
        }

        return result.ToArray();
    }

    private StaticDescription[] ProcessShape(HkpConvexVerticesShapeObject obj, ref LayerContent layer)
    {
        // TODO: ConvexVertices
        // return null;
        throw new NotImplementedException("Fix Pls");
    }

    public struct ChunkRef
    {
        public Vector3 Origin;
        public string Path;
    }
}