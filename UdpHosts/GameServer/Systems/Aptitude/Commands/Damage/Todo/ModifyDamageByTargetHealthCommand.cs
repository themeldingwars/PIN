using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class ModifyDamageByTargetHealthCommand : Command, ICommand
{
    private ModifyDamageByTargetHealthCommandDef Params;

    public ModifyDamageByTargetHealthCommand(ModifyDamageByTargetHealthCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}