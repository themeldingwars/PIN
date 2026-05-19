using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Damage;

public class SetDefaultDamageBonusCommand : Command, ICommand
{
    private SetDefaultDamageBonusCommandDef Params;

    public SetDefaultDamageBonusCommand(SetDefaultDamageBonusCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
    }
}