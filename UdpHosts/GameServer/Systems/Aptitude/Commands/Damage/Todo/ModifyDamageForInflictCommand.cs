using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Damage;

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