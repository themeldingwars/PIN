using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class ModifyDamageByTargetHealthCommand : ICommand
{
    private ModifyDamageByTargetHealthCommandDef Params;

    public ModifyDamageByTargetHealthCommand(ModifyDamageByTargetHealthCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}