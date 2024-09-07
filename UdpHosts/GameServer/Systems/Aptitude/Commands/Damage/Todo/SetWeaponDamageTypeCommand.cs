using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class SetWeaponDamageTypeCommand : Command, ICommand
{
    private SetWeaponDamageTypeCommandDef Params;

    public SetWeaponDamageTypeCommand(SetWeaponDamageTypeCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}