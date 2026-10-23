using GameServer.Entities;
using GameServer.StaticDB;
using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Target;

public class TargetByDamageResponseCommand : Command, ICommand
{
    private const uint _any = 1;

    private TargetByDamageResponseCommandDef Params;

    public TargetByDamageResponseCommand(TargetByDamageResponseCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        if (context.InitTime is null)
        {
            // The client refuses with status 1 when the context has not been initiated.
            Logger.Debug("{Command} {CommandId} fails because the context isn't initiated, returning false", nameof(TargetByDamageResponseCommand), Params.Id);
            result.SetFail(StatusCode.Status1);
            return;
        }

        byte damageTypeId = Params.VulnerableDamagetypeId;
        if (Params.UseWeaponDamageType == 1)
        {
            damageTypeId = context.DamageType;
            Logger.Debug("{Command} {CommandId} has UseWeaponDamageType set, testing damage type {damageTypeId} from the context", nameof(TargetByDamageResponseCommand), Params.Id, damageTypeId);
        }

        var previousTargets = context.Targets;
        var newTargets = new AptitudeTargets();

        foreach (IAptitudeTarget target in previousTargets)
        {
            if (MatchesDamageResponse(context, target, damageTypeId) != (Params.Negate == 1))
            {
                newTargets.Push(target);
            }
        }

        context.FormerTargets = previousTargets;
        context.Targets = newTargets;

        result.SetPass();
        return;
    }

    public override void Reset(Context context)
    {
        return;
    }

    /// <summary>
    ///     The client's matcher resolves the
    ///     target's game object, takes its damage response off the damageable component, and requires both
    ///     the response id and the vulnerability to the damage type to clear.
    /// </summary>
    /// <param name="context">The executing context</param>
    /// <param name="target">The candidate target</param>
    /// <param name="damageTypeId">The damage type the vulnerability is tested against, already resolved</param>
    /// <returns>True when the target matches the command's damage response</returns>
    private bool MatchesDamageResponse(Context context, IAptitudeTarget target, byte damageTypeId)
    {
        if (!context.Shard.Entities.TryGetValue(target.EntityId, out IEntity entity) || entity == null)
        {
            // The client's GetByGUID lookup failing is a no match too.
            return false;
        }

        if (Params.NotInvulnerable == 1)
        {
            Logger.Debug("{Command} {CommandId} has NotInvulnerable set, which the server has no state for", nameof(TargetByDamageResponseCommand), Params.Id);
        }

        byte damageResponse = entity.DamageResponse;
        if (Params.DamageresponseId != _any && Params.DamageresponseId != damageResponse)
        {
            return false;
        }

        if (damageTypeId != _any)
        {
            float multiplier = SDBInterface.GetDamageResponseDamageType(damageResponse, damageTypeId) ?? 1f;
            if (multiplier <= Params.VulnerableTol)
            {
                return false;
            }
        }

        return true;
    }
}
