using GameServer.StaticDB.Records.apt;

namespace GameServer.Systems.Aptitude.Commands.Cooldown;

public class InflictCooldownCommand : Command, ICommand
{
    private InflictCooldownCommandDef Params;

    public InflictCooldownCommand(InflictCooldownCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
    }
}