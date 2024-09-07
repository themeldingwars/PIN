using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

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