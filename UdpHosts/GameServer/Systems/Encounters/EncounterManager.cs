using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using AeroMessages.Common;
using AeroMessages.GSS.V66.Character.Command;
using AeroMessages.GSS.V66.Character.Event;
using AeroMessages.GSS.V66.Generic;
using GameServer.Data.SDB;
using GameServer.Data.SDB.Records.aptfs;
using GameServer.Entities;
using GameServer.Entities.Character;
using GameServer.Systems.Encounters.Encounters;

namespace GameServer.Systems.Encounters;

public class EncounterManager
{
    public Factory Factory;

    private const ulong _updateFlushIntervalMs = 40;
    private const ulong _lifetimeCheckIntervalMs = 1000;

    private readonly Shard Shard;
    private ulong _lastUpdateFlush = 0;
    private ulong _lastLifetimeCheck = 0;
    private bool _hasSpawnedZoneEncounters = false;

    public Dictionary<BaseEntity, IProximityHandler> EntitiesToCheckProximity = new Dictionary<BaseEntity, IProximityHandler>();
    private Dictionary<ulong, IEncounter> UiQueries = new Dictionary<ulong, IEncounter>();
    private HashSet<IEncounter> EncountersToUpdate = new HashSet<IEncounter>();
    private ConcurrentDictionary<ulong, Lifetime> LifetimeByEncounter = new ConcurrentDictionary<ulong, Lifetime>();

    public EncounterManager(Shard shard)
    {
        Shard = shard;
        Factory = new Factory(shard);
    }

    public void SendUiQuery(NewUiQuery uiQuery, INetworkPlayer target, IEncounter encounter)
    {
        UiQueries.Add(uiQuery.QueryGuid, encounter);

        target.NetChannels[ChannelType.ReliableGss].SendMessage(uiQuery, target.CharacterEntity.EntityId);
    }

    public void HandleUiQueryResponse(UiQueryResponse uiQueryResponse, INetworkPlayer player)
    {
        if (UiQueries.TryGetValue(uiQueryResponse.QueryGuid, out var encounter)
            && encounter is IDonationHandler donationHandler)
        {
            donationHandler.OnDonation(uiQueryResponse, player);

            UiQueries.Remove(uiQueryResponse.QueryGuid);
        }
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

    public void SpawnZoneEncounters(uint zoneId)
    {
        foreach (var entry in CustomDBInterface.GetZoneMeldingRepulsors(zoneId))
        {
            var guid = Shard.GetNextGuid((byte)Controller.Encounter);
            Add(guid, new MeldingRepulsor(Shard, guid, new HashSet<INetworkPlayer>(), entry.Value));
        }

        foreach (var entry in CustomDBInterface.GetZoneLgvRaces(zoneId))
        {
            var t = entry.Value.Terminal;
            var terminal = Shard.EntityMan.SpawnDeployable(820, t.Position, t.Orientation);
            terminal.Encounter = new EncounterComponent() { SpawnDef = entry.Value, Events = EncounterComponent.Event.Interaction };
        }
    }

    public void SetRemainingLifetime(ICanTimeout encounter, uint timeMs)
    {
        var tracker = LifetimeByEncounter.TryGetValue(encounter.EntityId, out var value) ? value : new Lifetime();

        tracker.ExpireAt = Shard.CurrentTimeLong + timeMs;
        LifetimeByEncounter[encounter.EntityId] = tracker;
    }

    public void StartUpdatingEncounter(IEncounter encounter)
    {
        EncountersToUpdate.Add(encounter);
    }

    public void StopUpdatingEncounter(IEncounter encounter)
    {
        EncountersToUpdate.Remove(encounter);
    }

    public void Tick(double deltaTime, ulong currentTime, CancellationToken ct)
    {
        if (!_hasSpawnedZoneEncounters && currentTime != 0)
        {
            _hasSpawnedZoneEncounters = true;

            if (Shard.Settings.LoadZoneEntities)
            {
                SpawnZoneEncounters(Shard.ZoneId);
            }
        }

        if (currentTime > _lastUpdateFlush + _updateFlushIntervalMs)
        {
            _lastUpdateFlush = currentTime;

            foreach (var encounter in EncountersToUpdate)
            {
                // todo add update queue
                encounter.OnUpdate(currentTime);

                // FlushChanges(encounter);
            }

            foreach (var (entity, encounter) in EntitiesToCheckProximity)
            {
                foreach (var p in encounter.Participants)
                {
                    if (!Shard.EntityMan.HasScopedInEntity(entity.EntityId, p))
                    {
                        continue;
                    }

                    if (Vector3.Distance(entity.Position, p.CharacterEntity.Position) < entity.Encounter.ProximityDistance)
                    {
                        encounter.OnProximity(entity, p);
                    }
                }
            }
        }

        if (currentTime > _lastLifetimeCheck + _lifetimeCheckIntervalMs)
        {
            _lastLifetimeCheck = currentTime;

            foreach ((ulong entityId, Lifetime tracker) in LifetimeByEncounter)
            {
                if (currentTime > tracker.ExpireAt)
                {
                    if (Shard.Encounters.TryGetValue(entityId, out var e) && e is ICanTimeout encounter)
                    {
                        encounter.OnTimeOut();

                        LifetimeByEncounter.Remove(encounter.EntityId, out _);
                    }
                }
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

    private class Lifetime
    {
        public ulong ExpireAt;
    }
}
