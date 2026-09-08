using GameServer.StaticDB.Records.apt;

namespace GameServer.Systems.Aptitude.Commands.Duration;

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
        var baseTime = context.EffectStartTime ?? context.InitTime;
        var duration = AbilitySystem.RegistryOp(context.Register, Params.DurationMs, (Enums.Operand)Params.DurationRegop);

        // The clocks are uint milliseconds. A small client clock lead (e.g. 67 ms in the pad log) is not
        // 49 days of elapsed time. Signed modular subtraction also handles the uint clock wrapping.
        var elapsed = unchecked((int)(currentTime - baseTime));
        var condition = elapsed > duration;

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