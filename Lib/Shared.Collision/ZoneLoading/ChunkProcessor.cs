using BepuPhysics;
using BepuUtilities;
using BepuUtilities.Memory;
using FauFau.Formats;
using FauFau.Formats.GtChunk;
using Serilog;
using Shared.Collision.Cache;
using Shared.Collision.Tagfile;

namespace Shared.Collision.ZoneLoading;

public static class ChunkProcessor
{
    private const uint _collisionLod = 3;

    private static readonly ILogger _logger = Log.ForContext(typeof(ChunkProcessor));

    public static StaticDescription[] ProcessChunk(
        string chunkPath,
        string cachePath,
        Simulation simulation,
        BufferPool pool,
        ThreadDispatcher dispatcher,
        bool forceReload = false)
    {
        var chunkName = Path.GetFileNameWithoutExtension(chunkPath);
        var cacheFile = ChunkCache.GetCachePath(cachePath, chunkName);

        if (!forceReload && ChunkCache.TryLoad(simulation, pool, dispatcher, cacheFile, out var cached))
        {
            return cached;
        }

        var chunk = new GtChunkV8();
        chunk.Load(chunkPath);

        var collisionMeshes = FindAllLod3CollisionMeshes(chunk, chunkName);

        if (collisionMeshes.Length == 0)
        {
            _logger.Warning("Chunk {Name} has no LOD3 collision layers, what?", chunkName);
            return [];
        }

        var loader = new TagfileLoader(simulation, pool, dispatcher);

        List<StaticDescription> allStatics = [];

        foreach (var mesh in collisionMeshes)
        {
            var hkxBytes = mesh.HavokData;

            if (hkxBytes.Length == 0)
            {
                continue;
            }

            var vertBlocks = EnwfToBepuConverter.ConvertVertBlocks(mesh.Verts);
            var indiceBlocks = EnwfToBepuConverter.ConvertIndiceBlocks(mesh.IndiceBlocks);
            var statics = loader.ProcessTagfileBytes(hkxBytes, vertBlocks, indiceBlocks);

            if (statics.Length > 0)
            {
                allStatics.AddRange(statics);
            }
        }

        var result = allStatics.ToArray();

        ChunkCache.Save(simulation, pool, result, cacheFile);

        return result;
    }

    private static GtChunk_MeshData[] FindAllLod3CollisionMeshes(GtChunkV8 chunk, string chunkName)
    {
        List<GtChunk_MeshData> result = [];

        for (int lod = 0; lod < chunk.Root.LodNodes.Length; lod++)
        {
            if (chunk.Root.LodNodes[lod].LodIdx != _collisionLod)
            {
                continue;
            }

            var layers = chunk.GetLodLayers(lod)
                .Concat(chunk.LodDataMap[lod].DatBlockIds.SelectMany(subChunk => chunk.GetSubChunkLayers(subChunk)));

            foreach (var layer in layers)
            {
                if (layer.Id != WorldLayerIds.StaticGeometryCollision)
                {
                    continue;
                }

                try
                {
                    result.Add(GtChunk_MeshData.Read(layer.Data));
                }
                catch (Exception e)
                {
                    _logger.Warning("Chunk {Name} has a collision layer that can't be read: {Error}", chunkName, e.Message);
                }
            }
        }

        return [.. result];
    }
}
