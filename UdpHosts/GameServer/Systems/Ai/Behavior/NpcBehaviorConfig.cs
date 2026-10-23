#nullable enable
namespace GameServer.Systems.Ai.Behavior;

public enum NpcBehaviorMode
{
    Base,
    Offensive,
    Defensive,
}

/// <summary>
///     The behavior definitions for an NPC, plus which set is currently active. Each slot is resolved from its text
///     column with the (opaque) instance id as fallback (text wins unless it is empty or a <c>_inst*</c> sentinel —
///     see <see cref="BehaviorInstanceMap" />). <see cref="Current" /> drives the behavior state machine.
/// </summary>
public sealed class NpcBehaviorConfig
{
    public NpcBehaviorConfig(
        string? baseText,
        uint baseInstanceId,
        string? offensiveText,
        uint offensiveInstanceId,
        string? defensiveText,
        uint defensiveInstanceId)
    {
        BaseInstanceId = baseInstanceId;
        OffensiveInstanceId = offensiveInstanceId;
        DefensiveInstanceId = defensiveInstanceId;

        Base = BehaviorInstanceMap.Resolve(baseText, baseInstanceId);
        var offensive = BehaviorInstanceMap.Resolve(offensiveText, offensiveInstanceId);
        var defensive = BehaviorInstanceMap.Resolve(defensiveText, defensiveInstanceId);

        // An empty slot stays null (so "has an offensive behavior" is meaningful for combat-capability checks).
        Offensive = offensive.IsEmpty ? null : offensive;
        Defensive = defensive.IsEmpty ? null : defensive;

        BaseResolved = BehaviorResolver.Resolve(Base);
        OffensiveResolved = Offensive is not null ? BehaviorResolver.Resolve(Offensive) : null;
        DefensiveResolved = Defensive is not null ? BehaviorResolver.Resolve(Defensive) : null;
    }

    public ParsedBehavior Base { get; }
    public ParsedBehavior? Offensive { get; }
    public ParsedBehavior? Defensive { get; }
    public NpcBehaviorMode ActiveMode { get; set; } = NpcBehaviorMode.Base;

    public ParsedBehavior Current => ActiveMode switch
    {
        NpcBehaviorMode.Offensive => Offensive ?? Base,
        NpcBehaviorMode.Defensive => Defensive ?? Base,
        _ => Base,
    };

    // The raw instance ids (for logging/diagnostics; the resolved behaviors above already fold them in).
    public uint BaseInstanceId { get; }
    public uint OffensiveInstanceId { get; }
    public uint DefensiveInstanceId { get; }

    // The same three behaviors resolved into shared parameter blocks (cadence / spacing / ability-moves / perception /
    // wander) plus a coarse style tag. The behavior system reads these to drive movement goals; unknown params are ignored.
    public ResolvedBehavior BaseResolved { get; }
    public ResolvedBehavior? OffensiveResolved { get; }
    public ResolvedBehavior? DefensiveResolved { get; }

    public ResolvedBehavior CurrentResolved => ActiveMode switch
    {
        NpcBehaviorMode.Offensive => OffensiveResolved ?? BaseResolved,
        NpcBehaviorMode.Defensive => DefensiveResolved ?? BaseResolved,
        _ => BaseResolved,
    };
}
