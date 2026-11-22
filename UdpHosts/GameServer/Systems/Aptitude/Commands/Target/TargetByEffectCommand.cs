using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Target;

public class TargetByEffectCommand : Command, ICommand
{
    private TargetByEffectCommandDef Params;

    public TargetByEffectCommand(TargetByEffectCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        var previousTargets = context.Targets;

        if (Params.FilterList == 0)
        {
            foreach (IAptitudeTarget target in previousTargets)
            {
                if (HasEffect(target, context))
                {
                    result.SetPass();
                    return;
                }
            }

            SetResult(ref result, false);
            return;
        }

        var newTargets = new AptitudeTargets();
        foreach (IAptitudeTarget target in previousTargets)
        {
            if (HasEffect(target, context) != (Params.Negate == 1))
            {
                newTargets.Push(target);
            }
        }

        context.FormerTargets = previousTargets;
        context.Targets = newTargets;

        SetResult(ref result, newTargets.Count > 0);
    }

    public override void Reset(Context context)
    {
        return;
    }

    private void SetResult(ref CommandResult result, bool anyTarget)
    {
        if (!anyTarget && Params.FailNoTargets == 1)
        {
            result.SetFail(StatusCode.TargetRequirementFailed);
            return;
        }

        result.SetPass();
    }

    private bool HasEffect(IAptitudeTarget target, Context context)
    {
        var first = target.FirstEffect(Params.EffectId);
        if (first == null || target.CountEffect(Params.EffectId) < Params.StackCount)
        {
            return false;
        }

        return Params.SameInitiator != 1 || context.Initiator == first.Context.Initiator;
    }
}