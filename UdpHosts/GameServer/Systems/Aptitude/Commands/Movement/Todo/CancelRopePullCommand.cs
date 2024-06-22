using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class CancelRopePullCommand : ICommand
{
    private CancelRopePullCommandDef Params;

    public CancelRopePullCommand(CancelRopePullCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}