using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Damage;

public class SetWeaponDamageCommand : Command, ICommand
{
    private SetWeaponDamageCommandDef Params;

    public SetWeaponDamageCommand(SetWeaponDamageCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}