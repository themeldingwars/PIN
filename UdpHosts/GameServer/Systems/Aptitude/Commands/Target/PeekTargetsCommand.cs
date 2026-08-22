using GameServer.StaticDB.Records.apt;

namespace GameServer.Systems.Aptitude.Commands.Target;

public class PeekTargetsCommand : Command, ICommand
{
    private PeekTargetsCommandDef Params;

    public PeekTargetsCommand(PeekTargetsCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        /* Former = 1 appears once in SDB, Current = 1 appears 107 times, they are mutually exclusive */

        if (Params.Former == 1)
        {
            var ok = context.TargetStack.TryPeek(out var result);
            if (ok)
            {
                context.FormerTargets = result;
            }

            return ok;
        }

        if (Params.Current == 1)
        {
            var ok = context.TargetStack.TryPeek(out var result);
            if (ok)
            {
                context.Targets = result;
            }

            return ok;
        }

        return true;
    }
}