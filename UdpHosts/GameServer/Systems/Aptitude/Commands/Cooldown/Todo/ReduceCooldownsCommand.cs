using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Cooldown;

public class ReduceCooldownsCommand : Command, ICommand
{
    private ReduceCooldownsCommandDef Params;

    public ReduceCooldownsCommand(ReduceCooldownsCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
    }
}