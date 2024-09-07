using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class ModifyDamageForInflictCommand : Command, ICommand
{
    private ModifyDamageForInflictCommandDef Params;

    public ModifyDamageForInflictCommand(ModifyDamageForInflictCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}