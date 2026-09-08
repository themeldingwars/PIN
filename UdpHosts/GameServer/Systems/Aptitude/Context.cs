using System;
using System.Collections.Generic;
using System.Numerics;

namespace GameServer.Systems.Aptitude;

public class Context
{
    public Context(IShard shard, IAptitudeTarget initiator)
    {
        Shard = shard;
        Initiator = initiator;
        ActivationInitiator = initiator;
        Self = initiator;
        Abilities = shard.Abilities;
        Targets = new AptitudeTargets();
        FormerTargets = new AptitudeTargets();
        InitPosition = initiator.Position;
        ExecutionId = Guid.NewGuid();
    }

    public uint ChainId { get; set; }
    public uint AbilityId { get; set; }

    /// <summary>
    /// The <c>dbitems::AbilityModule</c> that activated the chain (when
    /// resolved from the loadout slot). Used by
    /// <c>LoadRegisterFromModulePowerCommand</c> to load the ability's power
    /// rating, which scales data amounts like energy cost and damage.
    /// </summary>
    public uint AbilityModuleId { get; set; }

    public bool Success { get; set; }
    public IShard Shard { get; set; }
    public AbilitySystem Abilities { get; set; }
    public IAptitudeTarget Self { get; set; }
    public IAptitudeTarget Initiator { get; set; }

    /// <summary>
    /// The immutable caster identity for the root activation. <see cref="Initiator"/>
    /// can be overridden by effect commands for aptitude semantics, so
    /// activation cooldowns must use this value instead.
    /// </summary>
    public IAptitudeTarget ActivationInitiator { get; set; }

    public AptitudeTargets Targets { get; set; }
    public AptitudeTargets FormerTargets { get; set; }
    public Stack<AptitudeTargets> TargetStack { get; set; } = new();
    public float Register { get; set; }
    public float FormerRegister { get; set; }
    public int Bonus { get; set; }
    public uint InitTime { get; set; }
    public Vector3 InitPosition { get; set; }
    public ExecutionHint ExecutionHint { get; set; }
    public Guid ExecutionId { get; set; }

    public Dictionary<ICommand, ICommandActiveContext> Actives { get; set; } = [];

    /// <summary>
    /// Cooldowns queued by activation commands while the chain runs. The
    /// AbilitySystem starts them once the whole chain has succeeded, so a
    /// chain that fails a later requirement (e.g. not enough energy) does not
    /// consume the cooldown.
    /// </summary>
    public List<AbilityCooldownRequest> PendingCooldowns { get; set; } = [];

    /// <summary>
    /// When set, every effect the chain applies is recorded here. The proximity handling uses it to know
    /// whether a previous activation of the same client proximity command is still in effect, so a client
    /// that keeps re-sending the success message while the player stands on the trigger does not re-run the
    /// whole chain (re-applying effects, spawning projectiles and flushing status effect fields to everyone
    /// in range) several times a second.
    /// </summary>
    public List<AppliedEffectRecord> AppliedEffects { get; set; }

    public static Context CopyContext(Context original)
    {
        return new Context(original.Shard, original.Initiator)
        {
            ChainId = original.ChainId,
            AbilityId = original.AbilityId,
            AbilityModuleId = original.AbilityModuleId,
            Success = original.Success,
            Shard = original.Shard,
            Abilities = original.Abilities,
            Self = original.Self,
            Initiator = original.Initiator,
            ActivationInitiator = original.ActivationInitiator,
            Targets = original.Targets,
            FormerTargets = original.FormerTargets,
            TargetStack = original.TargetStack,
            Register = original.Register,
            FormerRegister = original.FormerRegister,
            Bonus = original.Bonus,
            InitTime = original.InitTime,
            InitPosition = original.InitPosition,
            ExecutionHint = original.ExecutionHint,
            ExecutionId = original.ExecutionId,
            PendingCooldowns = original.PendingCooldowns,
            AppliedEffects = original.AppliedEffects,
        };
    }

    /*
    public uint NamedVar;
    public uint Interaction;
    public uint SourceContext;
    public uint SourceEffect;
    */
}