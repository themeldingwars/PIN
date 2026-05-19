using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Duration;

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