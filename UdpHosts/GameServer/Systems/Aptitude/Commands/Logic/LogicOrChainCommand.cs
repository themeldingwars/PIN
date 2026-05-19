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

    public bool Execute(Context context)
    {
        var chain = context.Abilities.Factory.LoadChain(Params.OrChain);

        var prevExecutionHint = context.ExecutionHint;
        context.ExecutionHint = ExecutionHint.Logic;
        bool result = chain.Execute(context, Chain.ExecutionMethod.OrChain);
        context.ExecutionHint = prevExecutionHint;

        if (Params.AlwaysSuccess == 1)
        {
            return true;
        }
        else
        {
            return result;
        }
    }
}