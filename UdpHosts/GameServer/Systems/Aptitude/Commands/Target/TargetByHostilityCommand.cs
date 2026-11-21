using GameServer.Entities;
using GameServer.StaticDB.Records.aptfs;
using GameServer.Systems.Combat;

namespace GameServer.Systems.Aptitude.Commands.Target;

public class TargetByHostilityCommand : Command, ICommand
{
    private TargetByHostilityCommandDef Params;

    public TargetByHostilityCommand(TargetByHostilityCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        if (context.Targets.Count == 0 && Params.FailNoTargets == 1)
        {
            result.SetFail(StatusCode.TargetRequirementFailed);
            return;
        }

        var compareFrom = (Params.CompareFromInitiator == 1 ? context.Initiator : context.Self) as IEntity;
        var filtered = new AptitudeTargets();
        foreach (var target in context.Targets)
        {
            if (Keep(context, compareFrom, target))
            {
                filtered.Push(target);
            }
        }

        context.Targets = filtered;

        if (context.Targets.Count == 0 && Params.FailNoTargets == 1)
        {
            result.SetFail(StatusCode.TargetRequirementFailed);
            return;
        }

        result.SetPass();
    }

    public override void Reset(Context context)
    {
        return;
    }

    private bool Keep(Context context, IEntity compareFrom, IAptitudeTarget target)
    {
        if (target is not IEntity entity)
        {
            return true;
        }

        if (!Matches(context.Shard.Hostility.GetStance(compareFrom, entity)))
        {
            return IsIncluded(context, target);
        }

        if (Params.ExcludeMode == 1)
        {
            return true;
        }

        return IsIncluded(context, target) || !IsNamed(context, target);
    }

    private bool Matches(HostilityStance stance)
    {
        switch (Params.FilterType)
        {
            case 0:
                return stance is HostilityStance.Friendly or HostilityStance.Self;
            case 1:
                return stance is HostilityStance.Hostile;
            case 2:
                return stance is HostilityStance.Hostile or HostilityStance.Neutral;
            default:
                Logger.Warning("{Command} {CommandId} has unhandled filter type {FilterType}, keeping every target", nameof(TargetByHostilityCommand), Params.Id, Params.FilterType);
                return true;
        }
    }

    private static bool IsNamed(Context context, IAptitudeTarget target)
    {
        return target == context.Self || target == context.Initiator || target == context.Self.Owner;
    }

    private bool IsIncluded(Context context, IAptitudeTarget target)
    {
        return (target == context.Self && Params.IncludeSelf == 1) ||
            (target == context.Initiator && Params.IncludeInitiator == 1) ||
            (target == context.Self.Owner && Params.IncludeOwner == 1);
    }
}
