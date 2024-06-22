using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class RequireTryingToMoveCommand : ICommand
{
    private RequireTryingToMoveCommandDef Params;

    public RequireTryingToMoveCommand(RequireTryingToMoveCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}