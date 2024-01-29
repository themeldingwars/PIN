using GameServer.Data.SDB.Records.apt;

namespace GameServer.Aptitude;

public class LogicNegateCommand : ICommand
{
    private LogicNegateCommandDef Params;

    public LogicNegateCommand(LogicNegateCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        var chain = context.Abilities.Factory.LoadChain(Params.NegateChain);

        var prevExecutionHint = context.ExecutionHint;
        context.ExecutionHint = ExecutionHint.Logic;
        var result = chain.Execute(context);
        context.ExecutionHint = prevExecutionHint;

        result = !result;

        return result;
    }
}