using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

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