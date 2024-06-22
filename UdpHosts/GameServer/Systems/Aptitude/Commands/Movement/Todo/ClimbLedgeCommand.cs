using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class ClimbLedgeCommand : ICommand
{
    private ClimbLedgeCommandDef Params;

    public ClimbLedgeCommand(ClimbLedgeCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}