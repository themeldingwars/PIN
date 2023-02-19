using System.Collections.Concurrent;
using System.Collections.Generic;

namespace GameServer.Entities;

public class BaseEntity : IEntity
{
    public BaseEntity(IShard owner, ulong id)
    {
        Owner = owner;
        EntityId = id;
        ControllerRefMap = new ConcurrentDictionary<Enums.GSS.Controllers, ushort>();
    }

    public ulong EntityId { get; }
    public IShard Owner { get; }
    public IDictionary<Enums.GSS.Controllers, ushort> ControllerRefMap { get; }

    public void RegisterController(Enums.GSS.Controllers controller)
    {
        ControllerRefMap.Add(controller, Owner.AssignNewRefId(this, controller));
    }
}