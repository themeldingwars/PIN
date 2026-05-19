using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Unlock;

public class UnlockWarpaintsCommand : Command, ICommand
{
    private UnlockWarpaintsCommandDef Params;

    public UnlockWarpaintsCommand(UnlockWarpaintsCommandDef par)
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