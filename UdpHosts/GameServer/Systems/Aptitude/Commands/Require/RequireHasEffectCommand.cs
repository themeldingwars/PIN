using GameServer.Data.SDB.Records.aptfs;
using System;

namespace GameServer.Aptitude;

public class RequireHasEffectCommand : ICommand
{
    private RequireHasEffectCommandDef Params;

    public RequireHasEffectCommand(RequireHasEffectCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        // Console.WriteLine($"EffectID: {Params.EffectId}");
        bool result = false;

        // TODO: Handle Params.SameInitiator
        // NOTE: Investigate target handling
        if (context.Targets.Count > 0)
        {
            uint matchCounter = 0;
            foreach (IAptitudeTarget target in context.Targets)
            {
                bool targetResult = false;
                foreach (EffectState active in target.GetActiveEffects())
                {
                    if (active == null)
                    {
                        continue;
                    }

                    if (active.Effect.Id == Params.EffectId && active.Stacks >= Params.StackCount)
                    {
                        targetResult = true;
                        break;
                    }
                }

                if (!targetResult)
                {
                    result = false;
                    break;
                }
                else
                {
                    matchCounter++;
                }
            }
            
            if (matchCounter == context.Targets.Count)
            {
                result = true;
            }
        }
        else
        {
            var target = context.Self;
            foreach (EffectState active in target.GetActiveEffects())
            {
                if (active == null)
                {
                    continue;
                }

                if (active.Effect.Id == Params.EffectId && active.Stacks >= Params.StackCount)
                {
                    result = true;
                    break;
                }
            }
        }

        if (Params.Negate == 1)
        {
            result = !result;
        }

        return result;
    }
}