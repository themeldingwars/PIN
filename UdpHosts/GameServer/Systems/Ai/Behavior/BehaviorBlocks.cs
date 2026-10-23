#nullable enable
using System.Collections.Generic;

namespace GameServer.Systems.Ai.Behavior;

/// <summary>Firing cadence: how often the NPC pulls the trigger and how long it rests between bursts.</summary>
public sealed class CombatCadence
{
    public float? TriggerPullTime { get; set; }
    public float? FireRestDuration { get; set; }
}

/// <summary>Target spacing: the standoff distance the NPC tries to hold from its target.</summary>
public sealed class Spacing
{
    public float? CombatDist { get; set; }
    public float? MinDist { get; set; }
    public float? PreferredMinCombatDist { get; set; }
    public float? MaxMove { get; set; }
    public float? MoveChance { get; set; }
}

/// <summary>A special ability/attack the NPC can trigger (the am1/am2 "ability move" parameter family).</summary>
public sealed class AbilityMove
{
    public int Slot { get; set; }
    public int Id { get; set; }
    public float? Chance { get; set; }
    public float? Cooldown { get; set; }
    public float? Timeout { get; set; }
    public float? MaxDist { get; set; }
    public float? MinDist { get; set; }
    public float? NavToDist { get; set; }
    public bool? Targeted { get; set; }
    public bool? Facing { get; set; }
}

/// <summary>Detection / aggro / target-selection ranges.</summary>
public sealed class Perception
{
    public float? PerceptionDist { get; set; }
    public float? AggroDistance { get; set; }
    public float? TargetSelectDist { get; set; }
}

/// <summary>Roam parameters (map onto the Wander movement mode).</summary>
public sealed class WanderParams
{
    public bool? NearSpawn { get; set; }
    public float? Distance { get; set; }
    public float? MoveChance { get; set; }
    public float? PauseDuration { get; set; }
}

/// <summary>
///     A behavior resolved into its shared parameter blocks by <see cref="BehaviorResolver" />. Unknown parameter
///     names are ignored here (they remain on the source <see cref="ParsedBehavior" />).
/// </summary>
public sealed class ResolvedBehavior
{
    public ResolvedBehavior(string name, BehaviorStyle style, IReadOnlyList<AbilityMove> abilities)
    {
        Name = name;
        Style = style;
        Abilities = abilities;
    }

    public string Name { get; }
    public BehaviorStyle Style { get; }
    public CombatCadence Cadence { get; } = new();
    public Spacing Spacing { get; } = new();
    public Perception Perception { get; } = new();
    public WanderParams Wander { get; } = new();
    public IReadOnlyList<AbilityMove> Abilities { get; }

    public bool IsEmpty => Name.Length == 0;
}
