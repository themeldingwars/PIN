using GameServer.Data.SDB.Records.apt;

namespace GameServer.Aptitude;

public class TimeDurationCommand : ICommand
{
    private TimeDurationCommandDef Params;

    public TimeDurationCommand(TimeDurationCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        var currentTime = context.Shard.CurrentTime;
        var baseTime = context.InitTime;
        var duration = Params.DurationMs; // TODO: Handle Params.DurationRegop
        var condition = currentTime - baseTime > duration;

        bool result = true;

        if (condition)
        {
            result = false;
        }

        if (Params.Negate == 1)
        {
            result = !result;
        }

        return result;
    }
}