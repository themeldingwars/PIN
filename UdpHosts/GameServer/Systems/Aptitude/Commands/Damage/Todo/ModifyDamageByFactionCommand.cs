using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Damage;

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