﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using AeroMessages.Common;
using BepuPhysics;

namespace GameServer.Entities;

public class BaseEntity : IEntity
{
    public BaseEntity(IShard shard, ulong id)
    {
        Shard = shard;
        EntityId = id;
        AeroEntityId = new EntityId() { Backing = EntityId, ControllerId = Controller.Generic };
    }

    public ulong EntityId { get; }
    public EntityId AeroEntityId { get; protected set; }
    public IShard Shard { get; }
    public Vector3 Position { get; set; }
    public BodyHandle BodyHandle { get; set; }

    public InteractionComponent Interaction { get; set; }
    public ScopingComponent Scoping { get; set; }
    public EncounterComponent Encounter { get; set; }

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

    public bool IsGlobalScope()
    {
        return (Scoping != null) && Scoping.Global;
    }

    public float GetScopeRange()
    {
        return (Scoping != null) ? Scoping.Range : 100f;
    }
}