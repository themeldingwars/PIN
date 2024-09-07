using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class ForcedMovementDurationCommand : Command, ICommand
{
    private ForcedMovementDurationCommandDef Params;

    public ForcedMovementDurationCommand(ForcedMovementDurationCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}