using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class ModifyDamageByTypeCommand : Command, ICommand
{
    private ModifyDamageByTypeCommandDef Params;

    public ModifyDamageByTypeCommand(ModifyDamageByTypeCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}