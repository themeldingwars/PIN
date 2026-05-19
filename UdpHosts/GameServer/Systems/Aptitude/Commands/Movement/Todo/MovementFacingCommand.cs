using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Movement;

public class MovementFacingCommand : Command, ICommand
{
    private MovementFacingCommandDef Params;

    public MovementFacingCommand(MovementFacingCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}