using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Requirement;

public class RequireAppliedUnlockCommand : Command, ICommand
{
    private RequireAppliedUnlockCommandDef Params;

    public RequireAppliedUnlockCommand(RequireAppliedUnlockCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}