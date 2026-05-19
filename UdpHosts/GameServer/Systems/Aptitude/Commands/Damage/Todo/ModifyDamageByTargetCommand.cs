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

    public override void Execute(Context context, ref CommandResult result)
    {
    }
}