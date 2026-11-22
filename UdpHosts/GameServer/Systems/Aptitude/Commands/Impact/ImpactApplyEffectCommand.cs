using System.Collections.Generic;
using System.Linq;
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

    public override void Execute(Context context, ref CommandResult result)
    {
        Context effectContext = new Context(context.Shard, context.Initiator, context.Self)
        {
            ExecutionId = context.ExecutionId,
            CurrentTime = context.CurrentTime,
            ExecutionHint = ExecutionHint.ApplyEffect,
        };

        // The constructor took the pose at the shard's time, but the effect starts at the parent's time
        effectContext.SnapshotInitPose();

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

        var applied = new List<EffectState>();
        if (Params.ApplyToSelf == 1)
        {
            applied.Add(context.Abilities.DoApplyEffect(Params.EffectId, context.Self, effectContext));
        }
        else
        {
            foreach (IAptitudeTarget target in context.Targets)
            {
                applied.Add(context.Abilities.DoApplyEffect(Params.EffectId, target, effectContext));
            }
        }

        if (Params.RemoveOnRollback == 1)
        {
            if (context.CommandState.TryGetValue(Id, out var existing) && existing is List<EffectState> earlier)
            {
                earlier.AddRange(applied.Where(state => state != null));
            }
            else
            {
                context.CommandState[Id] = applied.Where(state => state != null).ToList();
            }
        }

        result.SetPass();
        return;
    }

    public override void Reset(Context context)
    {
        if (!context.CommandState.Remove(Id, out var state) || state is not List<EffectState> applied)
        {
            return;
        }

        foreach (var effect in applied)
        {
            if (AbilitySystem.IsActive(effect))
            {
                Logger.Information("RemoveOnRollback of {Command} {CommandId} triggers removal of {EffectId}", nameof(ImpactApplyEffectCommand), Params.Id, Params.EffectId);
                context.Abilities.DoRemoveEffect(effect, context.CurrentTime);
            }
        }
    }
}