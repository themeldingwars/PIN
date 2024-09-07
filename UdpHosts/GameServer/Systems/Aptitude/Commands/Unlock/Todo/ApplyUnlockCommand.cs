using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class ApplyUnlockCommand : Command, ICommand
{
    private ApplyUnlockCommandDef Params;

    public ApplyUnlockCommand(ApplyUnlockCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}