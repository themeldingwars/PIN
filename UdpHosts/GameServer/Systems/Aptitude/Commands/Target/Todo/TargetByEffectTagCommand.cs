using GameServer.Data.SDB;
using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class TargetByEffectTagCommand : ICommand
{
    private TargetByEffectTagCommandDef Params;

    public TargetByEffectTagCommand(TargetByEffectTagCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        var result = false;
        var previousTargets = context.Targets;
        var newTargets = new AptitudeTargets();
        var effectTagEffectIds = SDBInterface.GetStatusEffectTag(Params.TagId);

        foreach (IAptitudeTarget target in previousTargets)
        {
            foreach (EffectState active in target.GetActiveEffects())
            {
                if (active == null)
                {
                    continue;
                }

                if (effectTagEffectIds.Contains(active.Effect.Id) && active.Stacks >= Params.StackCount)
                {
                    newTargets.Push(target);
                    break;
                }
            }
        }

        context.FormerTargets = previousTargets;
        context.Targets = newTargets;

        if (Params.FailNoTargets == 1 && context.Targets.Count == 0)
        {
            result = false;
        }
        else
        {
            result = true;
        }

        if (Params.Negate == 1)
        {
            result = !result;
        }

        return result;
    }
}