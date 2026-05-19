using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Cooldown;

public class ResetCooldownsCommand : Command, ICommand
{
    private ResetCooldownsCommandDef Params;

    public ResetCooldownsCommand(ResetCooldownsCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
    }
}