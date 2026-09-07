using System;
using System.Collections.Generic;
using System.Numerics;
using GameServer.Entities;
using GameServer.Entities.Character;
using GameServer.Entities.Deployable;
using GameServer.Entities.Vehicle;
using GameServer.Enums;
using GameServer.StaticDB.Records.apt;

namespace GameServer.Systems.Aptitude.Commands.Target;

/// <summary>
///     Acquires every entity standing inside a cone in front of the entity running the
///     chain and appends them to the current target list, the same way
///     <see cref="TargetPBAECommand" /> does for a sphere.
/// </summary>
/// <remarks>
///     <para>
///     The cone starts at the chain runner (or at the initiation position when
///     <c>UseInitPos</c> is set) and points along its aim direction, or along its body
///     facing when <c>UseBodyOrient</c> is set. <c>Angle</c> is read as the full opening
///     angle in degrees, so the half angle used for the test is <c>Angle / 2</c>.
///     </para>
///     <para>
///     Three record fields shape the volume beyond the plain cone, and they are all
///     applied as a lateral (perpendicular to the axis) allowance at the candidate's
///     distance:
///     </para>
///     <list type="bullet">
///         <item><c>MinRadius</c> is a point-blank bubble around the apex; anything closer
///         than this is hit regardless of the angle, which is what keeps melee-style cones
///         from missing a target standing on top of the caster.</item>
///         <item><c>AimRadiusBias</c> widens the cone by a flat radius, so a narrow cone
///         still has some width right in front of the apex.</item>
///         <item><c>MaxRadius</c> caps how wide the cone is allowed to get at its far end
///         (ignored when it is zero).</item>
///     </list>
///     <para>
///     <c>IgnorePastEndpoints</c> switches the far end of the cone from a spherical cap
///     (straight line distance from the apex) to a flat cap (distance measured along the
///     axis). <c>IgnoreWalls</c> turns off the line of sight probe that otherwise drops
///     targets standing behind static geometry.
///     </para>
/// </remarks>
public class TargetConeAECommand : Command, ICommand
{
    /// <summary>Torso height of the line of sight probe, matching the one the AI engine uses.</summary>
    private const float EyeHeight = 1.4f;

    /// <summary>Half angles at or above this are treated as unbounded, since their tangent explodes.</summary>
    private const float MaxUsableHalfAngle = 89f * (MathF.PI / 180f);

    private const float Epsilon = 0.0001f;

    private TargetConeAECommandDef Params;

    public TargetConeAECommand(TargetConeAECommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        context.FormerTargets = new AptitudeTargets(context.Targets);

        // Unused:
        // Params.AimDirOffset / Params.AimPosOffset - StaticDB vectors, not converted to world space yet
        // Params.ScaleOffset / Params.ScaleQuerySize - client side scaling of the query volume
        // Params.UseBodyPosition - we only ever have the entity position, not a body/root bone position
        if (Params.Filter == 1)
        {
            // Same open question as TargetPBAE: probably filters the existing targets instead of adding to them
            Logger.Debug("{Command} {CommandId} has Filter set to 1, investigate what to do", nameof(TargetConeAECommand), Params.Id);
        }

        if (Params.IncludeInteractives == 1)
        {
            Logger.Debug("{Command} {CommandId} has IncludeInteractives set to 1, investigate what to do", nameof(TargetConeAECommand), Params.Id);
        }

        float range = AbilitySystem.RegistryOp(context.Register, Params.Range, (Operand)Params.RangeRegop);
        float minRadius = AbilitySystem.RegistryOp(context.Register, Params.MinRadius, (Operand)Params.RadiusRegop);
        float maxRadius = AbilitySystem.RegistryOp(context.Register, Params.MaxRadius, (Operand)Params.RadiusRegop);
        float halfAngle = GetHalfAngleRadians(Params.Angle);

        if (range <= 0f)
        {
            Logger.Debug("{Command} {CommandId} has a range of {Range}, no target can be acquired", nameof(TargetConeAECommand), Params.Id, range);
            return CheckMinTargets(context);
        }

        Vector3 origin = Params.UseInitPos == 1 ? context.InitPosition : context.Self.Position;
        if (!TryGetForward(context.Self, Params.UseBodyOrient == 1, out Vector3 forward))
        {
            Logger.Warning("{Command} {CommandId} could not resolve a facing for {Self}", nameof(TargetConeAECommand), Params.Id, context.Self);
            return CheckMinTargets(context);
        }

        var matches = new List<(BaseAptitudeEntity Entity, float Angle, float Distance)>();

        foreach (var pair in context.Shard.Entities)
        {
            if (pair.Value is not BaseAptitudeEntity candidate || candidate == context.Self)
            {
                continue;
            }

            if (!IsInsideCone(origin, forward, range, minRadius, maxRadius, halfAngle, candidate.Position, out float angle, out float distance))
            {
                continue;
            }

            if (Params.IgnoreWalls != 1 && !HasLineOfSight(context, origin, candidate))
            {
                continue;
            }

            matches.Add((candidate, angle, distance));
        }

        SortMatches(matches);
        TrimToMaxTargets(context, matches);

        foreach (var match in matches)
        {
            context.Targets.Push(match.Entity);
        }

        Logger.Debug(
            "{Command} {CommandId} acquired {Count} target(s) in a {Angle} degree cone of {Range}m",
            nameof(TargetConeAECommand),
            Params.Id,
            matches.Count,
            Params.Angle,
            range);

        return CheckMinTargets(context);
    }

