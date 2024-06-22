using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class RequireLootStoreCommand : ICommand
{
    private RequireLootStoreCommandDef Params;

    public RequireLootStoreCommand(RequireLootStoreCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}