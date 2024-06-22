using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class MovementTetherCommand : ICommand
{
    private MovementTetherCommandDef Params;

    public MovementTetherCommand(MovementTetherCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}