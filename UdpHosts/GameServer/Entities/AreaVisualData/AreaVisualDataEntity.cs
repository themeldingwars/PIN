using AeroMessages.Common;
using AeroMessages.GSS.V66.AreaVisualData.View;

namespace GameServer.Entities.AreaVisualData;

public sealed class AreaVisualDataEntity : BaseEntity
{
    public AreaVisualDataEntity(IShard shard, ulong eid)
        : base(shard, eid)
    {
        AeroEntityId = new EntityId() { Backing = EntityId, ControllerId = Controller.AreaVisualData };
        InitFields();
        InitViews();
    }

    public ObserverView AreaVisualData_ObserverView { get; set; }
    public ParticleEffectsView AreaVisualData_ParticleEffectsView { get; set; }
    public MapMarkerView AreaVisualData_MapMarkerView { get; set; }
    public TinyObjectView AreaVisualData_TinyObjectView { get; set; }
    public LootObjectView AreaVisualData_LootObjectView { get; set; }
    public ForceShieldView AreaVisualData_ForceShieldView { get; set; }

    private void InitFields()
    {
    }

    private void InitViews()
    {
        AreaVisualData_ObserverView = new ObserverView() { PositionProp = Position };
        AreaVisualData_ParticleEffectsView = new ParticleEffectsView();
    }
}