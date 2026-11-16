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
        if (Params?.DurationMs is not { } duration)
        {
            result.SetPass();
            return;
        }

        var replenished = context.ReplenishedTime ?? context.InitTime ?? context.CurrentTime;
        if (unchecked((int)(context.CurrentTime - replenished)) <= (int)duration)
        {
            result.SetPass();
        }
        else
        {
            result.SetFail();
        }
    }
}