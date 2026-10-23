using System.Numerics;
using BepuUtilities;
using GameServer.Entities;
using GameServer.Entities.Character;

namespace GameServer.Systems.Aptitude;

/// <summary>
///     Shared pose handling for the client's volume targeting commands. TargetConeAECommand and
///     TargetPBAECommand both resolve their query origin through the caster's pose buffer the same way,
///     so the rules live here once: which pose slot to read (body or aim/eye), whether to read the
///     context's snapshot from when it was created or the caster's live pose, and how the SDB's aim
///     position offset is applied along the pose basis.
/// </summary>
internal static class QueryPose
{
    internal const float _epsilon = 0.001f;
    private const float _bodyPositionZOffset = 0.1f;

    /// <summary>
    ///     Reads the caster's orientation as the (right, forward, up) basis the client takes from its pose
    ///     matrix. Always live: the client reads the basis from the pose matrix regardless of UseInitPos.
    /// </summary>
    /// <param name="self">The caster</param>
    /// <returns>The caster's pose basis</returns>
    public static QueryBasis BuildBasis(IAptitudeTarget self)
    {
        var orientation = self is BaseEntity entity ? entity.Orientation : Quaternion.Identity;
        var inverseOrientation = QuaternionEx.Inverse(orientation);

        return new QueryBasis(
            QuaternionEx.Transform(Vector3.UnitX, inverseOrientation),
            QuaternionEx.Transform(Vector3.UnitY, inverseOrientation),
            QuaternionEx.Transform(Vector3.UnitZ, inverseOrientation));
    }

    /// <summary>
    ///     The direction the caster is aiming, which is what the client's pose buffer vtable +0x18 answers.
    ///     Anything without a reported aim, non characters included, falls back to the body facing.
    /// </summary>
    /// <param name="context">The executing context</param>
    /// <param name="useInitPos">Whether to use the context's snapshot instead of the live pose</param>
    /// <param name="bodyForward">The fallback direction when the caster reports no aim</param>
    /// <returns>The aim direction</returns>
    public static Vector3 ResolveAimDirection(Context context, byte useInitPos, Vector3 bodyForward)
    {
        Vector3 aimDirection = useInitPos == 1 ? context.InitAimDirection : LiveAimDirection(context);
        return aimDirection.LengthSquared() <= _epsilon ? bodyForward : aimDirection;
    }

    /// <summary>
    ///     Resolves where a volume query is centred on the caster. UseInitPos swaps every pose read over to
    ///     the snapshot the context took when it was created, and UseBodyPosition picks which of the client's
    ///     two pose slots is used - the body one (pose buffer vtable +0x04) or the aim/eye one (+0x14).
    ///     Only the body slot gets the client's world Z nudge.
    /// </summary>
    /// <param name="context">The executing context</param>
    /// <param name="basis">The caster's pose basis, which the position offset is applied along</param>
    /// <param name="aimDirection">The aim direction the aim/eye origin is offset along</param>
    /// <param name="useInitPos">Whether to use the context's snapshot instead of the live pose</param>
    /// <param name="useBodyPosition">Whether to use the body pose slot instead of the aim/eye one</param>
    /// <param name="aimPosOffset">The SDB's position offset, in the caster's local space</param>
    /// <returns>The query origin in world space</returns>
    public static Vector3 BuildOrigin(Context context, QueryBasis basis, Vector3 aimDirection, byte useInitPos, byte useBodyPosition, Vector3 aimPosOffset)
    {
        Vector3 origin = useBodyPosition == 1
            ? (useInitPos == 1 ? context.InitBodyPosition : LiveBodyPosition(context))
            : (useInitPos == 1 ? context.InitAimOrigin : LiveAimOrigin(context, aimDirection));

        if (useBodyPosition == 1)
        {
            origin += Vector3.UnitZ * _bodyPositionZOffset;
        }

        return origin + basis.Offset(aimPosOffset);
    }

    /// <summary>
    ///     Turns a direction offset written in the caster's local space into a normalized world space axis,
    ///     measured along the forward direction the command selected.
    /// </summary>
    /// <param name="basis">The caster's pose basis</param>
    /// <param name="axisForward">The direction the local Y component is measured along</param>
    /// <param name="aimDirOffset">The SDB's direction offset, in the caster's local space</param>
    /// <param name="direction">The normalized axis, when it is not degenerate</param>
    /// <returns>False when the resulting axis is too short to query with</returns>
    public static bool TryBuildDirection(QueryBasis basis, Vector3 axisForward, Vector3 aimDirOffset, out Vector3 direction)
    {
        direction = basis.OffsetAlong(aimDirOffset, axisForward);
        if (direction.LengthSquared() <= _epsilon)
        {
            return false;
        }

        direction = Vector3.Normalize(direction);
        return true;
    }

    /// <summary>
    ///     The caster's body position evaluated now, matching the client's pose buffer vtable +0x04.
    /// </summary>
    /// <param name="context">The executing context</param>
    /// <returns>The body position at the context's execution time</returns>
    internal static Vector3 LiveBodyPosition(Context context)
    {
        if (context.Self is CharacterEntity character)
        {
            character.TryGetInterpolatedPose(context.CurrentTime, null, out var position, out _, out _);
            return position;
        }

        return context.Self.Position;
    }

    /// <summary>
    ///     The caster's aim/eye position evaluated now, matching the client's pose buffer vtable +0x14.
    /// </summary>
    /// <param name="context">The executing context</param>
    /// <param name="aimDirection">The aim direction the origin is offset along</param>
    /// <returns>The aim/eye position at the context's execution time</returns>
    private static Vector3 LiveAimOrigin(Context context, Vector3 aimDirection)
    {
        return context.Self is CharacterEntity character
            ? character.GetProjectileOrigin(context.CurrentTime, aimDirection)
            : context.Self.Position;
    }

    /// <summary>
    ///     The caster's live aim direction, what the client's pose buffer vtable +0x18 answers when the
    ///     context has no snapshot to use.
    /// </summary>
    /// <param name="context">The executing context</param>
    /// <returns>The aim direction, or a zero vector when the caster has none</returns>
    private static Vector3 LiveAimDirection(Context context)
    {
        return context.Self is CharacterEntity character ? character.AimDirection : Vector3.Zero;
    }
}
