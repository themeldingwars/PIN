using GameServer.Data.SDB.Records.apt;

namespace GameServer.Aptitude;

public class ImpactToggleEffectCommand : ICommand
{
    private ImpactToggleEffectCommandDef Params;

    public ImpactToggleEffectCommand(ImpactToggleEffectCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        Context effectContext = new Context(context.Shard, context.Initiator)
        {
            InitTime = context.InitTime,
            ExecutionHint = ExecutionHint.ApplyEffect
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
                    context.Abilities.DoRemoveEffect(active);
                    break;
                }
            }

            if (targetHasEffect)
            {
                continue;
            }

            if (Params.PreApplyChain != 0)
            {
                var chain = context.Abilities.Factory.LoadChain(Params.PreApplyChain);
                chain.Execute(new Context(context.Shard, context.Initiator)
                              {
                                  ChainId = Params.PreApplyChain,
                                  Targets = new AptitudeTargets(target),
                                  InitTime = context.InitTime,
                                  ExecutionHint = ExecutionHint.Proximity
                              });
            }

            context.Abilities.DoApplyEffect(Params.EffectId, target, effectContext);
        }

        return true;
    }
}
