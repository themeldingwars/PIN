using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

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