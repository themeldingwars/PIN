using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class ModifyDamageByMyHealthCommand : Command, ICommand
{
    private ModifyDamageByMyHealthCommandDef Params;

    public ModifyDamageByMyHealthCommand(ModifyDamageByMyHealthCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}