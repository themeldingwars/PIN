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

            long minCoordX = range.MinX;
            long maxCoordX = range.MaxX;
            long minCoordY = range.MinY;
            long maxCoordY = range.MaxY;

            double centerIndexX = (maxCoordX - minCoordX) / 2.0;
            double centerIndexY = (maxCoordY - minCoordY) / 2.0;

            if (zoneId == _coralForestZoneId)
            {
                centerIndexX = 4;
                centerIndexY = 3.5;
            }
            else if (zoneId == _sertaoZoneId)
            {
                centerIndexX = 9.5;
                centerIndexY = 3;
            }

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
