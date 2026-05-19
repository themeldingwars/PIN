using GameServer.StaticDB.Records.apt;

namespace GameServer.Systems.Aptitude.Commands.Logic;

public class WhileLoopCommand : Command, ICommand
{
    private const uint MaximumLaps = 100;
    private WhileLoopCommandDef Params;

    public WhileLoopCommand(WhileLoopCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        if (Params.BodyChain == 0 && Params.ConditionChain == 0)
        {
            // Guard against weird case 421249
            result.SetPass();
            return;
        }

        var conditionChain = context.Abilities.Factory.LoadChain(Params.ConditionChain);
        var bodyChain = context.Abilities.Factory.LoadChain(Params.BodyChain);

        var prevExecutionHint = context.ExecutionHint;
        context.ExecutionHint = ExecutionHint.Logic;

        uint lap = 0;
        while (lap < MaximumLaps)
        {
            var conditionResult = new CommandResult { Success = true };
            conditionChain.Execute(context, ref conditionResult);

            if (Params.DoWhile != 0)
            {
                bodyChain.Execute(context, ref result);

                if (!conditionResult.Success)
                {
                    break;
                }
            }
            else
            {
                if (!conditionResult.Success)
                {
                    break;
                }

                bodyChain.Execute(context, ref result);
            }

            lap++;
        }

        context.ExecutionHint = prevExecutionHint;

        result.SetPass();
        return;
    }
}