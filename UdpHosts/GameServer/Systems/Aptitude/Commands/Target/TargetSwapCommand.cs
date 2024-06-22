using GameServer.Data.SDB.Records.apt;

namespace GameServer.Aptitude;

public class TargetSwapCommand : ICommand
{
    private TargetSwapCommandDef Params;

    public TargetSwapCommand(TargetSwapCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        if (Params.ClearCurrent == 1)
        {
            context.Targets.Clear();
        }

        if (Params.ClearFormer == 1)
        {
            context.FormerTargets.Clear();
        }

        (context.Targets, context.FormerTargets) = (context.FormerTargets, context.Targets);

        return true;
    }
}