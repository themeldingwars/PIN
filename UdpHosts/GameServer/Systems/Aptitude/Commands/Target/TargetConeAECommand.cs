using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using GameServer.Entities;
using GameServer.Entities.Character;
using GameServer.Enums;
using GameServer.StaticDB;
using GameServer.StaticDB.Records.apt;

namespace GameServer.Systems.Aptitude.Commands.Target;

/// <summary>
///     Targets everything inside a cone/frustum swept out in front of the caster.
/// </summary>
public class TargetConeAECommand : Command, ICommand
{
    private TargetConeAECommandDef Params;

    public TargetConeAECommand(TargetConeAECommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        // The client pre-computes sin(Angle) in the constructor and multiplies it by the resolved range,
        // then runs the radius regop over that whole expression.
        float angleSine = MathF.Sin(Params.Angle * (MathF.PI / 180f));
        float range = AbilitySystem.RegistryOp(context.Register, Params.Range, (Operand)Params.RangeRegop);
        float minRadius = AbilitySystem.RegistryOp(context.Register, Params.MinRadius, (Operand)Params.RadiusRegop);
        float maxRadius = AbilitySystem.RegistryOp(context.Register, (angleSine * range) + Params.MaxRadius, (Operand)Params.RadiusRegop);

        if (Params.ScaleQuerySize == 1 || Params.ScaleOffset == 1)
        {
            Logger.Debug("{Command} {CommandId} has ScaleQuerySize/ScaleOffset set, which would scale by the caster's physics scale", nameof(TargetConeAECommand), Params.Id);
        }

        // The client compares the resolved radii against FLT_MAX and bails with status 10; in practice that
        // only ever triggers once a regop overflows to infinity.
        if (float.IsInfinity(minRadius) || float.IsInfinity(maxRadius))
        {
            Logger.Debug("{Command} {CommandId} resolved to an out of range radius, returning false", nameof(TargetConeAECommand), Params.Id);
            result.SetFail(StatusCode.Status10);
            return;
        }

        var hits = new List<IAptitudeTarget>();
        Vector3 origin = context.Self.Position;
        Vector3 direction = Vector3.UnitX;

        if (range > QueryPose._epsilon)
        {
            if (!TryBuildCone(context, out origin, out direction))
            {
                result.SetFail(StatusCode.Status1);
                return;
            }

            Vector3 farPoint = origin + (direction * range);
            Vector3 nearPoint = origin + (direction * (minRadius * Params.AimRadiusBias));

            Logger.Debug("{Command} {CommandId} frustum near={NearPoint} far={FarPoint} range={Range} nearRadius={ParamMinRadius} farRadius={MaxRadius} register={Register} angle={ParamAngle}",
                nameof(TargetConeAECommand),
                Params.Id,
                nearPoint,
                farPoint,
                range,
                minRadius,
                maxRadius,
                context.Register,
                Params.Angle);

            if (Params.IncludeInteractives == 1 && context.Self is CharacterEntity { InteractionTarget: BaseAptitudeEntity interactive })
            {
                // The client feeds this flag to the physics sweep as a collision layer mask. The server has no
                // equivalent, so (as with TargetPBAE) the entity the caster is interacting with is added directly.
                if (!hits.Contains(interactive))
                {
                    hits.Add(interactive);
                }
            }

            if (Params.IgnoreWalls == 1 || Params.IgnorePastEndpoints == 1)
            {
                // Both flags only alter the collision layer mask of the client's physics sweep. The server
                // query has no layer mask yet, so they stay a note rather than changing the result.
                Logger.Debug("{Command} {CommandId} has IgnoreWalls/IgnorePastEndpoints set, which the server query cannot honour", nameof(TargetConeAECommand), Params.Id);
            }

            // The near cap sits at nearPoint rather than the apex, so the sweep only covers what is left of
            // the range once that offset is taken out.
            float queryRange = range - (minRadius * Params.AimRadiusBias);

            foreach (var hitEntityId in context.Shard.Physics.QueryEntityIdsInCone(nearPoint, direction, queryRange, minRadius, maxRadius, context.Self.EntityId))
            {
                if (context.Shard.Entities.TryGetValue(hitEntityId, out var entity) && entity is BaseAptitudeEntity candidate && !hits.Contains(candidate))
                {
                    hits.Add(candidate);
                }
            }

            Logger.Debug("{Command} {CommandId} cone query found {hitCount} entities", nameof(TargetConeAECommand), Params.Id, hits.Count);
        }

        if (hits.Count < Params.MinTargets)
        {
            // The client discards its scratch result list here and leaves the context targets untouched.
            Logger.Debug("{Command} {CommandId} only found {hitCount} targets, below MinTargets {paramMinTargets}, returning false", nameof(TargetConeAECommand), Params.Id, hits.Count, Params.MinTargets);
            result.SetFail(StatusCode.Status3_TargetingFail);
            return;
        }

        if (Params.SortByAngle == 1)
        {
            hits = SortByAngle(hits, origin, direction);
        }

        if (Params.MaxTargets != 0 && hits.Count > Params.MaxTargets)
        {
            hits = hits.GetRange(0, Params.MaxTargets);
        }

        if (hits.Count == 0)
        {
            // No hits: the client clears the context targets only in filter mode, and still passes.
            if (Params.Filter == 1)
            {
                context.Targets.Clear();
            }

            result.SetPass();
            return;
        }

        if (Params.Filter == 1)
        {
            // Filter mode intersects instead of unions: keep the targets the context already had, in the
            // order it had them, dropping anything the cone did not also find.
            var kept = context.Targets.Where(hits.Contains).ToArray();
            context.Targets.Clear();

            foreach (var target in kept)
            {
                context.Targets.Push(target);
            }
        }
        else
        {
            foreach (var hit in hits)
            {
                if (!context.Targets.Contains(hit))
                {
                    context.Targets.Push(hit);
                }
            }
        }

        result.SetPass();
        return;
    }

