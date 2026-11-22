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

        // NOTE: Investigate target handling
        if (context.Targets.Count > 0)
        {
            cmdResult = true;
            foreach (IAptitudeTarget target in context.Targets)
            {
                if (!HasEffect(target, context))
                {
                    cmdResult = false;
                    break;
                }
            }
        }
        else
        {
            // Effect chains (e.g. a duration chain) usually run without targets, the client checks self in that case
            cmdResult = HasEffect(context.Self, context);
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