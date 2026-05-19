using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Duration;

public class ShootingDurationCommand : Command, ICommand
{
    private ShootingDurationCommandDef Params;

    public ShootingDurationCommand(ShootingDurationCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}