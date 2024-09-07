using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class MovementSlideCommand : Command, ICommand
{
    private MovementSlideCommandDef Params;

    public MovementSlideCommand(MovementSlideCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}