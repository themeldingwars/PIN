using System;
using System.Collections.Concurrent;
using System.IO;
using System.Numerics;
using FauFau.Formats;
using Serilog;
using Shared.Collision.ZoneLoading;

namespace GameServer.Systems.ZoneBorders;

/// <summary>
///     A zone's frame and transfer bounds, read once per zone from its .zone file. Both are null when the file is missing
///     or broken, then the zone has no border to cross.
/// </summary>
public sealed record ZoneGeometry(uint ZoneId, ZoneFrame? Frame, ZoneTransferBounds TransferBounds)
{
    private static readonly ILogger _logger = Log.ForContext<ZoneGeometry>();
    private static readonly ConcurrentDictionary<uint, Lazy<ZoneGeometry>> _zones = new();

    public static ZoneGeometry Get(uint zoneId, string mapsPath)
    {
        return _zones.GetOrAdd(zoneId, id => new Lazy<ZoneGeometry>(() => Read(id, mapsPath))).Value;
    }

    /// <summary>
    ///     The position in the neighbor's frame, when the neighbor is the zone this one's transfer bounds lead to. Null
    ///     for any other zone, a position converted there would be nowhere near its land.
    /// </summary>
    public Vector3? ToNeighbor(Vector3 position, ZoneGeometry neighbor)
    {
        if (TransferBounds?.TargetZoneId != neighbor.ZoneId || Frame is not { } from || neighbor.Frame is not { } to)
        {
            return null;
        }

        return from.Convert(position, to);
    }

    private static ZoneGeometry Read(uint zoneId, string mapsPath)
    {
        var path = Path.Combine(mapsPath, $"{zoneId}.zone");
        try
        {
            if (!File.Exists(path))
            {
                return new ZoneGeometry(zoneId, null, null);
            }

            var zone = new Zone();
            zone.Read(path);
            var bounds = ZoneTransferBounds.Read(zone);
            if (bounds != null)
            {
                _logger.Information("Zone {ZoneId} borders zone {TargetZoneId}, {Polygons} transfer polygons", zoneId, bounds.TargetZoneId, bounds.Polygons.Count);
            }

            return new ZoneGeometry(zoneId, ChunkOriginCalculator.Frame(zone, zoneId), bounds);
        }
        catch (Exception e)
        {
            _logger.Error(e, "Couldn't read the borders of zone {ZoneId} from {Path}", zoneId, path);
            return new ZoneGeometry(zoneId, null, null);
        }
    }
}
