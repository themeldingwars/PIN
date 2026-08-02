using System.Diagnostics;
using System.Numerics;
using BepuPhysics;
using BepuUtilities;
using BepuUtilities.Memory;
using Serilog;

namespace Shared.Collision.Cache;

public static class RigidBodyCache
{
    private const int _formatVersion = 1;
    private static readonly byte[] _magic = "PRBC"u8.ToArray();
    private static readonly ILogger _logger = Log.ForContext(typeof(RigidBodyCache));

    public static string GetCachePath(string cacheDir, string assetId)
    {
        var dir = Path.Combine(cacheDir, "rigidbodies");
        Directory.CreateDirectory(dir);
        return Path.Combine(dir, $"{assetId}.rbcache");
    }

    public static void Save(Simulation simulation, BufferPool pool, StaticDescription[] statics, string path)
    {
        var stopwatch = Stopwatch.StartNew();

        using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 65536);
        using var writer = new BinaryWriter(fs);

        writer.Write(_magic);
        writer.Write(_formatVersion);
        writer.Write(statics.Length);

        for (int i = 0; i < statics.Length; i++)
        {
            var stat = statics[i];

            writer.Write(stat.Shape.Type);

            writer.Write(stat.Pose.Position.X);
            writer.Write(stat.Pose.Position.Y);
            writer.Write(stat.Pose.Position.Z);
            writer.Write(stat.Pose.Orientation.X);
            writer.Write(stat.Pose.Orientation.Y);
            writer.Write(stat.Pose.Orientation.Z);
            writer.Write(stat.Pose.Orientation.W);

            ShapeSerializer.WriteShape(simulation, pool, writer, stat.Shape);
        }

        stopwatch.Stop();
        _logger.Information("RigidBodyCache: Saved {StaticCount} statics to {Path} in {Elapsed}ms", statics.Length, path, stopwatch.ElapsedMilliseconds);
    }

    public static bool TryLoad(Simulation simulation, BufferPool pool, ThreadDispatcher dispatcher, string path, out StaticDescription[] statics)
    {
        statics = [];

        if (!File.Exists(path))
        {
            return false;
        }

        try
        {
            var stopwatch = Stopwatch.StartNew();

            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 65536);
            using var reader = new BinaryReader(fs);

            var magic = reader.ReadBytes(4);
            if (magic.Length < 4 || magic[0] != _magic[0] || magic[1] != _magic[1] || magic[2] != _magic[2] || magic[3] != _magic[3])
            {
                _logger.Error("RigidBodyCache: Invalid magic header.");
                return false;
            }

            var version = reader.ReadInt32();
            if (version != _formatVersion)
            {
                _logger.Information("RigidBodyCache: Version mismatch (expected {_formatVersion}, got {version}).", _formatVersion, version);
                return false;
            }

            var staticCount = reader.ReadInt32();
            var result = new StaticDescription[staticCount];

            for (int i = 0; i < staticCount; i++)
            {
                var shapeTypeId = reader.ReadInt32();

                var pose = new RigidPose
                {
                    Position = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()),
                    Orientation = new Quaternion(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle())
                };

                var shapeIndex = ShapeSerializer.ReadShape(simulation, pool, dispatcher, reader, shapeTypeId);
                result[i] = new StaticDescription(pose, shapeIndex);
            }

            statics = result;

            stopwatch.Stop();
            _logger.Information("RigidBodyCache: Loaded {StaticCount} statics from cache in {Elapsed}ms", staticCount, stopwatch.ElapsedMilliseconds);
            return true;
        }
        catch (Exception e)
        {
            _logger.Error("RigidBodyCache: Failed to load: {Message} ({Type})", e.Message, e.GetType().Name);
            return false;
        }
    }
}
