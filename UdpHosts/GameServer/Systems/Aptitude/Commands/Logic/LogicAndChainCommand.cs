using GameServer.StaticDB.Records.apt;

namespace GameServer.Systems.Aptitude.Commands.Logic;

public class LogicAndChainCommand : Command, ICommand
{
    private LogicAndChainCommandDef Params;

    public LogicAndChainCommand(LogicAndChainCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        var chain = context.Abilities.Factory.LoadChain(Params.AndChain);

        var prevExecutionHint = context.ExecutionHint;
        context.ExecutionHint = ExecutionHint.Logic;
        var chainResult = new CommandResult { Success = true };
        chain.Execute(context, ref chainResult, Chain.ExecutionMethod.AndChain);
        context.ExecutionHint = prevExecutionHint;

        if (Params.AlwaysSuccess == 1)
        {
            result.SetPass();
            return;
        }
        else
        {
            if (chainResult.Success)
            {
                result.SetPass();
                return;
            }
            else
            {
                result.SetFail();
                return;
            }
        }
    }
}