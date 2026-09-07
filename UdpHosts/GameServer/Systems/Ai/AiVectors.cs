using System;
using System.Numerics;

namespace GameServer.Systems.Ai;

/// <summary>
///     Small pure helpers shared by the AI engine. Kept separate so the maths can be
///     asserted on directly instead of only through a running shard.
/// </summary>
public static class AiVectors
{
    /// <summary>Distance between two points ignoring Z, which is what all the AI ranges mean.</summary>
    public static float HorizontalDistance(Vector3 a, Vector3 b)
    {
        float dx = a.X - b.X;
        float dy = a.Y - b.Y;
        return MathF.Sqrt((dx * dx) + (dy * dy));
    }

    /// <summary>
    ///     Builds the orientation a character must have to face along <paramref name="forward" />.
    /// </summary>
    /// <remarks>
    ///     A character's orientation is a yaw-only rotation about the world +Z (up) axis. The
    ///     model's local frame is +X right, +Y forward, +Z up (see
    ///     <c>CharacterEntity.CalculateProjectileOrigin</c>, whose muzzle offset is
    ///     (0.2, 0, 1.62)), and the facing direction is
    ///     <c>QuaternionEx.Transform(new Vector3(0, 1, 0), QuaternionEx.Inverse(Orientation))</c>.
    ///     So to face a horizontal <paramref name="forward" />, the inverse orientation must
    ///     rotate local +Y onto it, which is a yaw-only rotation by
    ///     <c>Atan2(forward.X, forward.Y)</c>; the orientation itself is that rotation.
    /// </remarks>
    public static Quaternion OrientationFacing(Vector3 forward)
    {
        var flat = new Vector3(forward.X, forward.Y, 0f);
        if (flat.LengthSquared() < 0.0001f)
        {
            return Quaternion.Identity;
        }

        flat = Vector3.Normalize(flat);
        float yaw = MathF.Atan2(flat.X, flat.Y);
        return Quaternion.CreateFromAxisAngle(Vector3.UnitZ, yaw);
    }
}
