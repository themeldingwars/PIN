using GameServer.StaticDB.Records.apt;

namespace GameServer.Systems.Aptitude.Commands.Target;

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