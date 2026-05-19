using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Requirement;

public class RequireHasUnlockCommand : Command, ICommand
{
    private RequireHasUnlockCommandDef Params;

    public RequireHasUnlockCommand(RequireHasUnlockCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}