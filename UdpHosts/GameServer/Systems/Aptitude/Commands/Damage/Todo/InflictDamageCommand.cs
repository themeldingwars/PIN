using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Damage;

public class InflictDamageCommand : Command, ICommand
{
    private InflictDamageCommandDef Params;

    public InflictDamageCommand(InflictDamageCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
    }
}