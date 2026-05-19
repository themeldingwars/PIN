using GameServer.StaticDB.Records.apt;

namespace GameServer.Systems.Aptitude.Commands.Target;

public class TargetSelfCommand : Command, ICommand
{
    private TargetSelfCommandDef Params;

    public TargetSelfCommand(TargetSelfCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        context.FormerTargets = new AptitudeTargets(context.Targets);

        context.Targets.Push(context.Self);

        return true;
    }
}