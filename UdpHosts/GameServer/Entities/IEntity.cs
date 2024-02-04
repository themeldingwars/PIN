using System.Collections.Generic;
using AeroMessages.Common;

namespace GameServer.Entities;

public interface IEntity
{
    ulong EntityId { get; }
    EntityId AeroEntityId { get; }
    IShard Shard { get; }
    IDictionary<Enums.GSS.Controllers, ushort> ControllerRefMap { get; }

    bool IsInteractable();
    bool CanBeInteractedBy(IEntity other);
    byte GetInteractionType();
    uint GetInteractionDuration();

    void RegisterController(Enums.GSS.Controllers controller);
}