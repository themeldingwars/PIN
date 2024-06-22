using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class RequireInCombatCommand : ICommand
{
    private RequireInCombatCommandDef Params;

    public RequireInCombatCommand(RequireInCombatCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}