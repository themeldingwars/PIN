using GameServer.StaticDB.Records.apt;

namespace GameServer.Systems.Aptitude.Commands.Logic;

public class LogicOrCommand : Command, ICommand
{
    private LogicOrCommandDef Params;

    public LogicOrCommand(LogicOrCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        var chainA = context.Abilities.Factory.LoadChain(Params.AChain);
        var chainB = context.Abilities.Factory.LoadChain(Params.BChain);

        var prevExecutionHint = context.ExecutionHint;
        context.ExecutionHint = ExecutionHint.Logic;
        chainA.Execute(context, ref result);

        if (!result.Success)
        {
            chainB.Execute(context, ref result);
        }

        context.ExecutionHint = prevExecutionHint;

        return;
    }
}