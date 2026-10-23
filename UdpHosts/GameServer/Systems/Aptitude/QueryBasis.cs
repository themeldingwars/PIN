using System.Numerics;

namespace GameServer.Systems.Aptitude;

/// <summary>
///     The caster's orientation expressed as the (right, forward, up) basis the client reads out of its
///     pose matrix. The SDB's aim offsets are written in this space, so every volume command applies them
///     the same way.
/// </summary>
internal readonly struct QueryBasis
{
    public QueryBasis(Vector3 right, Vector3 forward, Vector3 up)
    {
        Right = right;
        Forward = forward;
        Up = up;
    }

    public Vector3 Right { get; }

    public Vector3 Forward { get; }

    public Vector3 Up { get; }

    /// <summary>
    ///     Places a locally offset point into world space, e.g. the SDB's aim position offset.
    /// </summary>
    /// <param name="localOffset">The offset in the caster's (right, forward, up) space</param>
    /// <returns>The offset as a world space vector</returns>
    public Vector3 Offset(Vector3 localOffset)
    {
        return (Right * localOffset.X) + (Forward * localOffset.Y) + (Up * localOffset.Z);
    }

    /// <summary>
    ///     Places a locally offset direction into world space, but with the forward component measured
    ///     along a chosen direction instead of the body's own forward axis. This is how the client combines
    ///     an aim direction offset with whichever forward the command selected, body facing or aim.
    /// </summary>
    /// <param name="localOffset">The offset in the caster's (right, forward, up) space</param>
    /// <param name="forward">The direction the local Y component is measured along</param>
    /// <returns>The offset as a world space vector</returns>
    public Vector3 OffsetAlong(Vector3 localOffset, Vector3 forward)
    {
        return (Right * localOffset.X) + (forward * localOffset.Y) + (Up * localOffset.Z);
    }
}
