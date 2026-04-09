using GameServer.Data.SDB.Records.apt;

namespace GameServer.Aptitude;

public class ImpactApplyEffectCommand : Command, ICommand
{
    private ImpactApplyEffectCommandDef Params;

    public ImpactApplyEffectCommand(ImpactApplyEffectCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        Context effectContext = new Context(context.Shard, context.Initiator)
        {
            ExecutionId = context.ExecutionId,
            InitTime = context.InitTime,
            ExecutionHint = ExecutionHint.ApplyEffect
        };

        if (Params.InheritInitPos == 1)
        {
            effectContext.InitPosition = context.InitPosition;
        }

        // Ability 40133. Teleportal Beacon Interact Ability ; player targeted. Add the other beacon to target list, then apply status effect to player, passing the target.
        if (Params.PassTargets == 1)
        {
            if (Params.UseFormer == 1)
            {
                effectContext.Targets = context.FormerTargets;
            }
            else
            {
                effectContext.Targets = context.Targets;
            }
        }

        if (Params.PassRegister == 1)
        {
            effectContext.Register = context.Register;
        }

        if (Params.PassBonus == 1)
        {
            effectContext.Bonus = context.Bonus;
        }

        if (Params.OverrideInitiator == 1)
        {
            // TODO: With who?
            effectContext.Initiator = context.Self;
        }
        else if (Params.OverrideInitiatorWithTarget == 1)
        {
            if (Params.UseFormer == 1)
            {
                if (context.FormerTargets.TryPeek(out var initiator))
                {
                    effectContext.Initiator = initiator;
                }
                else
                {
                    Logger.Warning("{Command} {CommandId} (effect {EffectId}) specifies OverrideInitiatorWithTarget but there are no targets", nameof(ImpactApplyEffectCommand), Params.Id, Params.EffectId);
                }
            }
            else
            {
                if (context.Targets.TryPeek(out var initiator))
                {
                    effectContext.Initiator = initiator;
                }
                else
                {
                    Logger.Warning("{Command} {CommandId} (effect {EffectId}) specifies OverrideInitiatorWithTarget but there are no targets", nameof(ImpactApplyEffectCommand), Params.Id, Params.EffectId);
                }
            }
        }

        if (Params.RemoveOnRollback == 1)
        {
            Logger.Debug("{Command} {CommandId} (effect {EffectId}) specifies it should be RemovedOnRollback", nameof(ImpactApplyEffectCommand), Params.Id, Params.EffectId);
        }

        if (Params.ApplyToSelf == 1)
        {
            if (Params.RemoveOnRollback == 1)
            {
                context.Actives.Add(this, new RemoveOnRollbackCommandActiveContext()
                {
                    Targets = new(context.Self)
                });
            }

            context.Abilities.DoApplyEffect(Params.EffectId, context.Self, effectContext);
        }
        else
        {
            if (Params.RemoveOnRollback == 1)
            {
                context.Actives.Add(this, new RemoveOnRollbackCommandActiveContext()
                {
                    Targets = context.Targets
                });
            }

            foreach (IAptitudeTarget target in context.Targets)
            {
                context.Abilities.DoApplyEffect(Params.EffectId, target, effectContext);
            }
        }

        return true;
    }

    public void OnRemove(Context context, ICommandActiveContext activeCommandContext)
    {
        var rollbackContext = (RemoveOnRollbackCommandActiveContext)activeCommandContext;
        foreach (IAptitudeTarget target in rollbackContext.Targets)
        {
            Logger.Information("RemoveOnRollback of {Command} {CommandId} triggers removal of {EffectId}", nameof(ImpactApplyEffectCommand), Params.Id, Params.EffectId);
            context.Abilities.DoRemoveEffect(target, Params.EffectId);
        }
    }
}

public class RemoveOnRollbackCommandActiveContext : ICommandActiveContext
{
    public AptitudeTargets Targets { get; set; }
}