using GameServer.StaticDB.Records.apt;

namespace GameServer.Systems.Aptitude.Commands.Target;

public class PopTargetsCommand : Command, ICommand
{
    private PopTargetsCommandDef Params;

    public PopTargetsCommand(PopTargetsCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        if (Params.Former == 1)
        {
            var ok = context.TargetStack.TryPop(out var result);
            if (ok)
            {
                context.FormerTargets = result;
            }

            return ok;
        }

        if (Params.Current == 1)
        {
            var ok = context.TargetStack.TryPop(out var result);
            if (ok)
            {
                context.Targets = result;
            }

            return ok;
        }

        return true;
    }
}