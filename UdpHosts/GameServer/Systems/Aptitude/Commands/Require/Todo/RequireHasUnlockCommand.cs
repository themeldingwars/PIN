using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class RequireHasUnlockCommand : ICommand
{
    private RequireHasUnlockCommandDef Params;

    public RequireHasUnlockCommand(RequireHasUnlockCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}