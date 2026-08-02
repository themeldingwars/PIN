#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities;
using BepuUtilities.Memory;
using CommandLine;
using Serilog;
using Shared.Collision;
using Shared.Collision.Layers;
using Shared.Collision.Zone;
using Shared.Collision.ZoneLoading;

namespace CollisionGenerator;

public class Options
{
    [Option('c', "cache-path", Required = true, HelpText = "Output directory for cache files")]
    public string CachePath { get; set; } = null!;

    [Option('m', "maps-path", Required = false, HelpText = "Path to .zone and .gtchunk files")]
    public string? MapsPath { get; set; }

    [Option("chunk-name", Required = false, HelpText = "Process single .gtchunk file by name (without extension)")]
    public string? ChunkName { get; set; }

    [Option("all-chunks", Required = false, HelpText = "Process ALL .gtchunk files in maps/chunks/")]
    public bool AllChunks { get; set; }

    [Option('z', "zone-id", Required = false, HelpText = "Load single zone by ID")]
    public uint? ZoneId { get; set; }

    [Option("all-zones", Required = false, HelpText = "Process all chunks for all zones (alias for --all-chunks)")]
    public bool AllZones { get; set; }

    [Option('a', "asset-db-path", Required = false, HelpText = "Path to system/assetdb")]
    public string? AssetDbPath { get; set; }

    [Option("asset-id", Required = false, HelpText = "Process single asset by ID")]
    public string? AssetId { get; set; }

    [Option("all-assets", Required = false, HelpText = "Process ALL .hkx files in assetdb")]
    public bool AllAssets { get; set; }

    [Option('j', "jobs", Required = false, Default = 0, HelpText = "Number of parallel jobs (default: CPU count - 2)")]
    public int Jobs { get; set; }
}

internal static class Program
{
    private static readonly ILogger Logger = Log.ForContext(typeof(Program));

    private static int Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        var parser = new Parser(c =>
        {
            c.CaseInsensitiveEnumValues = true;
            c.IgnoreUnknownArguments = false;
        });

        var result = parser.ParseArguments<Options>(args);

        Options? opts = null;
        bool success = false;

        result.WithParsed(o =>
        {
            opts = o;
            success = true;
        })
        .WithNotParsed(errs =>
         {
            foreach (var e in errs)
            {
                Console.Error.WriteLine($"Error: {e.Tag} - {e.ToString()}");
            }

            PrintUsage();
        });

        if (!success || opts is null)
        {
            return 1;
        }

