using System.Numerics;
using AeroMessages.Common;
using AeroMessages.GSS;
using GameServer.Systems.Combat;

namespace GameServer.Entities;

public interface IEntity
{
    ulong EntityId { get; }
    EntityId AeroEntityId { get; }
    IShard Shard { get; }
    Vector3 Position { get; set; }
    Quaternion Orientation { get; set; }
    HostilityInfoData HostilityInfo { get; set; }

    /// <summary>
    ///     The entity's own stance towards each faction, on top of its faction. Null when the entity has none,
    ///     which is what makes a player vs NPC pair resolve from the player's stance instead of the faction table.
    /// </summary>
    PersonalFactionStance? PersonalFactionStance { get; set; }

    byte DamageResponse { get; }

    bool IsInteractable();
    bool CanBeInteractedBy(IEntity other);
    byte GetInteractionType();
    uint GetInteractionDuration();

    bool IsGlobalScope();
    float GetScopeRange();
}