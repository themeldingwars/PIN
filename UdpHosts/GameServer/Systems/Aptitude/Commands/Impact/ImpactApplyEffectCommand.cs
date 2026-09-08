using System.Collections.Generic;
using GameServer.StaticDB.Records.apt;

namespace GameServer.Systems.Aptitude.Commands.Impact;

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
        // Effects are part of the activation graph. Copy the complete
        // activation context so nested energy spends, module power, and
        // deferred cooldowns remain associated with the root caster.
        Context effectContext = Context.CopyContext(context);
        effectContext.Targets = new AptitudeTargets();
        effectContext.FormerTargets = new AptitudeTargets();
        effectContext.TargetStack = new Stack<AptitudeTargets>();
        effectContext.ExecutionHint = ExecutionHint.ApplyEffect;

        // Copy activation identity/cooldowns, not the optional payload. CopyContext already copied these
        // values, so merely assigning them when a Pass* flag is set accidentally passed them unconditionally.
        effectContext.InitPosition = Params.InheritInitPos == 1 ? context.InitPosition : context.Initiator.Position;
        effectContext.Register = Params.PassRegister == 1 ? context.Register : float.NaN;
        effectContext.FormerRegister = float.NaN;
        effectContext.Bonus = Params.PassBonus == 1 ? context.Bonus : 0;

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

            return context.Abilities.DoApplyEffect(Params.EffectId, context.Self, effectContext);
        }

        if (Params.RemoveOnRollback == 1)
        {
            context.Actives.Add(this, new RemoveOnRollbackCommandActiveContext()
            {
                Targets = context.Targets
            });
        }

        foreach (IAptitudeTarget target in context.Targets)
        {
            if (!context.Abilities.DoApplyEffect(Params.EffectId, target, effectContext))
            {
                return false;
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