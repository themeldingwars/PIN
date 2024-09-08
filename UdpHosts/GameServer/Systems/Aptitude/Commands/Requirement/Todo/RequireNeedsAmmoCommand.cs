using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class RequireNeedsAmmoCommand : Command, ICommand
{
    private RequireNeedsAmmoCommandDef Params;

    public RequireNeedsAmmoCommand(RequireNeedsAmmoCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}