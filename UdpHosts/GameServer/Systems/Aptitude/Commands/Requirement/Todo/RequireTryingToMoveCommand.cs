using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Requirement;

public class RequireTryingToMoveCommand : Command, ICommand
{
    private RequireTryingToMoveCommandDef Params;

    public RequireTryingToMoveCommand(RequireTryingToMoveCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}