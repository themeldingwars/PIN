using System;
using System.Collections.Generic;
using System.Numerics;
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
        InitPosition = initiator.Position;
        CurrentTime = shard.CurrentTime;
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
    /// The time the context got initiated at, which durations are measured from.
    /// Not set until an initiation command ran or the context got applied as an effect.
    /// </summary>
    public uint? InitTime { get; set; }

    /// <summary>
    /// The time the running chain executes at, e.g. the activation time sent by the client or the tick an effect gets updated in.
    /// </summary>
    public uint CurrentTime { get; set; }

    public Vector3 InitPosition { get; set; }
    public ExecutionHint ExecutionHint { get; set; }
    public Guid ExecutionId { get; set; }
    public CommandResult PreviousResult { get; set; }

    public Dictionary<ICommand, ICommandActiveContext> Actives { get; set; } = [];

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
            InitTime = original.InitTime,
            CurrentTime = original.CurrentTime,
            InitPosition = original.InitPosition,
            ExecutionHint = original.ExecutionHint,
            ExecutionId = original.ExecutionId,
        };
    }

    /*
    public uint NamedVar;
    public uint Interaction;
    public uint SourceContext;
    public uint SourceEffect;
    */
}