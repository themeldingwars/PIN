using System;
using System.Numerics;
using AeroMessages.GSS.V66;
using AeroMessages.GSS.V66.Melding.View;

namespace GameServer.Entities.Melding;

public class MeldingEntity : BaseEntity
{
    public MeldingEntity(IShard shard, ulong eid, string perimiterSetName)
        : base(shard, eid)
    {
        PerimiterSetName = perimiterSetName;
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
            Unk1 = 0,
            Unk2 = 0,
            Unk3 = 0,
            ControlPoints_1 = Array.Empty<Vector3>(),
            Offsets_1 = Array.Empty<Vector3>(),
            ControlPoints_2 = Array.Empty<Vector3>(),
            Offsets_2 = Array.Empty<Vector3>(),
        };
        ScopeBubbleInfo = new ScopeBubbleInfoData()
        {
            Unk1 = 0,
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