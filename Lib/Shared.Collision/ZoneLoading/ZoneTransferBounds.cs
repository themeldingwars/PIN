using System.Buffers.Binary;
using System.Numerics;
using FauFau.Formats;

namespace Shared.Collision.ZoneLoading;

/// <summary>
///     The area of a zone that belongs to its neighbor: walking into it moves the player there. Coral Forest has
///     Sertao's land, Sertao has Coral Forest's, and the two don't overlap, so a player who just arrived isn't in the new
///     zone's area. The client draws the edges as a wall when a player comes close.
/// </summary>
public sealed class ZoneTransferBounds
{
    private const int _maxPolygons = 255;

    public ZoneTransferBounds(uint targetZoneId, IReadOnlyList<Vector2[]> polygons)
    {
        TargetZoneId = targetZoneId;
        Polygons = polygons;
    }

    public uint TargetZoneId { get; }

    public IReadOnlyList<Vector2[]> Polygons { get; }

    /// <summary>
    ///     Reads layer 0x21600 of the zone file: the target zone, the polygon count and per polygon its points. Null when
    ///     the zone has none, which is every zone except Coral Forest, Sertao and a few missions. Throws
    ///     <see cref="InvalidDataException" /> for more polygons than the client takes.
    /// </summary>
    public static ZoneTransferBounds? Read(Zone zone)
    {
        if (zone.Root.Find(WorldLayerIds.TransferBounds) is not GtDataLayer { Data.Length: >= 8 } layer)
        {
            return null;
        }

        ReadOnlySpan<byte> data = layer.Data;
        var targetZoneId = BinaryPrimitives.ReadUInt32LittleEndian(data);
        var count = BinaryPrimitives.ReadInt32LittleEndian(data[4..]);
        if (count is < 0 or > _maxPolygons)
        {
            throw new InvalidDataException($"The transfer bounds have {count} polygons, the client takes {_maxPolygons} at most");
        }

        var offset = 8;
        var polygons = new List<Vector2[]>(count);
        for (var i = 0; i < count; i++)
        {
            var points = new Vector2[BinaryPrimitives.ReadInt32LittleEndian(data[offset..])];
            offset += 4;
            for (var p = 0; p < points.Length; p++)
            {
                points[p] = new Vector2(BinaryPrimitives.ReadSingleLittleEndian(data[offset..]), BinaryPrimitives.ReadSingleLittleEndian(data[(offset + 4)..]));
                offset += 8;
            }

            polygons.Add(points);
        }

        return new ZoneTransferBounds(targetZoneId, polygons);
    }

    /// <summary>
    ///     Whether a position (x and y, the height doesn't matter) is in the area. Even-odd over all polygons, so the small
    ///     ones inside the big one are holes.
    /// </summary>
    public bool Contains(Vector2 point)
    {
        var inside = false;
        foreach (var polygon in Polygons)
        {
            for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
            {
                var a = polygon[i];
                var b = polygon[j];
                if ((a.Y > point.Y) != (b.Y > point.Y) && point.X < ((b.X - a.X) * (point.Y - a.Y) / (b.Y - a.Y)) + a.X)
                {
                    inside = !inside;
                }
            }
        }

        return inside;
    }
}
