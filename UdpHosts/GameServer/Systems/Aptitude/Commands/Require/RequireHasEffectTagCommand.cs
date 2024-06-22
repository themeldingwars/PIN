using System;
using GameServer.Data.SDB;
using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class RequireHasEffectTagCommand : ICommand
{
    private RequireHasEffectTagCommandDef Params;

    public RequireHasEffectTagCommand(RequireHasEffectTagCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        Console.WriteLine($"[RequireHasEffectTag] EffectTag: {Params.TagId}");
        bool result = false;
        var effectTagEffectIds = SDBInterface.GetStatusEffectTag(Params.TagId);

        if (context.Targets.Count > 0)
        {
            uint matchCounter = 0;
            foreach (IAptitudeTarget target in context.Targets)
            {
                foreach (EffectState active in target.GetActiveEffects())
                {
                    if (active == null)
                    {
                        continue;
                    }

                    if (effectTagEffectIds.Contains(active.Effect.Id) && active.Stacks >= Params.StackCount)
                    {
                        matchCounter++;
                        break;
                    }
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

                if (effectTagEffectIds.Contains(active.Effect.Id) && active.Stacks >= Params.StackCount)
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