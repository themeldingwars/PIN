using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class RopePullCommand : Command, ICommand
{
    private RopePullCommandDef Params;

    public RopePullCommand(RopePullCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}