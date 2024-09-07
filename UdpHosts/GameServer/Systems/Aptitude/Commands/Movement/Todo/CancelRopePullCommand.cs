using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class CancelRopePullCommand : Command, ICommand
{
    private CancelRopePullCommandDef Params;

    public CancelRopePullCommand(CancelRopePullCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}