using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class RequireArmyCommand : ICommand
{
    private RequireArmyCommandDef Params;

    public RequireArmyCommand(RequireArmyCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}