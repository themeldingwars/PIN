using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Movement;

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