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

    public bool Execute(Context context)
    {
        return true;
    }
}