using GameServer.Data.SDB.Records.apt;

namespace GameServer.Aptitude;

public class TargetClearCommand : Command, ICommand
{
    private TargetClearCommandDef Params;

    public TargetClearCommand(TargetClearCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        if (Params.Former == 1)
        {
            context.FormerTargets.Clear();
        }

        if (Params.Current == 1)
        {
            context.FormerTargets = new AptitudeTargets(context.Targets);
            context.Targets.Clear();
        }

        return true;
    }
}