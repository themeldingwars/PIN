using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Damage;

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