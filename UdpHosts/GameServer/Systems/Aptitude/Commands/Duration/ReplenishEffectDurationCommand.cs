using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Duration;

public class ReplenishEffectDurationCommand : Command, ICommand
{
    private ReplenishEffectDurationCommandDef Params;

    public ReplenishEffectDurationCommand(ReplenishEffectDurationCommandDef par)
        : base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        result.SetPass();
        if (Params?.EffectId is not { } effectId)
        {
            return;
        }

        if (context.Targets.Count > 0)
        {
            foreach (IAptitudeTarget target in context.Targets)
            {
                Replenish(target, effectId, context.CurrentTime);
            }
        }
        else
        {
            Replenish(context.Self, effectId, context.CurrentTime);
        }
    }

    private static void Replenish(IAptitudeTarget target, uint effectId, uint time)
    {
        if (target == null)
        {
            return;
        }

        foreach (var state in target.GetActiveEffects())
        {
            if (state?.Context != null && state.Effect.Id == effectId)
            {
                state.Context.ReplenishedTime = time;
            }
        }
    }
}