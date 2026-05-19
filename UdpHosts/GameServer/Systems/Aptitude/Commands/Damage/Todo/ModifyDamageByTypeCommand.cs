using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Damage;

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