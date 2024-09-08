using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class RequireLineOfSightCommand : Command, ICommand
{
    private RequireLineOfSightCommandDef Params;

    public RequireLineOfSightCommand(RequireLineOfSightCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}