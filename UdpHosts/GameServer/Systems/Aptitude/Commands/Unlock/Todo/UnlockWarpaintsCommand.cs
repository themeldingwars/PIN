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

    public bool Execute(Context context)
    {
        return true;
    }
}