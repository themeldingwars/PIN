using GameServer.StaticDB.Records.apt;

namespace GameServer.Systems.Aptitude.Commands.Duration;

public class AimRangeDurationCommand : Command, ICommand
{
    private AimRangeDurationCommandDef Params;

    public AimRangeDurationCommand(AimRangeDurationCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}