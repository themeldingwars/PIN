using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class RequireMovementFlagsCommand : ICommand
{
    private RequireMovementFlagsCommandDef Params;

    public RequireMovementFlagsCommand(RequireMovementFlagsCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}
