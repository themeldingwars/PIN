using GameServer.Data.SDB.Records.apt;

namespace GameServer.Aptitude;

public class TargetPreviousCommand : Command, ICommand
{
    private TargetPreviousCommandDef Params;

    public TargetPreviousCommand(TargetPreviousCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        context.Targets = new AptitudeTargets(context.FormerTargets);

        if (Params.Clearformer == 1)
        {
            context.FormerTargets.Clear();
        }

        return true;
    }
}