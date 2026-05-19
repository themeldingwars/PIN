using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Requirement;

public class RequireEquippedItemCommand : Command, ICommand
{
    private RequireEquippedItemCommandDef Params;

    public RequireEquippedItemCommand(RequireEquippedItemCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}