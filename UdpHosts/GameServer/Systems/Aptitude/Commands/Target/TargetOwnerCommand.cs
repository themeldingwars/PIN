using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Target;

public class TargetOwnerCommand : Command, ICommand
{
    private TargetOwnerCommandDef Params;

    public TargetOwnerCommand(TargetOwnerCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        context.FormerTargets = new AptitudeTargets(context.Targets);
        var target = context.Self;

        if (target.Owner != null)
        {
            context.Targets.Push(target.Owner);
        }
        else if (Params.FailNone == 1)
        {
            return false;
        }

        return true;
    }
}