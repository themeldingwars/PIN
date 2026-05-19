using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Unlock;

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