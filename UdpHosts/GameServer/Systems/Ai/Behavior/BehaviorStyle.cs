#nullable enable
namespace GameServer.Systems.Ai.Behavior;

/// <summary>
///     Coarse combat/locomotion classification of a behavior, derived from its name. It is only a tag: behaviors are
///     composed from shared parameter blocks (see <see cref="ResolvedBehavior" />), not a class hierarchy. The style
///     decides which blocks matter and how the behavior maps onto a movement mode (ranged hold, melee approach, wander, idle).
/// </summary>
public enum BehaviorStyle
{
    Unknown,
    Ranged,
    MeleeAdditive,
    MeleeFullbody,
    Melee,
    Charger,
    Kamikaze,
    Follower,
    Wanderer,
    Idle,
    Interaction,
}
