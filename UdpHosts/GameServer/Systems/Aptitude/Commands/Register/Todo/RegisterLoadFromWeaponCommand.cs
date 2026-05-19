using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Register;

public class RegisterLoadFromWeaponCommand : Command, ICommand
{
    private RegisterLoadFromWeaponCommandDef Params;

    public RegisterLoadFromWeaponCommand(RegisterLoadFromWeaponCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}