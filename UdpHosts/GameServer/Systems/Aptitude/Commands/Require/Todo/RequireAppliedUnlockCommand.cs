using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class RequireAppliedUnlockCommand : ICommand
{
    private RequireAppliedUnlockCommandDef Params;

    public RequireAppliedUnlockCommand(RequireAppliedUnlockCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}