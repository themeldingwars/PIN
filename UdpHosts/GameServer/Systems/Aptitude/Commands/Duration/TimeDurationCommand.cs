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
        // The client fails this command as well when the context isn't initiated
        if (context.InitTime is not { } initTime)
        {
            result.SetFail();
            return;
        }

        var elapsed = context.CurrentTime > initTime ? context.CurrentTime - initTime : 0;
        var duration = AbilitySystem.RegistryOp(context.Register, Params.DurationMs, (Enums.Operand)Params.DurationRegop);
        var condition = elapsed > duration;

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