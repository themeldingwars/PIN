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

    public override void Execute(Context context, ref CommandResult result)
    {
    }

    public override void Reset(Context context)
    {
    }
}