using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

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