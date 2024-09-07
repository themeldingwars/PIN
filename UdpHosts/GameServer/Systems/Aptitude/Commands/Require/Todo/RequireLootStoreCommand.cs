using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class RequireLootStoreCommand : Command, ICommand
{
    private RequireLootStoreCommandDef Params;

    public RequireLootStoreCommand(RequireLootStoreCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}