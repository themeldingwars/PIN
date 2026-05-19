using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Requirement;

public class RequireInCombatCommand : Command, ICommand
{
    private RequireInCombatCommandDef Params;

    public RequireInCombatCommand(RequireInCombatCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}