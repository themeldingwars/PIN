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

    public bool Execute(Context context)
    {
        return true;
    }
}