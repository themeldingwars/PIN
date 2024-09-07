using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class ModifyDamageByTargetDamageResponseCommand : Command, ICommand
{
    private ModifyDamageByTargetDamageResponseCommandDef Params;

    public ModifyDamageByTargetDamageResponseCommand(ModifyDamageByTargetDamageResponseCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}