using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class RequireMovementFlagsCommand : Command, ICommand
{
    private RequireMovementFlagsCommandDef Params;

    public RequireMovementFlagsCommand(RequireMovementFlagsCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}
