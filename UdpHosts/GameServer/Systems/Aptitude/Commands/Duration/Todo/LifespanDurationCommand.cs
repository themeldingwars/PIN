using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Duration;

public class LifespanDurationCommand : Command, ICommand
{
    private LifespanDurationCommandDef Params;

    public LifespanDurationCommand(LifespanDurationCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
    }
}