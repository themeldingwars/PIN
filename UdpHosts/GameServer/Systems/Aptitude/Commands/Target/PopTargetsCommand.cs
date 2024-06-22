using GameServer.Data.SDB.Records.apt;

namespace GameServer.Aptitude;

public class PopTargetsCommand : ICommand
{
    private PopTargetsCommandDef Params;

    public PopTargetsCommand(PopTargetsCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        if (Params.Former == 1)
        {
            var ok = context.FormerTargets.TryPop(out _);

            return ok;
        }

        if (Params.Current == 1)
        {
            var ok = context.Targets.TryPop(out _);

            return ok;
        }

        return true;
    }
}