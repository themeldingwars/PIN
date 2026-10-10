using System.Numerics;
using FauFau.Formats;
using Shared.Collision.Tagfile.Models;

namespace Shared.Collision.Tagfile;

public static class EnwfToBepuConverter
{
    public static VertBlockContent[] ConvertVertBlocks(Vector3[][] vertBlocks)
    {
        var result = new VertBlockContent[vertBlocks.Length];
        for (int i = 0; i < vertBlocks.Length; i++)
        {
            result[i] = new VertBlockContent { Verts = vertBlocks[i] };
        }

        return result;
    }

    public static IndiceBlockContent[] ConvertIndiceBlocks(EnwfLayer.IndiceBlock[] indiceBlocks)
    {
        var result = new IndiceBlockContent[indiceBlocks.Length];
        for (int i = 0; i < indiceBlocks.Length; i++)
        {
            var block = indiceBlocks[i];
            var indices = new uint[block.TriangleCount][];
            for (int j = 0; j < indices.Length; j++)
            {
                indices[j] = [block.Indices[j * 3], block.Indices[(j * 3) + 1], block.Indices[(j * 3) + 2]];
            }

            result[i] = new IndiceBlockContent { Indices = indices };
        }

        return result;
    }
}
