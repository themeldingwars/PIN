using System.Diagnostics;
using BepuPhysics;
using BepuUtilities;
using BepuUtilities.Memory;
using Serilog;
using Shared.Collision.Layers;
using Shared.Collision.Zone;

namespace Shared.Collision.ZoneLoading;

public class ZoneLoader
{
    private static readonly ILogger _logger = Log.Logger.ForContext<ZoneLoader>();

    private readonly Simulation _simulation;
    private readonly BufferPool _pool;
    private readonly ThreadDispatcher _dispatcher;
    private readonly string _mapsPath;
    private readonly string _cachePath;

    public ZoneLoader(Simulation simulation, BufferPool pool, ThreadDispatcher dispatcher, string mapsPath, string cachePath)
    {
        _simulation = simulation;
        _pool = pool;
        _dispatcher = dispatcher;
        _mapsPath = mapsPath;
        _cachePath = cachePath;
    }

    public long? LoadZone(uint zoneId, bool forceReload = false)
    {
        var stopwatch = Stopwatch.StartNew();

        var zoneFilePath = Path.Combine(_mapsPath, $"{zoneId}.zone");

        if (!File.Exists(zoneFilePath))
        {
            _logger.Error("Zone file not found: {Path}", zoneFilePath);
            return null;
        }

        var zone = ZoneFileReader.Read(zoneFilePath);

        if (zone.Root is not ZoneRootLayer rootLayer)
        {
            _logger.Error("Invalid zone root layer for zone {ZoneId}", zoneId);
            return null;
        }

        var chunkRefs = ChunkOriginCalculator.ExtractChunks(rootLayer, zoneId);

        _logger.Information($"Zone {{ZoneId}} ({{ZoneName}}): References {{Count}} {(chunkRefs.Length == 1 ? "chunk" : "chunks")}", zoneId, zone.Name, chunkRefs.Length);

        foreach (var chunkRef in chunkRefs)
        {
            _logger.Information("Loading chunk ({CurrentCount}/{TotalCount}) {ChunkName}", chunkRefs.IndexOf(chunkRef) + 1, chunkRefs.Length, chunkRef.Name);
            var chunkPath = Path.Combine(_mapsPath, "chunks", $"{chunkRef.Name}.gtchunk");

            var statics = ChunkProcessor.ProcessChunk(chunkPath, _cachePath, _simulation, _pool, _dispatcher, forceReload);

            if (statics.Length == 0)
            {
                continue;
            }

            foreach (var staticsItem in statics)
            {
                var adjusted = staticsItem;
                adjusted.Pose.Position += chunkRef.Origin;
                _simulation.Statics.Add(adjusted);
            }
        }

        stopwatch.Stop();
        _logger.Information("Zone {ZoneId}: Loaded successfully in {Duration}. Total statics: {Count}", zoneId, stopwatch.Elapsed, _simulation.Statics.Count);

        return zone.Timestamp;
    }
}
