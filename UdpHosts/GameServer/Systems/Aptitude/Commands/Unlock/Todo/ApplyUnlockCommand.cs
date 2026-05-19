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

    public override void Execute(Context context, ref CommandResult result)
    {
    }

    public override void Reset(Context context)
    {
        return;
    }
}