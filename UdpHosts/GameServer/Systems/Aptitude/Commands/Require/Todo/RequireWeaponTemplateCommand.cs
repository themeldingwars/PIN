using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class RequireWeaponTemplateCommand : Command, ICommand
{
    private RequireWeaponTemplateCommandDef Params;

    public RequireWeaponTemplateCommand(RequireWeaponTemplateCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}