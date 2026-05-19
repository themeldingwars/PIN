using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Damage;

public class ModifyDamageByTargetHealthCommand : Command, ICommand
{
    private ModifyDamageByTargetHealthCommandDef Params;

    public ModifyDamageByTargetHealthCommand(ModifyDamageByTargetHealthCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
    }
}