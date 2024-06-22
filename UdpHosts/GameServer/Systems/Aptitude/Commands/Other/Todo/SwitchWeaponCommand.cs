using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class SwitchWeaponCommand : ICommand
{
    private SwitchWeaponCommandDef Params;

    public SwitchWeaponCommand(SwitchWeaponCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}