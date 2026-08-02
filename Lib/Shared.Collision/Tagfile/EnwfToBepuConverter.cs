using System.Numerics;
using Shared.Collision.Layers;
using Shared.Collision.Tagfile.Models;

namespace Shared.Collision.Tagfile;

public static class EnwfToBepuConverter
{
    public static VertBlockContent[] ConvertVertBlocks(EnwfVertBlock[] vertBlocks)
    {
        var result = new VertBlockContent[vertBlocks.Length];
        for (int i = 0; i < vertBlocks.Length; i++)
        {
            var src = vertBlocks[i].Vertices;
            var verts = new Vector3[src.Length];
            for (int j = 0; j < src.Length; j++)
            {
                verts[j] = new Vector3(src[j].X, src[j].Y, src[j].Z);
            }

            result[i] = new VertBlockContent { Verts = verts };
        }

        return result;
    }

    public static IndiceBlockContent[] ConvertIndiceBlocks(EnwfIndiceBlock[] indiceBlocks)
    {
        var result = new IndiceBlockContent[indiceBlocks.Length];
        for (int i = 0; i < indiceBlocks.Length; i++)
        {
            var src = indiceBlocks[i].Indices;
            var indices = new uint[src.Length][];
            for (int j = 0; j < src.Length; j++)
            {
                indices[j] = [src[j].A, src[j].B, src[j].C];
            }

            result[i] = new IndiceBlockContent { Indices = indices };
        }

        return result;
    }
}
