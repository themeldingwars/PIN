using GameServer.StaticDB.Records.apt;

namespace GameServer.Systems.Aptitude.Commands.Logic;

public class LogicOrChainCommand : Command, ICommand
{
    private LogicOrChainCommandDef Params;

    public LogicOrChainCommand(LogicOrChainCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        var chain = context.Abilities.Factory.LoadChain(Params.OrChain);

        var prevExecutionHint = context.ExecutionHint;
        context.ExecutionHint = ExecutionHint.Logic;
        chain.Execute(context, ref result, Chain.ExecutionMethod.OrChain);
        context.ExecutionHint = prevExecutionHint;

        if (Params.AlwaysSuccess == 1)
        {
            result.SetPass();
            return;
        }
    }
}