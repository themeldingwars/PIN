using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Requirement;

public class RequireHasEffectCommand : Command, ICommand
{
    private RequireHasEffectCommandDef Params;

    public RequireHasEffectCommand(RequireHasEffectCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        // Logger.Debug("EffectID: {EffectId}", Params.EffectId);
        bool cmdResult = false;

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
                    cmdResult = false;
                    break;
                }
                else
                {
                    matchCounter++;
                }
            }

            if (matchCounter == context.Targets.Count)
            {
                cmdResult = true;
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
            cmdResult = !cmdResult;
        }

        if (cmdResult)
        {
            result.SetPass();
        }
        else
        {
            result.SetFail();
        }

        return;
    }

    public override void Reset(Context context)
    {
        return;
    }
}