using GameServer.Entities;
using GameServer.StaticDB.Records.aptfs;
using GameServer.Systems.Combat;

namespace GameServer.Systems.Aptitude.Commands.Target;

public class TargetHostilesCommand : Command, ICommand
{
    private TargetHostilesCommandDef Params;

    public TargetHostilesCommand(TargetHostilesCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        // The client filters ctx->targets1 in place (it never touches targets2), so the rebuilt list is the
        // only thing that changes here.
        var filtered = new AptitudeTargets();

        foreach (var target in context.Targets)
        {
            if (IsNamedTarget(context, target) || IsHostileTo(context, target))
            {
                filtered.Push(target);
            }
        }

        context.Targets = filtered;

        if (Params.FailNoTargets == 1 && context.Targets.Count == 0)
        {
            result.SetFail(StatusCode.Status3_TargetingFail);
            return;
        }

        result.SetPass();
        return;
    }

    public override void Reset(Context context)
    {
        return;
    }

    private static bool IsHostileTo(Context context, IAptitudeTarget target)
    {
        var stance = context.Shard.Hostility.GetStance(context.Self as IEntity, target as IEntity);
        return stance is HostilityStance.Hostile or HostilityStance.Neutral;
    }

    private bool IsNamedTarget(Context context, IAptitudeTarget target)
    {
        return (target == context.Self && Params.IncludeSelf == 1) ||
            (target == context.Initiator && Params.IncludeInitiator == 1) ||
            (target == context.Self.Owner && Params.IncludeOwner == 1);
    }
}