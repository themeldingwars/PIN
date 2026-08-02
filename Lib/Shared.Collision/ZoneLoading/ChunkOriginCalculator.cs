using System.Numerics;
using Shared.Collision.Layers;

namespace Shared.Collision.ZoneLoading;

public static class ChunkOriginCalculator
{
    private const float _chunkSize = 512f;
    private const uint _coralForestZoneId = 448;
    private const uint _sertaoZoneId = 1030;

    public static ZoneChunkRef[] ExtractChunks(ZoneRootLayer zoneRoot, uint zoneId)
    {
        var chunkRefs = new List<ZoneChunkRef>();

        foreach (var child in zoneRoot.Children)
        {
            if (child is not ZoneChunkInfoLayer chunkInfo)
            {
                continue;
            }

            var rangeLayer = chunkInfo.Children.OfType<ZoneChunkRangeLayer>().FirstOrDefault();
            if (rangeLayer == null)
            {
                continue;
            }

            var refs = chunkInfo.Children.OfType<ZoneChunkRefLayer>().ToList();
            var refs2 = chunkInfo.Children.OfType<ZoneChunkRef2Layer>().ToList();

            long minCoordX = rangeLayer.MinX;
            long maxCoordX = rangeLayer.MaxX;
            long minCoordY = rangeLayer.MinY;
            long maxCoordY = rangeLayer.MaxY;

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

            foreach (var refLayer in refs)
            {
                var origin = CalculateOrigin(maxCoordX, maxCoordY, centerIndexX, centerIndexY, refLayer.X, refLayer.Y);
                string chunkName = $"{rangeLayer.CubeFaceId}_{refLayer.X:D4}_{refLayer.Y:D4}";
                chunkRefs.Add(new ZoneChunkRef { Name = chunkName, Origin = origin });
            }

            foreach (var ref2Layer in refs2)
            {
                var origin = CalculateOrigin(maxCoordX, maxCoordY, centerIndexX, centerIndexY, ref2Layer.X, ref2Layer.Y);
                string chunkName = $"{rangeLayer.CubeFaceId}_{ref2Layer.X:D4}_{ref2Layer.Y:D4}";
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
