using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class ModifyDamageByFactionCommand : Command, ICommand
{
    private ModifyDamageByFactionCommandDef Params;

    public ModifyDamageByFactionCommand(ModifyDamageByFactionCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}