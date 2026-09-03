using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using Serilog;

namespace GameServer.Systems.World;

public record HiveSite(string Name, float X, float Y, float Z, float Radius, int Burst);

/// <summary>
///     Authored underground-population sites. In the game fiction, aranhas
///     stay below the ground until seismic activity wakes them. A thumper
///     that drills near a hive site wakes a dense attack instead of the
///     small default waves. Place sites in the game with the admin command
///     "hive add". Sites persist as one JSON file for each zone.
/// </summary>
public class HiveIndex
{
    private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };
    private readonly ILogger _logger = Log.ForContext<HiveIndex>();
    private readonly string _path;
    private readonly object _lock = new();
    private List<HiveSite> _sites = new();

    public HiveIndex(string dataDir, uint zoneId)
    {
        _path = string.IsNullOrWhiteSpace(dataDir)
            ? null
            : Path.Combine(dataDir, $"hives_{zoneId}.json");
        Load();
    }

    public IReadOnlyList<HiveSite> Sites
    {
        get
        {
            lock (_lock)
            {
                return _sites.ToList();
            }
        }
    }

    public void Add(HiveSite site)
    {
        lock (_lock)
        {
            _sites.Add(site);
            Save();
        }

        _logger.Information("Hive site added: {Name} at ({X},{Y},{Z}) radius {Radius} burst {Burst}", site.Name, site.X, site.Y, site.Z, site.Radius, site.Burst);
    }

    public int Clear()
    {
        lock (_lock)
        {
            var count = _sites.Count;
            _sites.Clear();
            Save();
            return count;
        }
    }

    /// <summary>
    ///     Returns the hive influence at a position, between 0 and 1. The
    ///     value falls linearly from 1 at the center of the strongest site
    ///     to 0 at its radius.
    /// </summary>
    public float InfluenceAt(Vector3 position, out HiveSite strongest)
    {
        strongest = null;
        var best = 0f;
        lock (_lock)
        {
            foreach (var site in _sites)
            {
                var dx = position.X - site.X;
                var dy = position.Y - site.Y;
                var distance = MathF.Sqrt((dx * dx) + (dy * dy));
                if (distance >= site.Radius)
                {
                    continue;
                }

                var falloff = 1f - (distance / site.Radius);
                if (falloff > best)
                {
                    best = falloff;
                    strongest = site;
                }
            }
        }

        return best;
    }

    private void Load()
    {
        if (_path == null || !File.Exists(_path))
        {
            return;
        }

        try
        {
            _sites = JsonSerializer.Deserialize<List<HiveSite>>(File.ReadAllText(_path)) ?? new List<HiveSite>();
            _logger.Information("Loaded {Count} hive sites from {Path}", _sites.Count, _path);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to load hive sites from {Path}", _path);
        }
    }

    private void Save()
    {
        if (_path == null)
        {
            return;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        File.WriteAllText(_path, JsonSerializer.Serialize(_sites, _jsonOptions));
    }
}
