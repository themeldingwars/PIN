using System;
using System.Numerics;
using AeroMessages.GSS.V66;
using AeroMessages.GSS.V66.Outpost.View;

namespace GameServer.Entities.Outpost;

public class OutpostEntity : BaseEntity
{
    public OutpostEntity(IShard shard, ulong eid, Data.SDB.Records.customdata.Outpost record)
        : base(shard, eid)
    {
        InitFields();
        InitViews(record);
    }

    public ObserverView Outpost_ObserverView { get; set; }

    public ulong EncounterId { get; set; }
    public ScopeBubbleInfoData ScopeBubbleInfo { get; set; }

    private void InitFields()
    {
        ScopeBubbleInfo = new ScopeBubbleInfoData()
        {
            Unk1 = 0,
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
            EncounterIdProp = new AeroMessages.Common.EntityId { Backing = EncounterId },
            ScopeBubbleInfoProp = ScopeBubbleInfo
        };
    }
}