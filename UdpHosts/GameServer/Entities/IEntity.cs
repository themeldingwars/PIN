using System.Collections.Generic;

namespace GameServer.Entities;

public interface IEntity
{
    ulong EntityId { get; }
    IShard Shard { get; }
    IDictionary<Enums.GSS.Controllers, ushort> ControllerRefMap { get; }

    bool IsInteractable();

    void RegisterController(Enums.GSS.Controllers controller);
}