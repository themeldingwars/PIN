using System.Diagnostics;
using BepuPhysics;
using BepuUtilities;
using BepuUtilities.Memory;
using FauFau.Formats;
using Serilog;

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

    /// <summary>
    ///     World position -> chunk/subchunk index, rebuilt on each zone load. Published atomically so
    ///     the physics debug renderer (own thread) can resolve the camera position without locking.
    /// </summary>
    public ZoneSpatialIndex SpatialIndex { get; } = new ZoneSpatialIndex();

    public long? LoadZone(uint zoneId, bool forceReload = false)
    {
        var stopwatch = Stopwatch.StartNew();

        var zoneFilePath = Path.Combine(_mapsPath, $"{zoneId}.zone");

        if (!File.Exists(zoneFilePath))
        {
            _logger.Error("Zone file not found: {Path}", zoneFilePath);
            return null;
        }

        var zone = new Zone();
        zone.Read(zoneFilePath);

        if (zone.Magic != "ZONE" || zone.Root?.Id != WorldLayerIds.ZoneRoot)
        {
            _logger.Error("Invalid zone root layer for zone {ZoneId}", zoneId);
            return null;
        }

        var chunkRefs = ChunkOriginCalculator.ExtractChunks(zone, zoneId);

        _logger.Information($"Zone {{ZoneId}} ({{ZoneName}}): References {{Count}} {(chunkRefs.Length == 1 ? "chunk" : "chunks")}", zoneId, zone.Name, chunkRefs.Length);

        foreach (var chunkRef in chunkRefs)
        {
            _logger.Information("Loading chunk ({CurrentCount}/{TotalCount}) {ChunkName}", Array.IndexOf(chunkRefs, chunkRef) + 1, chunkRefs.Length, chunkRef.Name);
            var chunkPath = Path.Combine(_mapsPath, "chunks", $"{chunkRef.Name}.gtchunk");

            // Build the spatial index from the subchunk bounds (header-only read, no decompression) so
            // the debug renderer can map the camera world position to a chunk/subchunk name.
            try
            {
                var chunkHeader = ChunkHeaderReader.ReadHeader(chunkPath);
                SpatialIndex.AddChunk(chunkRef, chunkHeader);
            }
            catch (Exception e)
            {
                _logger.Warning("Could not read subchunk bounds for chunk {ChunkName}: {Message}", chunkRef.Name, e.Message);
            }

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

        SpatialIndex.Publish();
        _logger.Information("Zone {ZoneId} spatial index: {Chunks} chunks, {SubChunks} subchunks", zoneId, SpatialIndex.ChunkCount, SpatialIndex.SubChunkCount);

        stopwatch.Stop();
        _logger.Information("Zone {ZoneId}: Loaded successfully in {Duration}. Total statics: {Count}", zoneId, stopwatch.Elapsed, _simulation.Statics.Count);

        return new DateTimeOffset(zone.TimeStamp).ToUnixTimeMilliseconds();
    }
}
