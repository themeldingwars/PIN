using System.Numerics;

namespace Shared.Collision.Navigation;

/// <summary>
///     A world-space triangle extracted from a static collision shape,
///     with a precomputed AABB used for navmesh tile filtering.
/// </summary>
public readonly struct NavTriangle
{
    public readonly Vector3 A;
    public readonly Vector3 B;
    public readonly Vector3 C;
    public readonly Vector3 Min;
    public readonly Vector3 Max;

    public NavTriangle(Vector3 a, Vector3 b, Vector3 c)
    {
        A = a;
        B = b;
        C = c;
        Min = Vector3.Min(Vector3.Min(a, b), c);
        Max = Vector3.Max(Vector3.Max(a, b), c);
    }

    public readonly bool OverlapsXY(Vector3 min, Vector3 max)
    {
        return Min.X <= max.X && Max.X >= min.X && Min.Y <= max.Y && Max.Y >= min.Y;
    }
}
