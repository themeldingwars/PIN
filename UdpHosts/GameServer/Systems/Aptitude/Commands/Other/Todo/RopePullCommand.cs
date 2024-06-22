using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class RopePullCommand : ICommand
{
    private RopePullCommandDef Params;

    public RopePullCommand(RopePullCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}