        return Run(opts);
    }

    private static int Run(Options opts)
    {
        var jobs = opts.Jobs > 0 ? opts.Jobs : Math.Max(1, Environment.ProcessorCount - 2);

        var stopwatch = Stopwatch.StartNew();
        var chunkTime = TimeSpan.Zero;
        var zoneTime = TimeSpan.Zero;
        var assetTime = TimeSpan.Zero;

        var hasChunkMode = !string.IsNullOrEmpty(opts.ChunkName) || opts.AllChunks;
        var hasZoneMode = opts.ZoneId.HasValue || opts.AllZones;
        var hasAssetMode = !string.IsNullOrEmpty(opts.AssetId) || opts.AllAssets;

        if (!hasChunkMode && !hasZoneMode && !hasAssetMode)
        {
            Console.Error.WriteLine("Error: No mode selected. Specify at least one: chunk, zone, or asset mode.");
            PrintUsage();
            return 1;
        }

        if (!string.IsNullOrEmpty(opts.ChunkName) && opts.AllChunks)
        {
            Console.Error.WriteLine("Error: --chunk-name and --all-chunks are mutually exclusive.");
            PrintUsage();
            return 1;
        }

        if (opts.ZoneId.HasValue && opts.AllZones)
        {
            Console.Error.WriteLine("Error: --zone-id and --all-zones are mutually exclusive.");
            PrintUsage();
            return 1;
        }

        if (!string.IsNullOrEmpty(opts.AssetId) && opts.AllAssets)
        {
            Console.Error.WriteLine("Error: --asset-id and --all-assets are mutually exclusive.");
            PrintUsage();
            return 1;
        }

        if ((hasChunkMode || hasZoneMode) && string.IsNullOrEmpty(opts.MapsPath))
        {
            Console.Error.WriteLine("Error: --maps-path is required for chunk and zone modes.");
            PrintUsage();
            return 1;
        }

        if (hasAssetMode && string.IsNullOrEmpty(opts.AssetDbPath))
        {
            Console.Error.WriteLine("Error: --asset-db-path is required for asset mode.");
            PrintUsage();
            return 1;
        }

        try
        {
            Directory.CreateDirectory(opts.CachePath);
        }
        catch (Exception e)
        {
            Logger.Error("Cannot create cache path: {Error}", e.Message);
            return 1;
        }

        // Phase 1: Chunk phase (if --all-chunks or --all-zones)
        if (opts.AllChunks || opts.AllZones)
        {
            Logger.Information("=== CHUNK PHASE: Processing all chunks ===");
            var chunkNames = DiscoverChunks(opts.MapsPath!);
            if (chunkNames.Length == 0)
            {
                Logger.Warning("No .gtchunk files found in {Path}", Path.Combine(opts.MapsPath!, "chunks"));
            }
            else
            {
                Logger.Information("Found {Count} chunk files", chunkNames.Length);
                ParallelForEach(chunkNames, jobs, (name, ct) =>
                {
                    ProcessSingleChunk(name, opts.MapsPath!, opts.CachePath);
                });
            }
        }
        else if (!string.IsNullOrEmpty(opts.ChunkName))
        {
            Logger.Information("=== CHUNK PHASE: Processing single chunk '{Name}' ===", opts.ChunkName);
            ProcessSingleChunk(opts.ChunkName, opts.MapsPath!, opts.CachePath);
        }

        chunkTime = stopwatch.Elapsed;
        stopwatch.Restart();

        // Phase 2: Zone phase
        if (opts.ZoneId.HasValue)
        {
            Logger.Information("=== ZONE PHASE: Loading zone {ZoneId} ===", opts.ZoneId.Value);
            ProcessSingleZone(opts.ZoneId.Value, opts.MapsPath!, opts.CachePath, jobs);
        }

        zoneTime = stopwatch.Elapsed;
        stopwatch.Restart();

        // Phase 3: Asset phase
        if (!string.IsNullOrEmpty(opts.AssetId))
        {
            Logger.Information("=== ASSET PHASE: Loading asset '{AssetId}' ===", opts.AssetId);
            ProcessSingleAsset(opts.AssetId, opts.AssetDbPath!, opts.CachePath);
        }
        else if (opts.AllAssets)
        {
            Logger.Information("=== ASSET PHASE: Loading all assets ===");
            var assetIds = DiscoverAssets(opts.AssetDbPath!);
            if (assetIds.Length == 0)
            {
                Logger.Warning("No asset files found in {Path}", opts.AssetDbPath!);
            }
            else
            {
                Logger.Information("Found {Count} asset files", assetIds.Length);
                ParallelForEach(assetIds, jobs, (assetId, ct) =>
                {
                    ProcessSingleAsset(assetId, opts.AssetDbPath!, opts.CachePath);
                });
            }
        }

        assetTime = stopwatch.Elapsed;
        stopwatch.Stop();

        Logger.Information("=== Summary ===");
        Logger.Information("  Chunk phase: {Elapsed}", chunkTime);
        Logger.Information("  Zone phase:  {Elapsed}", zoneTime);
        Logger.Information("  Asset phase: {Elapsed}", assetTime);
        Logger.Information("  Total:       {Elapsed}", stopwatch.Elapsed);
        Logger.Information("Done.");
        return 0;
    }

    private static void PrintUsage()
    {
        Console.WriteLine(@"
Usage: CollisionGenerator [options]

Required:
  --cache-path, -c    Output directory for cache files (REQUIRED)

Chunk mode (requires --maps-path):
  --maps-path, -m     Path to .zone and .gtchunk files
  --chunk-name        Process a single .gtchunk file by name (without extension)
  --all-chunks        Process ALL .gtchunk files in maps/chunks/
  Note: --chunk-name and --all-chunks are mutually exclusive

Zone mode (requires --maps-path):
  --maps-path, -m     Path to .zone and .gtchunk files
  --zone-id, -z       Load a single zone by ID
  --all-zones         Process all chunks for all zones (alias for --all-chunks)
  Note: --zone-id and --all-zones are mutually exclusive

Asset mode (requires --asset-db-path):
  --asset-db-path, -a Path to system/assetdb
  --asset-id          Process a single asset by ID
  --all-assets        Process ALL .hkx files in assetdb
  Note: --asset-id and --all-assets are mutually exclusive

Other:
  --jobs, -j          Number of parallel jobs (default: CPU count - 2)

Examples:
  # Process a single chunk
  CollisionGenerator -c ./cache -m ./maps --chunk-name chunk_001

  # Process all chunks
  CollisionGenerator -c ./cache -m ./maps --all-chunks

  # Load a single zone
  CollisionGenerator -c ./cache -m ./maps -z 1

  # Process all zones (alias for --all-chunks)
  CollisionGenerator -c ./cache -m ./maps --all-zones

  # Process a single asset
  CollisionGenerator -c ./cache -a ./assetdb --asset-id 12345

  # Process all assets
  CollisionGenerator -c ./cache -a ./assetdb --all-assets

  # Process all zones AND all assets
  CollisionGenerator -c ./cache -m ./maps --all-zones -a ./assetdb --all-assets
");
    }

    private static string[] DiscoverChunks(string mapsPath)
    {
        var chunksDir = Path.Combine(mapsPath, "chunks");
        if (!Directory.Exists(chunksDir))
        {
            return [];
        }

        return Directory.GetFiles(chunksDir, "*.gtchunk")
            .Select(f => Path.GetFileNameWithoutExtension(f))
            .OrderBy(n => n)
            .ToArray();
    }

    private static string[] DiscoverAssets(string assetDbPath)
    {
        if (!Directory.Exists(assetDbPath))
        {
            return [];
        }

        var result = new List<string>();
        foreach (var dir in Directory.EnumerateDirectories(assetDbPath))
        {
            var dirName = Path.GetFileName(dir);
            if (dirName.All(char.IsDigit))
            {
                var hkxFiles = Directory.GetFiles(dir, "*.hkx");
                foreach (var file in hkxFiles)
                {
                    var assetId = Path.GetFileNameWithoutExtension(file);
                    result.Add(assetId);
                }
            }
        }

        return result.OrderBy(a => a).ToArray();
    }

    private static void ProcessSingleChunk(string label, string mapsPath, string cachePath)
    {
        var pool = new BufferPool();
        var disp = new ThreadDispatcher(1);
        var sim = Simulation.Create(pool, new NarrowPhaseCallbacks(), new PoseIntegratorCallbacks(new Vector3(0, 0, -8)), new SolveDescription(8, 1));

        try
        {
            var gtchunkPath = Path.Combine(mapsPath, "chunks", $"{label}.gtchunk");
            Logger.Information("Processing chunk: {Label} ({Path})", label, gtchunkPath);

            var statics = ChunkProcessor.ProcessChunk(gtchunkPath, cachePath, sim, pool, disp, true);
            Logger.Information("Chunk {Label}: Processed {Count} static descriptions", label, statics.Length);
        }
        catch (Exception e)
        {
            Logger.Error("Failed to process chunk {Label}: {Error}", label, e.Message);
        }
        finally
        {
            sim.Dispose();
            pool.Clear();
            disp.Dispose();
        }
    }

    private static void ProcessSingleZone(uint zoneId, string mapsPath, string cachePath, int jobs)
    {
        Logger.Information("Loading zone: {ZoneId}", zoneId);

        var zoneFilePath = Path.Combine(mapsPath, $"{zoneId}.zone");
        if (!File.Exists(zoneFilePath))
        {
            Logger.Error("Zone file not found: {Path}", zoneFilePath);
            return;
        }

        try
        {
            var zone = ZoneFileReader.Read(zoneFilePath);
            if (zone.Root is not ZoneRootLayer rootLayer)
            {
                Logger.Error("Invalid zone root layer for zone {ZoneId}", zoneId);
                return;
            }

            var chunkRefs = ChunkOriginCalculator.ExtractChunks(rootLayer, zoneId);
            Logger.Information("Zone {ZoneId}: found {Count} chunks to process", zoneId, chunkRefs.Length);

            var chunkNames = chunkRefs.Select(ref_ => ref_.Name).ToArray();
            ParallelForEach(chunkNames, jobs, (name, ct) => ProcessSingleChunk(name, mapsPath, cachePath));
        }
        catch (Exception e)
        {
            Logger.Error("Failed to load zone {ZoneId}: {Error}", zoneId, e.Message);
        }
    }

    private static void ProcessSingleAsset(string assetId, string assetDbPath, string cachePath)
    {
        var pool = new BufferPool();
        var disp = new ThreadDispatcher(1);
        var sim = Simulation.Create(pool, new NarrowPhaseCallbacks(), new PoseIntegratorCallbacks(new Vector3(0, 0, -8)), new SolveDescription(8, 1));

        try
        {
            Logger.Information("Loading asset: {AssetId}", assetId);
            var loader = new RigidBodyLoader(sim, pool, disp, assetDbPath, cachePath);
            var statics = loader.Load(assetId);
            Logger.Information("Asset {AssetId}: Loaded {Count} static descriptions", assetId, statics.Length);
        }
        catch (Exception e)
        {
            Logger.Error("Failed to load asset {AssetId}: {Error}", assetId, e.Message);
        }
        finally
        {
            sim.Dispose();
            pool.Clear();
            disp.Dispose();
        }
    }

    private static void ParallelForEach<T>(T[] items, int maxDegree, Action<T, CancellationToken> action)
    {
        var cts = new CancellationTokenSource();
        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = maxDegree,
            CancellationToken = cts.Token
        };

        try
        {
            Parallel.ForEach(items, options, item =>
            {
                action(item, cts.Token);
            });
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception e)
        {
            Logger.Error("Parallel processing failed: {Error}", e.Message);
            cts.Cancel();
        }
    }
}
