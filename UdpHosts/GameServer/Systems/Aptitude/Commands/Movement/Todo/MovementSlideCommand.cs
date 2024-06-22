using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class MovementSlideCommand : ICommand
{
    private MovementSlideCommandDef Params;

    public MovementSlideCommand(MovementSlideCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}