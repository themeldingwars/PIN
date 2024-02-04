using System;
using System.Numerics;
using AeroMessages.Common;
using AeroMessages.GSS.V66;
using AeroMessages.GSS.V66.MeldingBubble.View;

namespace GameServer.Entities.MeldingBubble;

public class MeldingBubbleEntity : BaseEntity
{
    public MeldingBubbleEntity(IShard shard, ulong eid)
        : base(shard, eid)
    {
        AeroEntityId = new EntityId() { Backing = EntityId, ControllerId = Controller.MeldingBubble };
        InitFields();
        InitViews();
    }

    public ObserverView MeldingBubble_ObserverView { get; set; }

    public PositionStruct Position { get; set; }
    public RadiusStruct Radius { get; set; }
    public byte BubbleType { get; set; }
    public byte FxFlags { get; set; }
    public ScopeBubbleInfoData ScopeBubbleInfo { get; set; }

    public void SetPosition(PositionStruct newValue)
    {
        Position = newValue;
        MeldingBubble_ObserverView.PositionProp = newValue;
    }

    public void SetRadius(RadiusStruct newValue)
    {
        Radius = newValue;
        MeldingBubble_ObserverView.RadiusProp = newValue;
    }

    public void SetBubbleType(byte newValue)
    {
        BubbleType = newValue;
        MeldingBubble_ObserverView.BubbleTypeProp = newValue;
    }

    public void SetFxFlags(byte newValue)
    {
        FxFlags = newValue;
        MeldingBubble_ObserverView.FxFlagsProp = newValue;
    }

    private void InitFields()
    {
        Position = new PositionStruct
        {
            Position = Vector3.Zero,
            Time = Shard.CurrentTime
        };
        Radius = new RadiusStruct
        {
            Radius = 1f,
            Time = Shard.CurrentTime
        };
        ScopeBubbleInfo = new ScopeBubbleInfoData()
        {
            Unk1 = 0,
            Unk2 = 1
        };
    }

    private void InitViews()
    {
        MeldingBubble_ObserverView = new ObserverView
        {
            PositionProp = Position,
            RadiusProp = Radius,
            BubbleTypeProp = BubbleType,
            FxFlagsProp = FxFlags,
            ScopeBubbleInfoProp = ScopeBubbleInfo
        };
    }
}