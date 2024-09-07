using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class ClimbLedgeCommand : Command, ICommand
{
    private ClimbLedgeCommandDef Params;

    public ClimbLedgeCommand(ClimbLedgeCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}