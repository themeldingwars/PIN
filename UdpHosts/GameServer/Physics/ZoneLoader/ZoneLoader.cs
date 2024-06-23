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
    private readonly JsonSerializerOptions _serializerOptions = new()
    {
        IncludeFields = true,
        PropertyNameCaseInsensitive = true
    };

    public ZoneLoader(Simulation simulation, BufferPool pool, ThreadDispatcher dispatcher)
    {
        Simulation = simulation;
        BufferPool = pool;
        ThreadDispatcher = dispatcher;

        _serializerOptions.Converters.Add(new TagfileObjectJsonConverter());
        _serializerOptions.Converters.Add(new Vector4Converter());
        _serializerOptions.Converters.Add(new Vector3Converter());
        _serializerOptions.Converters.Add(new StringBooleanConverter());
    }

    public Simulation Simulation { get; protected set; }
    public BufferPool BufferPool { get; private set; }
    public ThreadDispatcher ThreadDispatcher { get; private set; }

    public void LoadCollision(string mapsPath, uint zoneId)
    {
        Stopwatch stopWatch = new Stopwatch();
        stopWatch.Start();

        var zoneFilePath = $"{mapsPath}\\{zoneId}.pinzone.json";
        PinZone zoneData = LoadZoneJSON(zoneFilePath);
        if (zoneData == null)
        {
            Console.WriteLine($"Everything went to shit and we did not load the zone");
        }

        Console.WriteLine($"Loading {zoneData.Chunks.Length} chunks");
        foreach (var chunk in zoneData.Chunks)
        {
            var chunkFilePath = $"{mapsPath}\\chunks\\{chunk.Name}.pinchunk.json";
            LoadChunkJSON(chunk.Origin, chunkFilePath);
        }

        stopWatch.Stop();
        TimeSpan ts = stopWatch.Elapsed;
        string elapsedTime = string.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours,
            ts.Minutes,
            ts.Seconds,
            ts.Milliseconds / 10);
        
        Console.WriteLine($"ZoneLoader LoadCollision Finished in {elapsedTime}");
    }

    private PinZone LoadZoneJSON(string path)
    {
        Console.WriteLine($"ZoneLoader LoadZoneJSON {path}");
        try
        {
            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<PinZone>(json, _serializerOptions);
        }
        catch (Exception e)
        {
            Console.WriteLine($"ZoneLoader LoadZoneJSON Failed: {e}");
            return null;
        }
    }

    private void LoadChunkJSON(Vector3 origin, string path)
    {
        Console.WriteLine($"ZoneLoader LoadChunkJSON {path}");
        try
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            string json = File.ReadAllText(path);
            PinChunk chunk = JsonSerializer.Deserialize<PinChunk>(json, _serializerOptions);

            stopWatch.Stop();
            TimeSpan ts1 = stopWatch.Elapsed;
            string elapsedTime = string.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts1.Hours,
                ts1.Minutes,
                ts1.Seconds,
                ts1.Milliseconds / 10);
            Console.WriteLine($"ZoneLoader LoadChunkJSON Deserialized in {elapsedTime}");
            stopWatch.Restart();

            foreach (PinChunkSubChunk subChunk in chunk.SubChunks) 
            {
                var myLayer = subChunk.Cg;
                var root = subChunk.Cg.GetTagfileObject("#0001");
                var statics = ProcessChunkObject(root, ref myLayer);
                for (int i = 0; i < statics.Length; i++)
                {
                    var stat = statics[i];
                    stat.Pose.Position += origin;
                    Simulation.Statics.Add(stat);
                }

                // TODO: Process subChunk.Cg2, subChunk.Cg3
            }

            stopWatch.Stop();
            TimeSpan ts2 = stopWatch.Elapsed;
            string elapsedTime2 = string.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts2.Hours,
                ts2.Minutes,
                ts2.Seconds,
                ts2.Milliseconds / 10);
            Console.WriteLine($"ZoneLoader LoadChunkJSON Processed in {elapsedTime2}");
        }
        catch (Exception e)
        {
            Console.WriteLine($"ZoneLoader LoadChunkJSON Failed: {e}");
        }
    }

    private StaticDescription[] ProcessChunkObject(BaseTagfileObject obj, ref ENWFLayer layer)
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
            
            /*
            case HkpConvexVerticesShapeObject convexVertices:
                return ProcessShape(convexVertices, ref layer);
            */

            // Containers
            case HkpListShapeObject list:
                return ProcessContainer(list, ref layer);
            case HkpMoppBvTreeShapeObject moppBvTree:
                return ProcessContainer(moppBvTree, ref layer);
            
            // Modifiers
            case HkpConvexTranslateShapeObject convexTranslate:
                return ProcessModifier(convexTranslate, ref layer);
            case HkpTransformShapeObject transform:
                return ProcessModifier(transform, ref layer);
            case HkpConvexTransformShapeObject convexTransform:
                return ProcessModifier(convexTransform, ref layer);
        }

        // Console.WriteLine($"Failed to ProcessChunkObject with {obj}");
        throw new NotImplementedException($"ProcessChunkObject could not process an object {obj}");
    }

    private StaticDescription[] ProcessContainer(HkpListShapeObject obj, ref ENWFLayer layer)
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

    private StaticDescription[] ProcessContainer(HkpMoppBvTreeShapeObject obj, ref ENWFLayer layer)
    {
        var childObj = layer.GetTagfileObject(obj.Child);
        return ProcessChunkObject(childObj, ref layer);
    }

    private StaticDescription[] ProcessModifier(HkpConvexTranslateShapeObject obj, ref ENWFLayer layer)
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

    private StaticDescription[] ProcessModifier(HkpTransformShapeObject obj, ref ENWFLayer layer)
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

    private StaticDescription[] ProcessModifier(HkpConvexTransformShapeObject obj, ref ENWFLayer layer)
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

    private StaticDescription[] ProcessShape(HkpBoxShapeObject obj, ref ENWFLayer layer)
    {
        var box = new Box(obj.HalfExtents[0] * 2, obj.HalfExtents[1] * 2, obj.HalfExtents[2] * 2);
        var stat = new StaticDescription(RigidPose.Identity,  Simulation.Shapes.Add(box));
        return[stat];
    }

    private StaticDescription[] ProcessShape(HkpSphereShapeObject obj, ref ENWFLayer layer)
    {
        var sphere = new Sphere(obj.Radius);
        var stat = new StaticDescription(RigidPose.Identity,  Simulation.Shapes.Add(sphere));
        return[stat];
    }

    private StaticDescription[] ProcessShape(HkpCapsuleShapeObject obj, ref ENWFLayer layer)
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

    private StaticDescription[] ProcessShape(HkpCylinderShapeObject obj, ref ENWFLayer layer)
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

    private StaticDescription[] ProcessShape(HkpExtendedMeshShapeObject obj, ref ENWFLayer layer)
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

            var mesh = BepuData.LoadMeshContent(meshContent, BufferPool, scale, ThreadDispatcher);
            
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

    private StaticDescription[] ProcessShape(HkpConvexVerticesShapeObject obj, ref ENWFLayer layer)
    {
        // TODO: ConvexVertices
        // return null;
        throw new NotImplementedException("Fix Pls");
    }

    private class PinZone
    {
        public string Name;

        // public ulong Timestamp
        // public uint GeneratedAt;
        public PinZoneChunk[] Chunks;
        public ENWFLayer[] Imports;
    }

    private class PinZoneChunk
    {
        public string Name;
        public Vector3 Origin;
    }

    private class PinChunk
    {
        public string Name;
        public PinChunkSubChunk[] SubChunks;
    }

    private class PinChunkSubChunk
    {
        public string Name;
        public ENWFLayer Cg;
        public ENWFLayer Cg2 = null;
        public ENWFLayer Cg3 = null;
    }
}