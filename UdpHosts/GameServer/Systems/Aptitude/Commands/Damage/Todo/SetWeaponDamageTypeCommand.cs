using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class SetWeaponDamageTypeCommand : ICommand
{
    private SetWeaponDamageTypeCommandDef Params;

    public SetWeaponDamageTypeCommand(SetWeaponDamageTypeCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}