using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

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