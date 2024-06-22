using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class RequireLineOfSightCommand : ICommand
{
    private RequireLineOfSightCommandDef Params;

    public RequireLineOfSightCommand(RequireLineOfSightCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}