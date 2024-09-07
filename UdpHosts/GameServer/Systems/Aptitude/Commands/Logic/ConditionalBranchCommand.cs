using GameServer.Data.SDB.Records.apt;

namespace GameServer.Aptitude;

public class ConditionalBranchCommand : Command, ICommand
{
    private ConditionalBranchCommandDef Params;

    public ConditionalBranchCommand(ConditionalBranchCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        var prevExecutionHint = context.ExecutionHint;
        context.ExecutionHint = ExecutionHint.Logic;

        var conditionChain = context.Abilities.Factory.LoadChain(Params.IfChain);
        var conditionResult = conditionChain.Execute(context);
        bool success = true;
        if (conditionResult && Params.ThenChain != 0)
        {
            var thenChain = context.Abilities.Factory.LoadChain(Params.ThenChain);
            success = thenChain.Execute(context);
        }

        if (!conditionResult && Params.ElseChain != 0)
        {
            var elseChain = context.Abilities.Factory.LoadChain(Params.ElseChain);
            success = elseChain.Execute(context);
        }

        context.ExecutionHint = prevExecutionHint;

        return success;
    }
}