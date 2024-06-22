using GameServer.Data.SDB.Records.apt;

namespace GameServer.Aptitude;

public class TargetClearCommand : ICommand
{
    private TargetClearCommandDef Params;

    public TargetClearCommand(TargetClearCommandDef par)
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
            context.Targets.Clear();
        }

        return true;
    }
}