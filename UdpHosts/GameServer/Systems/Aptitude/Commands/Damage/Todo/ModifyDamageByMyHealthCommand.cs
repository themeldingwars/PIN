using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Damage;

public class ModifyDamageByMyHealthCommand : Command, ICommand
{
    private ModifyDamageByMyHealthCommandDef Params;

    public ModifyDamageByMyHealthCommand(ModifyDamageByMyHealthCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
    }
}