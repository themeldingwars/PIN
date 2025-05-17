using GameServer.Data.SDB.Records.apt;

namespace GameServer.Aptitude;

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