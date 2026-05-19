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

    public override void Execute(Context context, ref CommandResult result)
    {
    }
}