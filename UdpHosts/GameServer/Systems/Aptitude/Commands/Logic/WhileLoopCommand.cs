using GameServer.Data.SDB.Records.apt;

namespace GameServer.Aptitude;

public class WhileLoopCommand : ICommand
{
    private const uint MaximumLaps = 100;
    private WhileLoopCommandDef Params;

    public WhileLoopCommand(WhileLoopCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        if (Params.BodyChain == 0 && Params.ConditionChain == 0)
        {
            // Guard against weird case 421249
            return true;
        }

        var conditionChain = context.Abilities.Factory.LoadChain(Params.ConditionChain);
        var bodyChain = context.Abilities.Factory.LoadChain(Params.BodyChain);
    
        uint lap = 0;
        while (lap < MaximumLaps)
        {
            var conditionResult = conditionChain.Execute(context);

            if (Params.DoWhile != 0)
            {
                bodyChain.Execute(context);

                if (!conditionResult)
                {
                    break;
                }
            }
            else
            {
                if (!conditionResult)
                {
                    break;
                }

                bodyChain.Execute(context);
            }

            lap++;
        }

        return true;
    }
}