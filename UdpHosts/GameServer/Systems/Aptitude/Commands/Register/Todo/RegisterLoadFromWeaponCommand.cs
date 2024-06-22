using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class RegisterLoadFromWeaponCommand : ICommand
{
    private RegisterLoadFromWeaponCommandDef Params;

    public RegisterLoadFromWeaponCommand(RegisterLoadFromWeaponCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}