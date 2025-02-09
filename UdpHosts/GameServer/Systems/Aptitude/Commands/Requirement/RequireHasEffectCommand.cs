using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class RequireHasEffectCommand : Command, ICommand
{
    private RequireHasEffectCommandDef Params;

    public RequireHasEffectCommand(RequireHasEffectCommandDef par)
: base(par)
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

                        if (Params.SameInitiator == 1 && context.Initiator != active.Context.Initiator)
                        {
                            targetResult = false;
                        }

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

        /*
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

                    if (Params.SameInitiator == 1 && context.Initiator != active.Context.Initiator)
                    {
                        result = false;
                    }

                    break;
                }
            }
        }
        */

        if (Params.Negate == 1)
        {
            result = !result;
        }

        return result;
    }
}