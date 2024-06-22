using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class MovementFacingCommand : ICommand
{
    private MovementFacingCommandDef Params;

    public MovementFacingCommand(MovementFacingCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}