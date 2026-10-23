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
///     Targets everything whose collision volume touches a sphere around the caster.
/// </summary>
public class TargetPBAECommand : Command, ICommand
{
    private const float _maxRadius = 215.001f;
    private const ulong _noEntity = ulong.MaxValue;

    private TargetPBAECommandDef Params;

    public TargetPBAECommand(TargetPBAECommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        context.FormerTargets = new AptitudeTargets(context.Targets);

        // Unused:
        // Params.IgnoreWalls
        // Params.ScaleOffset
        float radius = AbilitySystem.RegistryOp(context.Register, Params.Radius, (Operand)Params.RadiusRegop);

        if (Params.UseWeaponRadius == 1)
        {
            radius *= context.ScaleMultiplier;

            if (context.ScaleMultiplier == 0f)
            {
                Logger.Warning("{Command} {CommandId} has UseWeaponRadius set but the context carries no size multiplier, leaving the query radius at 0", nameof(TargetPBAECommand), Params.Id);
            }
        }

        if (Params.ScaleQuerySize == 1)
        {
            Logger.Debug("{Command} {CommandId} has ScaleQuerySize set, leaving the radius unscaled by the caster's physics scale", nameof(TargetPBAECommand), Params.Id);
        }

        radius = Math.Min(radius, _maxRadius);

        if (Params.MaxTargets != 0 && context.Targets.Count >= Params.MaxTargets)
        {
            Logger.Warning("The context target count {contextTargetCount} exceeds the MaxTargets {paramMaxTargets} BEFORE command executes", context.Targets.Count, Params.MaxTargets);
        }

        // The client resolves the origin before it looks at Filter, so both modes measure from the same point.
        QueryBasis basis = QueryPose.BuildBasis(context.Self);
        Vector3 aimDirection = QueryPose.ResolveAimDirection(context, Params.UseInitPos, basis.Forward);
        Vector3 origin = QueryPose.BuildOrigin(context, basis, aimDirection, Params.UseInitPos, Params.UseBodyPosition, SDBUtils.Vector3FromFauFau(Params.AimPosOffset));

        var hits = new List<IAptitudeTarget>();

        if (Params.IncludeInteractives == 1 && context.Self is CharacterEntity { InteractionTarget: BaseAptitudeEntity interactive })
        {
            // IncludeInteractives is a collision layer input for the client's query. The server query has no
            // layer mask yet, so the entity the caster is interacting with is added directly, which keeps
            // interaction duration chains holding their target while the interaction is active.
            if (!hits.Contains(interactive))
            {
                hits.Add(interactive);
            }
        }

        if (Params.Filter == 1)
        {
            // Only 5 of the 2207 defs set this. The client's filter branch skips the volume query entirely,
            // re-tests each target the context already has against the radius, and then clears the target
            // list either way. Where the surviving targets are meant to go is not clear from the code, so
            // it stays a note rather than a guess.
            Logger.Debug("{Command} {CommandId} has Filter set to 1, investigate what to do", nameof(TargetPBAECommand), Params.Id);
        }
        else if (radius > QueryPose._epsilon)
        {
            ulong ignoreEntityId = Params.IncludeSelf == 1 ? _noEntity : context.Self.EntityId;

            foreach (var hitEntityId in context.Shard.Physics.QueryEntityIdsInSphere(origin, radius, ignoreEntityId))
            {
                if (context.Shard.Entities.TryGetValue(hitEntityId, out var entity) && entity is BaseAptitudeEntity candidate && !hits.Contains(candidate))
                {
                    hits.Add(candidate);
                }
            }

            Logger.Debug("{Command} {CommandId} sphere query origin={Origin} radius={Radius} found {hitCount} entities register={Register}",
                nameof(TargetPBAECommand),
                Params.Id,
                origin,
                radius,
                hits.Count,
                context.Register);
        }

        if (hits.Count == 0)
        {
            if (Params.MinTargets != 0)
            {
                Logger.Debug("{Command} {CommandId} found no targets and MinTargets is set, returning false", nameof(TargetPBAECommand), Params.Id);
                result.SetFail(StatusCode.Status3_TargetingFail);
                return;
            }

            result.SetPass();
            return;
        }

        // The client caps at MaxTargets, and when it has to truncate it sorts the hits first so the ones it
        // keeps are the closest. The sort key is a float on the hit record; distance from the query origin is
        // the only ordering that fits a sphere query.
        if (Params.MaxTargets != 0 && hits.Count > Params.MaxTargets)
        {
            hits = hits
                .OrderBy((hit) => Vector3.DistanceSquared(origin, hit.Position))
                .ToList()
                .GetRange(0, Params.MaxTargets);
        }

        // The client skips a hit that is already in the context's targets rather than adding it twice, which
        // matters here because a second entry would apply the chain's effect to that target twice.
        foreach (var hit in hits)
        {
            if (!context.Targets.Contains(hit))
            {
                context.Targets.Push(hit);
            }
        }

        result.SetPass();
        return;
    }

    public override void Reset(Context context)
    {
        return;
    }
}
