using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class ModifyDamageByTargetDamageResponseCommand : ICommand
{
    private ModifyDamageByTargetDamageResponseCommandDef Params;

    public ModifyDamageByTargetDamageResponseCommand(ModifyDamageByTargetDamageResponseCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}