using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class ModifyDamageByFactionCommand : ICommand
{
    private ModifyDamageByFactionCommandDef Params;

    public ModifyDamageByFactionCommand(ModifyDamageByFactionCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}