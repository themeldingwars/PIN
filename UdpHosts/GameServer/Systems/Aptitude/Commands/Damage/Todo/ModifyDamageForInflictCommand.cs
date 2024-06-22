using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class ModifyDamageForInflictCommand : ICommand
{
    private ModifyDamageForInflictCommandDef Params;

    public ModifyDamageForInflictCommand(ModifyDamageForInflictCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}