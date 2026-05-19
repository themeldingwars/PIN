using GameServer.StaticDB.Records.apt;

namespace GameServer.Systems.Aptitude.Commands.Logic;

public class LogicNegateCommand : Command, ICommand
{
    private LogicNegateCommandDef Params;

    public LogicNegateCommand(LogicNegateCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        var chain = context.Abilities.Factory.LoadChain(Params.NegateChain);

        var prevExecutionHint = context.ExecutionHint;
        context.ExecutionHint = ExecutionHint.Logic;
        chain.Execute(context, ref result);
        context.ExecutionHint = prevExecutionHint;

        if (result.Success)
        {
            result.SetFail();
        }
        else
        {
            result.SetPass();
        }

        return;
    }
}