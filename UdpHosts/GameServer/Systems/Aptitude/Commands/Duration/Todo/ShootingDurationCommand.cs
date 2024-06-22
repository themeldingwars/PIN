using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class ShootingDurationCommand : ICommand
{
    private ShootingDurationCommandDef Params;

    public ShootingDurationCommand(ShootingDurationCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}