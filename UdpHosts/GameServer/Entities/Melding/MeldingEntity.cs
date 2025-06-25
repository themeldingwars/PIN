using System;
using System.Numerics;
using AeroMessages.Common;
using AeroMessages.GSS.V66;
using AeroMessages.GSS.V66.Melding.View;

namespace GameServer.Entities.Melding;

public sealed class MeldingEntity : BaseEntity
{
    public MeldingEntity(IShard shard, ulong eid, string perimiterSetName)
        : base(shard, eid)
    {
        AeroEntityId = new EntityId() { Backing = EntityId, ControllerId = Controller.Melding };
        PerimiterSetName = perimiterSetName;
        Scoping = new ScopingComponent() { Global = true };
        InitFields();
        InitViews();
    }

    public ObserverView Melding_ObserverView { get; set; }

    public string PerimiterSetName { get; set; } = string.Empty;
    public ActiveDataStruct ActiveData { get; set; }
    public ScopeBubbleInfoData ScopeBubbleInfo { get; set; }

    public void SetActiveData(ActiveDataStruct newValue)
    {
        ActiveData = newValue;
        Melding_ObserverView.ActiveDataProp = newValue;
    }

    private void InitFields()
    {
        ActiveData = new ActiveDataStruct()
        {
            TimestampMicro = 0,
            Unk2 = 0,
            Unk3 = 0,
            FromPoints = Array.Empty<Vector3>(),
            FromTangents = Array.Empty<Vector3>(),
            ToPoints = Array.Empty<Vector3>(),
            ToTangets = Array.Empty<Vector3>(),
        };
        ScopeBubbleInfo = new ScopeBubbleInfoData()
        {
            Layer = 0,
            Unk2 = 1
        };
    }

    private void InitViews()
    {
        Melding_ObserverView = new ObserverView
        {
            PerimiterSetNameProp = PerimiterSetName,
            ActiveDataProp = ActiveData,
            ScopeBubbleInfoProp = ScopeBubbleInfo
        };
    }
}