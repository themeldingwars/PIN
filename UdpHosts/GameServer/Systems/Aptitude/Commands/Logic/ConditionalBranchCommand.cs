using GameServer.StaticDB.Records.apt;

namespace GameServer.Systems.Aptitude.Commands.Logic;

public class ConditionalBranchCommand : Command, ICommand
{
    private ConditionalBranchCommandDef Params;

    public ConditionalBranchCommand(ConditionalBranchCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        var prevExecutionHint = context.ExecutionHint;
        context.ExecutionHint = ExecutionHint.Logic;

        var conditionChain = context.Abilities.Factory.LoadChain(Params.IfChain);
        var conditionResult = new CommandResult { Success = true };
        conditionChain.Execute(context, ref conditionResult);

        if (conditionResult.Success && Params.ThenChain != 0)
        {
            var thenChain = context.Abilities.Factory.LoadChain(Params.ThenChain);
            thenChain.Execute(context, ref result);
        }

        if (!conditionResult.Success && Params.ElseChain != 0)
        {
            var elseChain = context.Abilities.Factory.LoadChain(Params.ElseChain);
            elseChain.Execute(context, ref result);
        }

        context.ExecutionHint = prevExecutionHint;

        return;
    }
}