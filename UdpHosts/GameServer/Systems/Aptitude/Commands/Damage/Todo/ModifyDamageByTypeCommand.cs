using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class ModifyDamageByTypeCommand : ICommand
{
    private ModifyDamageByTypeCommandDef Params;

    public ModifyDamageByTypeCommand(ModifyDamageByTypeCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}