    /// <summary>
    ///     Reads the record's full opening angle (degrees) as the half angle (radians) of the
    ///     test. A missing or nonsensical angle degrades to a hemisphere rather than to a
    ///     degenerate cone that can never hit anything.
    /// </summary>
    private static float GetHalfAngleRadians(float angle)
    {
        if (angle <= 0f || angle > 360f)
        {
            return MathF.PI / 2f;
        }

        return angle * 0.5f * (MathF.PI / 180f);
    }

    /// <summary>
    ///     Resolves the direction the cone points at: the entity's aim direction, or its body
    ///     facing when the record asks for it or when the entity has no aim direction at all.
    /// </summary>
    private static bool TryGetForward(IAptitudeTarget source, bool useBodyOrient, out Vector3 forward)
    {
        Vector3 candidate = Vector3.Zero;

        if (!useBodyOrient)
        {
            candidate = source switch
            {
                CharacterEntity character => character.AimDirection,
                DeployableEntity deployable => deployable.AimDirection,
                VehicleEntity vehicle => vehicle.AimDirection,
                _ => Vector3.Zero
            };
        }

        if (candidate.LengthSquared() < Epsilon && source is BaseEntity entity)
        {
            // The model's local frame is +X right, +Y forward, +Z up, transformed by the
            // inverse orientation - see AiVectors.OrientationFacing
            candidate = Vector3.Transform(new Vector3(0f, 1f, 0f), Quaternion.Conjugate(entity.Orientation));
        }

        if (candidate.LengthSquared() < Epsilon)
        {
            forward = Vector3.Zero;
            return false;
        }

        forward = Vector3.Normalize(candidate);
        return true;
    }

    private bool IsInsideCone(
        Vector3 origin,
        Vector3 forward,
        float range,
        float minRadius,
        float maxRadius,
        float halfAngle,
        Vector3 position,
        out float angle,
        out float distance)
    {
        Vector3 offset = position - origin;
        distance = offset.Length();
        angle = 0f;

        if (distance < Epsilon)
        {
            // Standing exactly on the apex, always inside
            return true;
        }

        if (minRadius > 0f && distance <= minRadius)
        {
            // Point-blank bubble around the apex, hit regardless of the angle
            return true;
        }

        float axial = Vector3.Dot(offset, forward);
        if (axial <= 0f)
        {
            // Behind the apex
            return false;
        }

        bool withinRange = Params.IgnorePastEndpoints == 1 ? axial <= range : distance <= range;
        if (!withinRange)
        {
            return false;
        }

        angle = MathF.Acos(Math.Clamp(axial / distance, -1f, 1f));

        float allowedRadius = halfAngle >= MaxUsableHalfAngle ? float.PositiveInfinity : axial * MathF.Tan(halfAngle);
        allowedRadius += MathF.Max(Params.AimRadiusBias, 0f);

        if (maxRadius > 0f)
        {
            allowedRadius = MathF.Min(allowedRadius, maxRadius);
        }

        float lateral = MathF.Sqrt(MathF.Max((distance * distance) - (axial * axial), 0f));

        return lateral <= allowedRadius;
    }

    private bool HasLineOfSight(Context context, Vector3 origin, BaseAptitudeEntity candidate)
    {
        var physics = context.Shard.Physics;
        if (physics == null)
        {
            // No collision data loaded, so nothing can occlude
            return true;
        }

        Vector3 from = origin + new Vector3(0f, 0f, EyeHeight);
        Vector3 to = candidate.Position + new Vector3(0f, 0f, EyeHeight);
        var hit = physics.SegmentRayCast(from, to, context.Self.EntityId);

        return !hit.Hit || hit.HitEntityId == candidate.EntityId;
    }

    /// <summary>
    ///     Orders the acquired targets so that trimming to <c>MaxTargets</c> keeps the ones
    ///     the caster most likely aimed at: closest to the cone axis when <c>SortByAngle</c>
    ///     is set, closest to the apex otherwise.
    /// </summary>
    private void SortMatches(List<(BaseAptitudeEntity Entity, float Angle, float Distance)> matches)
    {
        if (Params.SortByAngle == 1)
        {
            matches.Sort(static (a, b) =>
            {
                int byAngle = a.Angle.CompareTo(b.Angle);
                return byAngle != 0 ? byAngle : a.Distance.CompareTo(b.Distance);
            });
        }
        else
        {
            matches.Sort(static (a, b) =>
            {
                int byDistance = a.Distance.CompareTo(b.Distance);
                return byDistance != 0 ? byDistance : a.Angle.CompareTo(b.Angle);
            });
        }
    }

    private void TrimToMaxTargets(Context context, List<(BaseAptitudeEntity Entity, float Angle, float Distance)> matches)
    {
        if (Params.MaxTargets == 0)
        {
            return;
        }

        int room = Params.MaxTargets - context.Targets.Count;
        if (room < 0)
        {
            Logger.Warning(
                "{Command} {CommandId} already had {Count} target(s) before it ran, more than its MaxTargets of {MaxTargets}",
                nameof(TargetConeAECommand),
                Params.Id,
                context.Targets.Count,
                Params.MaxTargets);
            room = 0;
        }

        if (matches.Count > room)
        {
            matches.RemoveRange(room, matches.Count - room);
        }
    }

    private bool CheckMinTargets(Context context)
    {
        if (context.Targets.Count < Params.MinTargets)
        {
            Logger.Debug(
                "{Command} {CommandId} acquired {Count} target(s), below its MinTargets of {MinTargets}, returning false",
                nameof(TargetConeAECommand),
                Params.Id,
                context.Targets.Count,
                Params.MinTargets);
            return false;
        }

        return true;
    }
}
