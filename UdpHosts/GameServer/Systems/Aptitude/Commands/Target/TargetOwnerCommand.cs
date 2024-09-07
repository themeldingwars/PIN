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
        // todo aptitude: handle owner
        if (Params.FailNone == 1)
        {
            return false;
        }

        return true;
    }
}