using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Impact;

public class HealDamageCommand : Command, ICommand
{
    private HealDamageCommandDef Params;

    public HealDamageCommand(HealDamageCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
    }
}