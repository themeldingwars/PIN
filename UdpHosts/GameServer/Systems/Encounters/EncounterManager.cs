using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using AeroMessages.GSS.V66.Generic;
using GameServer.Data.SDB.Records.aptfs;
using GameServer.Entities;
using GameServer.Entities.Character;
using GameServer.Systems.Encounters.Encounters;

namespace GameServer.Systems.Encounters;

public class EncounterManager
{
    private const ulong _updateFlushIntervalMs = 20;

    private readonly Shard Shard;
    private ulong _lastUpdateFlush = 0;

    public EncounterManager(Shard shard)
    {
        Shard = shard;
    }

    public Thumper CreateThumper(
        uint nodeType,
        Vector3 position,
        BaseEntity owner,
        ResourceNodeBeaconCalldownCommandDef commandDef)
    {
        var ownerPlayer = (CharacterEntity)owner;

        var thumperEntity = Shard.EntityMan.SpawnThumper(nodeType, position, owner, commandDef);

        // add squadmates later
        var thumper = new Thumper(
          Shard,
          Shard.GetNextGuid(),
          new HashSet<INetworkPlayer>() { ownerPlayer.Player },
          thumperEntity);

        thumperEntity.Encounter = new EncounterComponent() { EncounterId = thumper.EntityId, Instance = thumper };

        Add(thumper.EntityId, thumper);

        return thumper;
    }

    public void Tick(double deltaTime, ulong currentTime, CancellationToken ct)
    {
        if (currentTime > _lastUpdateFlush + _updateFlushIntervalMs)
        {
            _lastUpdateFlush = currentTime;

            foreach (var encounter in Shard.Encounters.Values)
            {
                // todo add update queue
                encounter.OnUpdate(currentTime);
                // FlushChanges(encounter);
            }
        }
    }

    public void Add(ulong guid, IEncounter encounter)
    {
        Shard.Encounters.Add(guid, encounter);
        ScopeIn(encounter);
    }

    public void Remove(IEncounter encounter)
    {
        ScopeOut(encounter);
        Remove(encounter.EntityId);
    }

    public void Remove(ulong guid)
    {
        Shard.Encounters.TryGetValue(guid, out IEncounter encounter);
        if (encounter != null)
        {
            Shard.Encounters.Remove(guid);
        }
    }

    private void ScopeIn(IEncounter encounter)
    {
        if (encounter.View == null || encounter.View.GetPackedChangesSize() == 0)
        {
            return;
        }

        var size = encounter.View.GetPackedChangesSize();
        var serializedData = new Memory<byte>(new byte[size]);
        encounter.View.PackChanges(serializedData.Span);

        var msg = new EncounterUIScopeIn(size)
                  {
                      EncounterId = encounter.AeroEntityId,
                      Header = encounter.View.GetHeader(),
                      SinCard = new SinCardData[] { },
                      SchemaVersion = 2,
                      ShadowFieldValues = serializedData.ToArray()
                  };

        foreach (var player in encounter.Participants)
        {
            player.NetChannels[ChannelType.ReliableGss].SendMessage(msg, player.CharacterEntity.EntityId);
        }
    }

    private void FlushChanges(IEncounter encounter)
    {
        if (encounter.View == null || encounter.View.GetPackedChangesSize() == 0)
        {
            return;
        }

        var size = encounter.View.GetPackedChangesSize();
        var serializedData = new Memory<byte>(new byte[size]);
        encounter.View.PackChanges(serializedData.Span);

        var msg = new EncounterUIUpdate(size)
                  {
                      EncounterId = encounter.AeroEntityId,
                      ShadowFieldValues = serializedData.ToArray(),
                      BlobData = new byte[] { },
                  };

        foreach (var player in encounter.Participants)
        {
            player.NetChannels[ChannelType.ReliableGss].SendMessage(msg, player.CharacterEntity.EntityId);
        }
    }

    private void ScopeOut(IEncounter encounter)
    {
        if (encounter.View == null)
        {
            return;
        }

        var msg = new EncounterUIScopeOut() { EncounterId = encounter.AeroEntityId };
        foreach (var player in encounter.Participants)
        {
            player.NetChannels[ChannelType.ReliableGss].SendMessage(msg, player.CharacterEntity.EntityId);
        }
    }
}
