using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class RequireReloadCommand : ICommand
{
    private RequireReloadCommandDef Params;

    public RequireReloadCommand(RequireReloadCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}