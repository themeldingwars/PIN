using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Movement;

public class ClimbLedgeCommand : Command, ICommand
{
    private ClimbLedgeCommandDef Params;

    public ClimbLedgeCommand(ClimbLedgeCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}