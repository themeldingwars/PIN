using GameServer.StaticDB.Records.apt;

namespace GameServer.Systems.Aptitude.Commands.Register;

public class RegisterComparisonCommand : Command, ICommand
{
    private RegisterComparisonCommandDef Params;

    public RegisterComparisonCommand(RegisterComparisonCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        // NOTE: The params can have multiple set, need to double check if this implementation is appropriate. It currently returns true if any condition matches.
        bool cmdResult = false;

        if (Params.EqualTo == 1)
        {
            float minValue = Params.CompareVal - Params.EqualTol;
            float maxValue = Params.CompareVal + Params.EqualTol;

            if (context.Register >= minValue && context.Register <= maxValue)
            {
                cmdResult = true;
            }
        }

        if (Params.LessThan == 1)
        {
            if (context.Register < Params.CompareVal)
            {
                cmdResult = true;
            }
        }

        if (Params.GreaterThan == 1)
        {
            if (context.Register > Params.CompareVal)
            {
                cmdResult = true;
            }
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

    public override void Reset(Context context)
    {
    }
}