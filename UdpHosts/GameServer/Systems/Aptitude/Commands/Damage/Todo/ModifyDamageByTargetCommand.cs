using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Damage;

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