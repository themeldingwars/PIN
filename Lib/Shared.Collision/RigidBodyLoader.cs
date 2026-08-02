using BepuPhysics;
using BepuUtilities;
using BepuUtilities.Memory;
using Serilog;
using Shared.Collision.Cache;
using Shared.Collision.Tagfile;
using Shared.Collision.Utilities;

namespace Shared.Collision;

public class RigidBodyLoader
{
    private static readonly ILogger _logger = Log.ForContext<RigidBodyLoader>();

    private readonly Simulation _simulation;
    private readonly BufferPool _bufferPool;
    private readonly ThreadDispatcher _threadDispatcher;
    private readonly TagfileLoader _tagfileLoader;
    private readonly string _assetDBPath;
    private readonly string _cachePath;

    public RigidBodyLoader(Simulation simulation, BufferPool bufferPool, ThreadDispatcher threadDispatcher, string assetDBPath, string cachePath)
    {
        _simulation = simulation;
        _bufferPool = bufferPool;
        _threadDispatcher = threadDispatcher;
        _tagfileLoader = new TagfileLoader(simulation, bufferPool, threadDispatcher);
        _assetDBPath = assetDBPath;
        _cachePath = cachePath;
    }

    public StaticDescription[] Load(string assetId)
    {
        if (!string.IsNullOrEmpty(_cachePath))
        {
            var cachePath = RigidBodyCache.GetCachePath(_cachePath, assetId);
            if (RigidBodyCache.TryLoad(_simulation, _bufferPool, _threadDispatcher, cachePath, out var cached))
            {
                return cached;
            }
        }

        StaticDescription[] loaded = LoadFromHKX(assetId);
        if (loaded.Length > 0 && !string.IsNullOrEmpty(_cachePath))
        {
            var cachePath = RigidBodyCache.GetCachePath(_cachePath, assetId);
            RigidBodyCache.Save(_simulation, _bufferPool, loaded, cachePath);
        }

        return loaded;
    }

    private StaticDescription[] LoadFromHKX(string assetId)
    {
        try
        {
            if (string.IsNullOrEmpty(_assetDBPath))
            {
                return [];
            }

            string hkxPath = Path.Combine(_assetDBPath, AssetPathResolver.ComputeFolderName(assetId));
            if (!Directory.Exists(hkxPath))
            {
                return [];
            }

            hkxPath = Directory.EnumerateFiles(hkxPath, $"{assetId}.*")
                .FirstOrDefault(f => Path.GetFileNameWithoutExtension(f) == assetId) ?? string.Empty;

            if (hkxPath == string.Empty)
            {
                _logger.Warning("RigidBody HKX not found: {AssetId}", assetId);
                return [];
            }

            byte[] hkxBytes = File.ReadAllBytes(hkxPath);
            return _tagfileLoader.ProcessTagfileBytes(hkxBytes);
        }
        catch (Exception e)
        {
            _logger.Error("LoadRigidBody Failed: {Message} ({Type}) on {AssetId}\n{StackTrace}", e.Message, e.GetType().Name, assetId, e.StackTrace);
            return [];
        }
    }
}
