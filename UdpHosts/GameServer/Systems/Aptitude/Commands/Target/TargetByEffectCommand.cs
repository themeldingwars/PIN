using System;
using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class TargetByEffectCommand : Command, ICommand
{
    private TargetByEffectCommandDef Params;

    public TargetByEffectCommand(TargetByEffectCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        // todo Params.FilterList, equal to 1 in 1086 instances, 0 in 6 instances
        var previousTargets = context.Targets;
        var newTargets = new AptitudeTargets();

        Console.WriteLine("prev:" + previousTargets.Count);
        foreach (IAptitudeTarget target in previousTargets)
        {
            foreach (EffectState active in target.GetActiveEffects())
            {
                if (active == null)
                {
                    continue;
                }

                var condition = Params.EffectId == active.Effect.Id && active.Stacks >= Params.StackCount;
                if (Params.Negate == 1)
                {
                    condition = !condition;
                }

                if (condition)
                {
                    if (Params.SameInitiator == 1)
                    {
                        var condition2 = Params.Negate == 1
                                             ? context.Initiator == active.Context.Initiator
                                             : context.Initiator != active.Context.Initiator;
                        if (condition2)
                        {
                            newTargets.Push(target);
                        }
                    }
                    else
                    {
                        newTargets.Push(target);
                    }

                    break;
                }
            }
        }

        context.FormerTargets = previousTargets;
        context.Targets = newTargets;

        if (Params.FailNoTargets == 1 && context.Targets.Count == 0)
        {
            return false;
        }

        return true;
    }
}