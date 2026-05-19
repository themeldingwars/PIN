using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Movement;

public class MovementSlideCommand : Command, ICommand
{
    private MovementSlideCommandDef Params;

    public MovementSlideCommand(MovementSlideCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
    }
}