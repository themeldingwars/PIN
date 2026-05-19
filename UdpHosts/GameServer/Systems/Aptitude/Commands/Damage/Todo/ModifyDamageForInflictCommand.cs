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

    public override void Execute(Context context, ref CommandResult result)
    {
    }
}