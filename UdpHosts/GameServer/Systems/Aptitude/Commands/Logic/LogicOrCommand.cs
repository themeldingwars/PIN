using GameServer.Data.SDB.Records.apt;

namespace GameServer.Aptitude;

public class LogicOrCommand : Command, ICommand
{
    private LogicOrCommandDef Params;

    public LogicOrCommand(LogicOrCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        var chainA = context.Abilities.Factory.LoadChain(Params.AChain);
        var chainB = context.Abilities.Factory.LoadChain(Params.BChain);

        var prevExecutionHint = context.ExecutionHint;
        context.ExecutionHint = ExecutionHint.Logic;
        bool result = chainA.Execute(context);

        if (result == false)
        {
            result = chainB.Execute(context);
        }

        context.ExecutionHint = prevExecutionHint;

        return result;
    }
}