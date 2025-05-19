using GameServer.Data.SDB.Records.apt;

namespace GameServer.Aptitude;

public class ImpactToggleEffectCommand : Command, ICommand
{
    private ImpactToggleEffectCommandDef Params;

    public ImpactToggleEffectCommand(ImpactToggleEffectCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        Context effectContext = new Context(context.Shard, context.Initiator)
        {
            InitTime = context.InitTime,
        };

        if (Params.PassRegister == 1)
        {
            effectContext.Register = context.Register;
        }

        if (Params.PassBonus == 1)
        {
            effectContext.Bonus = context.Bonus;
        }

        foreach (IAptitudeTarget target in context.Targets)
        {
            bool targetHasEffect = false;
            foreach (EffectState active in target.GetActiveEffects())
            {
                if (active == null)
                {
                    continue;
                }

                if (active.Effect.Id == Params.EffectId)
                {
                    targetHasEffect = true;
                    effectContext.ExecutionHint = ExecutionHint.RemoveEffect;
                    effectContext.Abilities.DoRemoveEffect(active);
                    break;
                }
            }

            if (targetHasEffect)
            {
                continue;
            }

            if (Params.PreApplyChain != 0)
            {
                var chain = effectContext.Abilities.Factory.LoadChain(Params.PreApplyChain);
                chain.Execute(effectContext);
            }

            effectContext.ExecutionHint = ExecutionHint.ApplyEffect;
            effectContext.Abilities.DoApplyEffect(Params.EffectId, target, effectContext);
        }

        return true;
    }
}