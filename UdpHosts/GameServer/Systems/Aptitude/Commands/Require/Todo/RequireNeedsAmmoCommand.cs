using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class RequireNeedsAmmoCommand : ICommand
{
    private RequireNeedsAmmoCommandDef Params;

    public RequireNeedsAmmoCommand(RequireNeedsAmmoCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}