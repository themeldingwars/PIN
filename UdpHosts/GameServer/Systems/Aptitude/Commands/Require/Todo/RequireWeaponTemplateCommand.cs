using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class RequireWeaponTemplateCommand : ICommand
{
    private RequireWeaponTemplateCommandDef Params;

    public RequireWeaponTemplateCommand(RequireWeaponTemplateCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}