using System.Collections.Concurrent;
using System.Collections.Generic;

namespace GameServer.Entities;

public class BaseEntity : IEntity
{
    public BaseEntity(IShard shard, ulong id)
    {
        Shard = shard;
        EntityId = id;
        ControllerRefMap = new ConcurrentDictionary<Enums.GSS.Controllers, ushort>();
    }

    public ulong EntityId { get; }
    public IShard Shard { get; }
    public IDictionary<Enums.GSS.Controllers, ushort> ControllerRefMap { get; }

    public InteractionComponent Interaction { get; set; }

    public virtual bool IsInteractable()
    {
        return false;
    }

    public virtual bool CanBeInteractedBy(IEntity other)
    {
        return false;
    }

    public byte GetInteractionType()
    {
        return (Interaction != null) ? (byte)Interaction.Type : (byte)0;
    }

    public uint GetInteractionDuration()
    {
        return (Interaction != null) ? Interaction.DurationMs : 0;
    }

    public void RegisterController(Enums.GSS.Controllers controller)
    {
        ControllerRefMap.Add(controller, Shard.AssignNewRefId(this, controller));
    }
}