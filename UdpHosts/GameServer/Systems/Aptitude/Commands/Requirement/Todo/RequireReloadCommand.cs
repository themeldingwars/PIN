using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class RequireReloadCommand : Command, ICommand
{
    private RequireReloadCommandDef Params;

    public RequireReloadCommand(RequireReloadCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}