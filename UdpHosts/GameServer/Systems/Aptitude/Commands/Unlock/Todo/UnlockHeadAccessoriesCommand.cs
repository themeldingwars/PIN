using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Unlock;

public class UnlockHeadAccessoriesCommand : Command, ICommand
{
    private UnlockHeadAccessoriesCommandDef Params;

    public UnlockHeadAccessoriesCommand(UnlockHeadAccessoriesCommandDef par)
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