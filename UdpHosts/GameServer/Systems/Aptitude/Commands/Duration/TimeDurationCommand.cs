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

    public override void Execute(Context context, ref CommandResult result)
    {
        var currentTime = context.Shard.CurrentTime;
        var baseTime = context.InitTime;
        var duration = AbilitySystem.RegistryOp(context.Register, Params.DurationMs, (Enums.Operand)Params.DurationRegop);
        var condition = currentTime - baseTime > duration;

        bool cmdResult = true;

        if (condition)
        {
            cmdResult = false;
        }

        if (Params.Negate == 1)
        {
            cmdResult = !cmdResult;
        }

        if (cmdResult)
        {
            result.SetPass();
        }
        else
        {
            result.SetFail();
        }

        return;
    }
}