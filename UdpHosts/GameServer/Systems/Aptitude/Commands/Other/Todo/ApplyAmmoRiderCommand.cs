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

    public override void Execute(Context context, ref CommandResult result)
    {
    }
}