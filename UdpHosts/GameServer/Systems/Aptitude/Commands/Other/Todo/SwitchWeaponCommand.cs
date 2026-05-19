using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Other;

public class SwitchWeaponCommand : Command, ICommand
{
    private SwitchWeaponCommandDef Params;

    public SwitchWeaponCommand(SwitchWeaponCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
    }
}