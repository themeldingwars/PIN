using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

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