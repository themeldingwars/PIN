using GameServer.Data.SDB.Records.apt;

namespace GameServer.Aptitude;

public class TimeDurationCommand : Command, ICommand
{
    private TimeDurationCommandDef Params;

    public TimeDurationCommand(TimeDurationCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        var currentTime = context.Shard.CurrentTime;
        var baseTime = context.InitTime;
        var duration = AbilitySystem.RegistryOp(context.Register, Params.DurationMs, (Enums.Operand)Params.DurationRegop);
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