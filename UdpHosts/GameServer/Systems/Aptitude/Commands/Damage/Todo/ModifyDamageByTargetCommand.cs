using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class ModifyDamageByTargetCommand : ICommand
{
    private ModifyDamageByTargetCommandDef Params;

    public ModifyDamageByTargetCommand(ModifyDamageByTargetCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}