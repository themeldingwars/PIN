using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Movement;

public class MovementTetherCommand : Command, ICommand
{
    private MovementTetherCommandDef Params;

    public MovementTetherCommand(MovementTetherCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
    }
}