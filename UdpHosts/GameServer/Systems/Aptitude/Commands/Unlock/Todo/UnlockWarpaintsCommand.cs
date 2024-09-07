using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

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