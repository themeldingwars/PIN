using System.Numerics;
using FauFau.Formats;

namespace Shared.Collision.ZoneLoading;

public static class ChunkOriginCalculator
{
    private const float _chunkSize = 512f;
    private const uint _coralForestZoneId = 448;
    private const uint _sertaoZoneId = 1030;

    public static ZoneChunkRef[] ExtractChunks(Zone zone, uint zoneId)
    {
        var chunkRefs = new List<ZoneChunkRef>();

        foreach (var chunkInfo in zone.Root.FindAll(WorldLayerIds.ChunkInfo).OfType<GtContainerLayer>())
        {
            var range = chunkInfo.Find<ZoneChunkRangeLayer>();
            if (range == null)
            {
                continue;
            }

            var refs = chunkInfo.FindAll<ZoneChunkRefLayer>().Where(l => l.Id == WorldLayerIds.ChunkRef);
            var refs2 = chunkInfo.FindAll<ZoneChunkRefLayer>().Where(l => l.Id == WorldLayerIds.ChunkRef2);

            long maxCoordX = range.MaxX;
            long maxCoordY = range.MaxY;
            var (centerIndexX, centerIndexY) = CenterIndex(range, zoneId);

            // The first range decides the cube face and the origins of every reference
            foreach (var reference in refs.Concat(refs2))
            {
                var origin = CalculateOrigin(maxCoordX, maxCoordY, centerIndexX, centerIndexY, reference.X, reference.Y);
                string chunkName = $"{range.CubeFace}_{reference.X:D4}_{reference.Y:D4}";
                chunkRefs.Add(new ZoneChunkRef { Name = chunkName, Origin = origin });
            }
        }

        return [.. chunkRefs];
    }

    /// <summary>
    ///     Where the zone's frame sits on its cube face, null for a zone without chunks. Zones that share chunks, Coral
    ///     Forest and Sertao, have different frames, positions convert through <see cref="ZoneFrame.Convert" />.
    /// </summary>
    public static ZoneFrame? Frame(Zone zone, uint zoneId)
    {
        var range = zone.Root.FindAll(WorldLayerIds.ChunkInfo).OfType<GtContainerLayer>()
                        .Select(chunkInfo => chunkInfo.Find<ZoneChunkRangeLayer>())
                        .FirstOrDefault(found => found != null);
        if (range == null)
        {
            return null;
        }

        // A chunk's origin is (x - max + center) * 512, so the frame's origin is chunk (max - center) of the face
        var (centerIndexX, centerIndexY) = CenterIndex(range, zoneId);
        return new ZoneFrame(range.CubeFace, new Vector2((float)((range.MaxX - centerIndexX) * _chunkSize), (float)((range.MaxY - centerIndexY) * _chunkSize)));
    }

    private static (double X, double Y) CenterIndex(ZoneChunkRangeLayer range, uint zoneId)
    {
        return zoneId switch
        {
            _coralForestZoneId => (4, 3.5),
            _sertaoZoneId => (9.5, 3),
            _ => ((range.MaxX - range.MinX) / 2.0, (range.MaxY - range.MinY) / 2.0),
        };
    }

    private static Vector3 CalculateOrigin(long maxCoordX, long maxCoordY, double centerIndexX, double centerIndexY, long x, long y)
    {
        double coordIndexX = maxCoordX - x;
        double coordIndexY = maxCoordY - y;

        double coordMultiX = centerIndexX - coordIndexX;
        double coordMultiY = centerIndexY - coordIndexY;

        int originX = (int)(coordMultiX * _chunkSize);
        int originY = (int)(coordMultiY * _chunkSize);

        return new Vector3(originX, originY, 0);
    }
}
