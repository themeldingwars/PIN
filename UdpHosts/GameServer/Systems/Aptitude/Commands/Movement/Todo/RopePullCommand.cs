using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Movement;

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