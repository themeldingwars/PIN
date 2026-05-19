using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Other;

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