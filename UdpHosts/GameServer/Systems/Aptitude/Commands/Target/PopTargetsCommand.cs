using GameServer.Data.SDB.Records.apt;

namespace GameServer.Aptitude;

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
            var ok = context.FormerTargets.TryPop(out _);

            return ok;
        }

        if (Params.Current == 1)
        {
            context.Targets = new AptitudeTargets(context.FormerTargets);
            context.FormerTargets = new AptitudeTargets();
        }

        return true;
    }
}