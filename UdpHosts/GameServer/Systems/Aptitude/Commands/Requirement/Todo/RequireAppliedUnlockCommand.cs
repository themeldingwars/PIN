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

    public override void Execute(Context context, ref CommandResult result)
    {
    }

    public override void Reset(Context context)
    {
    }
}