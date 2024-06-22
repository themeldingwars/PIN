using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class ApplyAmmoRiderCommand : ICommand
{
    private ApplyAmmoRiderCommandDef Params;

    public ApplyAmmoRiderCommand(ApplyAmmoRiderCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}