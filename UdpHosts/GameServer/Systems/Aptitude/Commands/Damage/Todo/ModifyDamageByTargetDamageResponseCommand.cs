using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Damage;

public class ModifyDamageByTargetDamageResponseCommand : Command, ICommand
{
    private ModifyDamageByTargetDamageResponseCommandDef Params;

    public ModifyDamageByTargetDamageResponseCommand(ModifyDamageByTargetDamageResponseCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}