using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class RequireArmyCommand : Command, ICommand
{
    private RequireArmyCommandDef Params;

    public RequireArmyCommand(RequireArmyCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}