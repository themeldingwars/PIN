using System.Collections.Generic;
using System.Numerics;
using AeroMessages.Common;

namespace GameServer.Entities;

public interface IEntity
{
    ulong EntityId { get; }
    EntityId AeroEntityId { get; }
    IShard Shard { get; }
    Vector3 Position { get; set; }

    bool IsInteractable();
    bool CanBeInteractedBy(IEntity other);
    byte GetInteractionType();
    uint GetInteractionDuration();

    bool IsGlobalScope();
    float GetScopeRange();
}