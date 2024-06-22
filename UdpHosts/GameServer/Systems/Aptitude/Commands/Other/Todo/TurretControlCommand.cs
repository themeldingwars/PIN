using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class TurretControlCommand : ICommand
{
    private TurretControlCommandDef Params;

    public TurretControlCommand(TurretControlCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}