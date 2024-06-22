using GameServer.Data.SDB.Records.apt;

namespace GameServer.Aptitude;

public class TargetDifferenceCommand : ICommand
{
    private TargetDifferenceCommandDef Params;

    public TargetDifferenceCommand(TargetDifferenceCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        // todo aptitude: target difference

        if (Params.ReplaceFormer == 1)
        {
            context.FormerTargets = context.Targets;
        }

        if (Params.SwapCurrentFormer == 1)
        {
            (context.Targets, context.FormerTargets) = (context.FormerTargets, context.Targets);
        }

        return true;
    }
}