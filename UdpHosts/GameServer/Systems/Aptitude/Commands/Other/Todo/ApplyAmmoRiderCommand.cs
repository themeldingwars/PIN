using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class ApplyAmmoRiderCommand : Command, ICommand
{
    private ApplyAmmoRiderCommandDef Params;

    public ApplyAmmoRiderCommand(ApplyAmmoRiderCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}