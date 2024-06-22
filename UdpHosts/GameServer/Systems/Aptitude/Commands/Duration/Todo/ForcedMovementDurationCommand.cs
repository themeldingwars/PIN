using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class ForcedMovementDurationCommand : ICommand
{
    private ForcedMovementDurationCommandDef Params;

    public ForcedMovementDurationCommand(ForcedMovementDurationCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}