using System;
using System.Collections.Generic;
using System.Linq;
using Aero.Gen;
using AeroMessages.Common;
using AeroMessages.GSS.V66.Generic;

namespace GameServer.Systems.Encounters;

public abstract class BaseEncounter : IEncounter
{
    protected static readonly Random Rng = new();

    protected BaseEncounter(IShard shard, ulong entityId, HashSet<INetworkPlayer> participants)
    {
        Shard = shard;
        EntityId = entityId;
        AeroEntityId = new EntityId() { Backing = EntityId, ControllerId = Controller.Encounter };
        Participants = participants;
    }

    protected BaseEncounter(IShard shard, ulong entityId, INetworkPlayer soloParticipant)
        : this(shard, entityId, new HashSet<INetworkPlayer>() { soloParticipant })
    {
    }

    public IShard Shard { get; protected set; }
    public ulong EntityId { get; }
    public EntityId AeroEntityId { get; protected set; }

    public HashSet<INetworkPlayer> Participants { get; }
    public INetworkPlayer SoloParticipant => Participants.Single();
    public virtual IAeroEncounter View => null;

    public virtual void OnUpdate(ulong currentTime)
    {
    }

    public virtual void OnSignal()
    {
    }

    public virtual void OnSuccess()
    {
        // todo rewards
        Shard.EncounterMan.Remove(this);
    }

    public virtual void OnFailure()
    {
        Shard.EncounterMan.Remove(this);
    }

    protected void PlayDialog(uint id)
    {
        var msg = new PlayDialogScriptMessage() { DialogId = id, Unk1 = new ulong[] { 0 } };

        foreach (var p in Participants)
        {
            p.NetChannels[ChannelType.ReliableGss].SendMessage(msg, p.CharacterEntity.EntityId);
        }
    }

    protected void RewardWithResource(uint resourceId, uint quantity)
    {
        foreach (var p in Participants)
        {
            p.Inventory.AddResource(resourceId, quantity);
        }
    }
}