using System.Numerics;

namespace Shared.Collision.ZoneLoading;

/// <summary>
///     Where a zone's positions are measured from: the spot of its origin on a face of the world's cube, in meters. Coral
///     Forest and Sertao share terrain but not their frames, the same spot has an x 2816 higher and a y 256 lower in
///     Sertao than in Coral Forest.
/// </summary>
public readonly record struct ZoneFrame(uint CubeFace, Vector2 Origin)
{
    /// <summary>The same spot in another zone's frame, null when the zones aren't on the same cube face. Height stays.</summary>
    public Vector3? Convert(Vector3 position, ZoneFrame to)
    {
        if (to.CubeFace != CubeFace)
        {
            return null;
        }

        var offset = Origin - to.Origin;
        return position with { X = position.X + offset.X, Y = position.Y + offset.Y };
    }
}
