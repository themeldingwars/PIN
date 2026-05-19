using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Requirement;

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