using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Damage;

public class ModifyDamageByHeadshotCommand : Command, ICommand
{
    private ModifyDamageByHeadshotCommandDef Params;

    public ModifyDamageByHeadshotCommand(ModifyDamageByHeadshotCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
    }
}