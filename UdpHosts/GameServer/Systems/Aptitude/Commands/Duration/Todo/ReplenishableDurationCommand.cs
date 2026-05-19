using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Duration;

public class ReplenishableDurationCommand : Command, ICommand
{
    private ReplenishableDurationCommandDef Params;

    public ReplenishableDurationCommand(ReplenishableDurationCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
    }
}