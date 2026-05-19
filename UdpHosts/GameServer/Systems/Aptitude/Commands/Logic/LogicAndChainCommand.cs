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

    public bool Execute(Context context)
    {
        var chain = context.Abilities.Factory.LoadChain(Params.AndChain);

        var prevExecutionHint = context.ExecutionHint;
        context.ExecutionHint = ExecutionHint.Logic;
        bool result = chain.Execute(context, Chain.ExecutionMethod.AndChain);
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