using GameServer.StaticDB.Records.apt;

namespace GameServer.Systems.Aptitude.Commands.Target;

public class PushTargetsCommand : Command, ICommand
{
    private PushTargetsCommandDef Params;

    public PushTargetsCommand(PushTargetsCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        if (Params.Former == 1)
        {
            context.TargetStack.Push(context.FormerTargets);
        }

        if (Params.Current == 1)
        {
            context.TargetStack.Push(context.Targets);
        }

        return true;
    }
}