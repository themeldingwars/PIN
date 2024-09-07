using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class ModifyDamageByTargetCommand : Command, ICommand
{
    private ModifyDamageByTargetCommandDef Params;

    public ModifyDamageByTargetCommand(ModifyDamageByTargetCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}