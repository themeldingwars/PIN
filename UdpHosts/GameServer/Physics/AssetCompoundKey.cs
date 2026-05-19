using System;
using System.Numerics;

namespace GameServer.Physics;

public struct AssetCompoundKey : IEquatable<AssetCompoundKey>
{
    public uint AssetId;
    public float Scale;
    public Vector3 Offset;

    public AssetCompoundKey(uint assetId, Vector3 offset, float scale)
    {
        AssetId = assetId;
        Scale = scale;
        Offset = offset;
    }

    public readonly bool Equals(AssetCompoundKey other)
    {
        return AssetId == other.AssetId &&
               BitConverter.SingleToInt32Bits(Scale) == BitConverter.SingleToInt32Bits(other.Scale) &&
               Offset == other.Offset;
    }

    public override readonly bool Equals(object obj)
    {
        return obj is AssetCompoundKey other && Equals(other);
    }

    public override readonly int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = (hash * 31) + AssetId.GetHashCode();
            hash = (hash * 31) + BitConverter.SingleToInt32Bits(Scale);
            return hash;
        }
    }
}