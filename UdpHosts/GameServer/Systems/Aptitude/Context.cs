using System;
using System.Collections.Generic;
using System.Numerics;
using BepuUtilities;
using GameServer.Entities;
using GameServer.Entities.Character;
using GameServer.Enums;

namespace GameServer.Systems.Aptitude;

public class Context
{
    public Context(IShard shard, IAptitudeTarget initiator)
    {
        Shard = shard;
        Initiator = initiator;
        Self = initiator;
        Abilities = shard.Abilities;
        Targets = new AptitudeTargets();
        FormerTargets = new AptitudeTargets();
        CurrentTime = shard.CurrentTime;
        InitPosition = initiator.Position;
        SnapshotInitPose();
        ExecutionId = Guid.NewGuid();
    }

    public uint ChainId { get; set; }
    public uint AbilityId { get; set; }
    public bool Success { get; set; }
    public IShard Shard { get; set; }
    public AbilitySystem Abilities { get; set; }
    public IAptitudeTarget Self { get; set; }
    public IAptitudeTarget Initiator { get; set; }
    public AptitudeTargets Targets { get; set; }
    public AptitudeTargets FormerTargets { get; set; }
    public Stack<AptitudeTargets> TargetStack { get; set; } = new();
    public float Register { get; set; }
    public float FormerRegister { get; set; }
    public int Bonus { get; set; }

    /// <summary>
    ///     tfSetWeaponDamageCommand recomputes it from the weapon's damage curve
    ///     apt::StatusEffect_Apply fills it from the damage that came with the effect application request.
    /// </summary>
    public int DamagePoints { get; set; }

    /// <summary>
    ///     Maybe set for weapon abilities
    /// </summary>
    public byte DamageType { get; set; }

    /// <summary>
    ///     A size multiplier the commands that flag it scale their volumes by. Client apt_context +0x82c, zeroed
    ///     apt::StatusEffect_Apply fills it from the application request, where
    ///     the request's default is 1.0.
    /// </summary>
    public float ScaleMultiplier { get; set; }

    /// <summary>
    /// The time the context got initiated at, which durations are measured from.
    /// Not set until an initiation command ran or the context got applied as an effect.
    /// </summary>
    public uint? InitTime { get; set; }

    /// <summary>
    /// The time the running chain executes at, e.g. the activation time sent by the client or the tick an effect gets updated in.
    /// </summary>
    public uint CurrentTime { get; set; }

    public Vector3 InitPosition { get; set; }

    public Vector3 InitBodyPosition { get; set; }
    public Vector3 InitAimOrigin { get; set; }
    public Vector3 InitAimDirection { get; set; }

    public ExecutionHint ExecutionHint { get; set; }
    public Guid ExecutionId { get; set; }
    public CommandResult PreviousResult { get; set; }

    public Dictionary<ICommand, ICommandActiveContext> Actives { get; set; } = [];

    /// <summary>
    /// Ids of commands that already ran once inside this context, used by fire once commands such as
    /// UpdateWaitAndFireOnceCommand. Lives on the context because command instances get rebuilt per chain load.
    /// </summary>
    public HashSet<uint> FiredOnceCommandIds { get; } = [];

    public Dictionary<AptitudeStat, ActiveStatModifier> StatChangelist { get; } = new();

    public static Context CopyContext(Context original)
    {
        return new Context(original.Shard, original.Initiator)
        {
            ChainId = original.ChainId,
            AbilityId = original.AbilityId,
            Success = original.Success,
            Shard = original.Shard,
            Abilities = original.Abilities,
            Self = original.Self,
            Initiator = original.Initiator,
            Targets = original.Targets,
            FormerTargets = original.FormerTargets,
            TargetStack = original.TargetStack,
            Register = original.Register,
            Bonus = original.Bonus,
            DamagePoints = original.DamagePoints,
            DamageType = original.DamageType,
            ScaleMultiplier = original.ScaleMultiplier,
            InitTime = original.InitTime,
            CurrentTime = original.CurrentTime,
            InitPosition = original.InitPosition,
            InitBodyPosition = original.InitBodyPosition,
            InitAimOrigin = original.InitAimOrigin,
            InitAimDirection = original.InitAimDirection,
            ExecutionHint = original.ExecutionHint,
            ExecutionId = original.ExecutionId,
        };
    }

    /// <summary>
    ///     Takes the pose snapshot the client's apt_context_SetInitPos takes when a context is initiated: the
    ///     body position, the aim/eye position and the aim direction, all read from the caster's pose at the
    ///     context's current time.
    /// </summary>
    private void SnapshotInitPose()
    {
        if (Self is CharacterEntity character)
        {
            character.TryGetInterpolatedPose(CurrentTime, null, out var bodyPosition, out _, out _);
            InitBodyPosition = bodyPosition;
            InitAimDirection = character.TryGetAimSample(CurrentTime, out var sampledAim) ? sampledAim : character.AimDirection;
            InitAimOrigin = character.GetProjectileOrigin(CurrentTime, InitAimDirection);
            return;
        }

        // Deployables, turrets and tiny objects have no pose buffer of their own, so their live transform is
        // both pose slots and their facing stands in for an aim direction.
        InitBodyPosition = Self.Position;
        InitAimOrigin = Self.Position;
        InitAimDirection = Self is BaseEntity entity
            ? QuaternionEx.Transform(Vector3.UnitY, QuaternionEx.Inverse(entity.Orientation))
            : Vector3.UnitY;
    }

    /*
    public uint NamedVar;
    public uint Interaction;
    public uint SourceContext;
    public uint SourceEffect;
    */
}