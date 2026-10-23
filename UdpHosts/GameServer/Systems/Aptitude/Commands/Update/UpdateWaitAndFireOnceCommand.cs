using GameServer.StaticDB.Records.apt;

namespace GameServer.Systems.Aptitude.Commands.Update;

public class UpdateWaitAndFireOnceCommand : Command, ICommand
{
    private UpdateWaitAndFireOnceCommandDef Params;

    public UpdateWaitAndFireOnceCommand(UpdateWaitAndFireOnceCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        var delay = AbilitySystem.RegistryOp(context.Register, Params.Duration, (Enums.Operand)Params.Regop);

        if (delay <= 0 || Params.Chain == 0)
        {
            result.SetPass();
            return;
        }

        if (context.InitTime is not { } timeInitiated)
        {
            Logger.Debug("UpdateWaitAndFireOnceCommand {CommandId} has no initiation time to delay from", Id);
            result.SetPass();
            return;
        }

        var fireAt = (uint)(timeInitiated + delay);

        if (context.CurrentTime <= fireAt)
        {
            result.SetPass();
            return;
        }

        // Marks the command as fired, Add fails if it already ran in this context. Marking before executing keeps a
        // nested chain from re-entering this command.
        if (!context.FiredOnceCommandIds.Add(Id))
        {
            result.SetPass();
            return;
        }

        var chain = context.Abilities.Factory.LoadChain(Params.Chain);

        var previousTime = context.CurrentTime;
        var previousHint = context.ExecutionHint;

        // The update chain only runs every UpdateFrequency ms, so run the sub chain at the time it was meant to fire at.
        // Its result is kept out of the update chain result, a failing sub chain should not end the effect.
        context.CurrentTime = fireAt;
        context.ExecutionHint = ExecutionHint.Logic;

        var chainResult = new CommandResult { Success = true };
        chain.Execute(context, ref chainResult);

        context.CurrentTime = previousTime;
        context.ExecutionHint = previousHint;

        result.SetPass();
        return;
    }

    public override void Reset(Context context)
    {
        context.FiredOnceCommandIds.Remove(Id);
    }
}