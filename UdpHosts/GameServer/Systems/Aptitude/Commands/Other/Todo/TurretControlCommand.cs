using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class TurretControlCommand : Command, ICommand
{
    private TurretControlCommandDef Params;

    public TurretControlCommand(TurretControlCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}