    public override void Reset(Context context)
    {
        return;
    }

    /// <summary>
    ///     Scores each hit by how far it sits off the cone axis and sorts nearest to the axis first,
    ///     matching the client's angle sort.
    /// </summary>
    /// <param name="hits">The hits to sort</param>
    /// <param name="origin">The cone apex</param>
    /// <param name="direction">The normalized cone axis</param>
    /// <returns>The sorted hits</returns>
    private static List<IAptitudeTarget> SortByAngle(List<IAptitudeTarget> hits, Vector3 origin, Vector3 direction)
    {
        var scored = new List<(IAptitudeTarget Target, float Score)>(hits.Count);

        foreach (var hit in hits)
        {
            Vector3 toTarget = hit.Position - origin;
            float distanceSquared = toTarget.LengthSquared();

            if (distanceSquared <= QueryPose._epsilon)
            {
                // The client skips anything on top of the apex, since its angle would be undefined.
                continue;
            }

            float score = 1f - Vector3.Dot(direction, Vector3.Normalize(toTarget));
            scored.Add((hit, score));
        }

        return scored.OrderBy((entry) => entry.Score).Select((entry) => entry.Target).ToList();
    }

    /// <summary>
    ///     Builds the cone's apex and axis from the caster's pose. The pose rules themselves live in
    ///     <see cref="QueryPose" />, shared with the other volume commands; what is cone specific is the
    ///     axis, which replaces the body's forward basis vector unless UseBodyOrient is set.
    /// </summary>
    /// <param name="context">The executing context</param>
    /// <param name="origin">The cone apex</param>
    /// <param name="direction">The normalized cone axis</param>
    /// <returns>False when the resulting axis is degenerate</returns>
    private bool TryBuildCone(Context context, out Vector3 origin, out Vector3 direction)
    {
        QueryBasis basis = QueryPose.BuildBasis(context.Self);

        // The aim direction is needed before the origin, because the aim/eye pose position is offset along it.
        Vector3 aimDirection = QueryPose.ResolveAimDirection(context, Params.UseInitPos, basis.Forward);

        Vector3 aimPosOffset = SDBUtils.Vector3FromFauFau(Params.AimPosOffset);
        Vector3 aimDirOffset = SDBUtils.Vector3FromFauFau(Params.AimDirOffset);

        origin = QueryPose.BuildOrigin(context, basis, aimDirection, Params.UseInitPos, Params.UseBodyPosition, aimPosOffset);

        Vector3 axisForward = Params.UseBodyOrient == 1 ? basis.Forward : aimDirection;
        if (!QueryPose.TryBuildDirection(basis, axisForward, aimDirOffset, out direction))
        {
            Logger.Debug("{Command} {CommandId} has a degenerate AimDirOffset, returning false", nameof(TargetConeAECommand), Params.Id);
            return false;
        }

        Logger.Debug("{Command} {CommandId} cone origin={Origin} axis={Axis} aim={Aim} bodyForward={Forward} bodyRight={Right} bodyUp={Up} useInitPos={ParamUseInitPos} useBodyOrient={ParamUseBodyOrient} useBodyPosition={ParamUseBodyPosition} aimDirOffset={AimDirOffset} aimPosOffset={AimPosOffset}",
            nameof(TargetConeAECommand),
            Params.Id,
            origin,
            direction,
            aimDirection,
            basis.Forward,
            basis.Right,
            basis.Up,
            Params.UseInitPos,
            Params.UseBodyOrient,
            Params.UseBodyPosition,
            aimDirOffset,
            aimPosOffset);

        return true;
    }
}