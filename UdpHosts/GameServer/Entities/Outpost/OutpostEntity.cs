using System;
using System.Collections.Generic;
using AeroMessages.Common;
using AeroMessages.GSS.V66;
using AeroMessages.GSS.V66.Outpost.View;
using GameServer.Data.SDB.Records.customdata;
using GameServer.Data.SDB.Records.dbcharacter;

namespace GameServer.Entities.Outpost;

public class OutpostEntity : BaseEntity
{
    private static readonly Random Rng = new();

    public OutpostEntity(IShard shard, ulong eid, Data.SDB.Records.customdata.Outpost record)
        : base(shard, eid)
    {
        AeroEntityId = new EntityId() { Backing = EntityId, ControllerId = Controller.Outpost };
        Scoping = new ScopingComponent() { Global = true };
        InitFields();
        InitViews(record);
        AddSpawnPoints(record);
    }

    public ObserverView Outpost_ObserverView { get; set; }

    public ulong EncounterId { get; set; }
    public ScopeBubbleInfoData ScopeBubbleInfo { get; set; }

    public SpawnPoint RandomSpawnPoint => SpawnPoints[Rng.Next(SpawnPoints.Count)];
    public bool IsCapturedByHostiles => Faction.HostileFactionIds.Contains(Outpost_ObserverView.FactionIdProp);
    private List<SpawnPoint> SpawnPoints { get; set; } = [];

    private void InitFields()
    {
        ScopeBubbleInfo = new ScopeBubbleInfoData()
        {
            Layer = 0,
            Unk2 = 1
        };
    }

    private void InitViews(Data.SDB.Records.customdata.Outpost record)
    {
        Outpost_ObserverView = new ObserverView
        {
            OutpostNameProp = record.OutpostName,
            PositionProp = record.Position,
            LevelBandIdProp = record.LevelBandId,
            SinUnlockIndexProp = record.SinUnlockIndex,
            TeleportCostProp = record.TeleportCost,
            ProgressProp = 0f,
            FactionIdProp = record.FactionId,
            TeamProp = 0,
            UnderAttackProp = 0,
            OutpostTypeProp = record.OutpostType,
            PossibleBuffsIdProp = record.PossibleBuffsId,
            PowerLevelProp = 0,
            MWCurrentProp = 0,
            MWMaxProp = 0,
            MapMarkerTypeIdProp = record.MarkerType,
            RadiusProp = record.Radius,
            Dynamic_11Prop = new byte[4],
            EncounterIdProp = new EntityId { Backing = EncounterId },
            ScopeBubbleInfoProp = ScopeBubbleInfo
        };
    }

    private void AddSpawnPoints(Data.SDB.Records.customdata.Outpost record)
    {
        if (record.SpawnPoints.Count == 0)
        {
            SpawnPoints = [ new SpawnPoint { Position = record.Position } ];
        }
        else
        {
            SpawnPoints = record.SpawnPoints;
        }
    }
}