using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class SetWeaponDamageCommand : ICommand
{
    private SetWeaponDamageCommandDef Params;

    public SetWeaponDamageCommand(SetWeaponDamageCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}