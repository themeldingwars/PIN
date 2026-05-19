using GameServer.StaticDB;
using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Requirement;

public class RequireHasEffectTagCommand : Command, ICommand
{
    private RequireHasEffectTagCommandDef Params;

    public RequireHasEffectTagCommand(RequireHasEffectTagCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        Logger.Debug("[{Command} {CommandId}] EffectTag: {TagId}", nameof(RequireHasEffectTagCommand), Params.Id, Params.TagId);
        bool cmdResult = false;
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
                cmdResult = true;
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
                    cmdResult = true;
                    break;
                }
            }
        }

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