using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class ModifyDamageByMyHealthCommand : ICommand
{
    private ModifyDamageByMyHealthCommandDef Params;

    public ModifyDamageByMyHealthCommand(ModifyDamageByMyHealthCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}