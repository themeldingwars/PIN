using System.Collections.Generic;
using System.Numerics;
using Aero.Gen;
using Aero.Protocol;
using AeroMessages.Common;
using AeroMessages.GSS;
using AeroMessages.GSS.AreaVisualData;
using AeroMessages.GSS.AreaVisualData.View;
namespace GameServer.Entities.AreaVisualData;

public sealed class AreaVisualDataEntity : BaseEntity
{
    public const byte InvalidIndex = 255;
    public const byte MaxFlagCount = 24;
    public const byte MaxTeamCount = 4;
    public const byte MaxShieldCount = 32;
    public const byte MaxLootCount = 24;
    public const byte MaxMarkersCount = 35;
    public const byte MaxTinyCount = 32;
    public const byte MaxParticleCount = 20;

    public static readonly HashSet<GssAreaVisualDataView> ValidViews =
    [
        GssAreaVisualDataView.ObserverView,
        GssAreaVisualDataView.ForceShieldView,
        GssAreaVisualDataView.LootObjectView,
        GssAreaVisualDataView.MapMarkerView,
        GssAreaVisualDataView.ParticleEffectsView,
        GssAreaVisualDataView.TinyObjectView,
    ];

    public AreaVisualDataEntity(IShard shard, ulong eid, Vector3 position)
        : base(shard, eid)
    {
        AeroEntityId = new EntityId() { Backing = EntityId, ControllerId = Controller.AreaVisualData };
        Position = position;
    }

    public ScopeBubbleInfoData ScopeBubble { get; set; } = new ScopeBubbleInfoData()
    {
        Layer = 0,
        Unk2 = 1
    };

    public ObserverView AreaVisualData_ObserverView { get; set; }
    public ParticleEffectsView AreaVisualData_ParticleEffectsView { get; set; }
    public MapMarkerView AreaVisualData_MapMarkerView { get; set; }
    public TinyObjectView AreaVisualData_TinyObjectView { get; set; }
    public LootObjectView AreaVisualData_LootObjectView { get; set; }
    public ForceShieldView AreaVisualData_ForceShieldView { get; set; }

    public bool IsObserverActive => AreaVisualData_ObserverView != null;
    public bool IsParticleEffectsActive => AreaVisualData_ParticleEffectsView != null;
    public bool IsMapMarkerActive => AreaVisualData_MapMarkerView != null;
    public bool IsTinyObjectActive => AreaVisualData_TinyObjectView != null;
    public bool IsLootObjecActive => AreaVisualData_LootObjectView != null;
    public bool IsForceShieldActive => AreaVisualData_ForceShieldView != null;

    public void SetPosition(Vector3 value)
    {
        Position = value;
        AreaVisualData_ObserverView?.PositionProp = Position;
        AreaVisualData_MapMarkerView?.PositionProp = Position;
        AreaVisualData_ForceShieldView?.PositionProp = Position;
    }

    public void SetScopeBubble(ScopeBubbleInfoData value)
    {
        ScopeBubble = value;
        AreaVisualData_ObserverView?.ScopeBubbleInfoProp = ScopeBubble;
    }

    public uint GetFreeFlagIndex()
    {
        InitObserverView();
        return GetFreeIndexOfArrayProperty(AreaVisualData_ObserverView, "ContextFlags", MaxFlagCount);
    }

    public uint GetFreeTeamIndex()
    {
        InitObserverView();
        return GetFreeIndexOfArrayProperty(AreaVisualData_ObserverView, "ContextTeams", MaxTeamCount);
    }

    public uint GetFreeShieldIndex()
    {
        InitForceShieldView();
        return GetFreeIndexOfArrayProperty(AreaVisualData_ForceShieldView, "ForceShields", MaxShieldCount);
    }

    public uint GetFreeLootIndex()
    {
        InitLootObjectView();
        return GetFreeIndexOfArrayProperty(AreaVisualData_LootObjectView, "LootObjects", MaxLootCount);
    }

    public uint GetFreeMarkerIndex()
    {
        InitMapMarkerView();
        return GetFreeIndexOfArrayProperty(AreaVisualData_MapMarkerView, "MapMarkers", MaxMarkersCount);
    }

    public uint GetFreeParticleIndex()
    {
        InitParticleEffectsView();
        return GetFreeIndexOfArrayProperty(AreaVisualData_ParticleEffectsView, "ParticleEffects", MaxParticleCount);
    }

    public uint GetFreeTinyIndex()
    {
        InitTinyObjectView();
        return GetFreeIndexOfArrayProperty(AreaVisualData_TinyObjectView, "TinyObjects", MaxTinyCount);
    }

    public void AddContextFlag(ContextFlag data)
    {
        uint index = GetFreeFlagIndex();
        if (index == InvalidIndex)
        {
            index = 0;
        }

        SetContextFlag(index, data);
    }

    public void AddContextTeam(ContextTeam data)
    {
        uint index = GetFreeTeamIndex();
        if (index == InvalidIndex)
        {
            index = 0;
        }

        SetContextTeam(index, data);
    }

    public void AddForceShield(ForceShieldData data)
    {
        uint index = GetFreeShieldIndex();
        if (index == InvalidIndex)
        {
            index = 0;
        }

        SetForceShield(index, data);
    }

    public void AddLootObject(LootObjectData data)
    {
        uint index = GetFreeLootIndex();
        if (index == InvalidIndex)
        {
            index = 0;
        }

        SetLootObject(index, data);
    }

    public void AddMapMarker(MapMarkerData data)
    {
        uint index = GetFreeMarkerIndex();
        if (index == InvalidIndex)
        {
            index = 0;
        }

        SetMapMarker(index, data);
    }

    public void AddParticleEffect(ParticleEffect data)
    {
        uint index = GetFreeParticleIndex();
        if (index == InvalidIndex)
        {
            index = 0;
        }

        SetParticleEffect(index, data);
    }

    public void AddTinyObject(TinyObjectData data)
    {
        uint index = GetFreeTinyIndex();
        if (index == InvalidIndex)
        {
            index = 0;
        }

        SetTinyObject(index, data);
    }

    public void SetContextFlag(uint index, ContextFlag data)
    {
        SetIndexOfArrayProperty(AreaVisualData_ObserverView, "ContextFlags", index, data);
    }

    public void SetContextTeam(uint index, ContextTeam data)
    {
        SetIndexOfArrayProperty(AreaVisualData_ObserverView, "ContextTeams", index, data);
    }

    public void SetForceShield(uint index, ForceShieldData data)
    {
        SetIndexOfArrayProperty(AreaVisualData_ForceShieldView, "ForceShields", index, data);
    }

    public void SetLootObject(uint index, LootObjectData data)
    {
        SetIndexOfArrayProperty(AreaVisualData_LootObjectView, "LootObjects", index, data);
    }

    public void SetMapMarker(uint index, MapMarkerData data)
    {
        SetIndexOfArrayProperty(AreaVisualData_MapMarkerView, "MapMarkers", index, data);
    }

    public void SetParticleEffect(uint index, ParticleEffect data)
    {
        SetIndexOfArrayProperty(AreaVisualData_ParticleEffectsView, "ParticleEffects", index, data);
    }

    public void SetTinyObject(uint index, TinyObjectData data)
    {
        SetIndexOfArrayProperty(AreaVisualData_TinyObjectView, "TinyObjects", index, data);
    }

    public void ClearContextFlag(uint index)
    {
        ClearIndexOfArrayProperty(AreaVisualData_ObserverView, "ContextFlags", index);
    }

    public void ClearContextTeam(uint index)
    {
        ClearIndexOfArrayProperty(AreaVisualData_ObserverView, "ContextTeams", index);
    }

    public void ClearForceShield(uint index)
    {
        ClearIndexOfArrayProperty(AreaVisualData_ForceShieldView, "ForceShield", index);
    }

    public void ClearLootObject(uint index)
    {
        ClearIndexOfArrayProperty(AreaVisualData_LootObjectView, "LootObjects", index);
    }

    public void ClearMapMarker(uint index)
    {
        ClearIndexOfArrayProperty(AreaVisualData_MapMarkerView, "MapMarkers", index);
    }

    public void ClearParticleEffect(uint index)
    {
        ClearIndexOfArrayProperty(AreaVisualData_ParticleEffectsView, "ParticleEffects", index);
    }

    public void ClearTinyObject(uint index)
    {
        ClearIndexOfArrayProperty(AreaVisualData_TinyObjectView, "TinyObjects", index);
    }

    public void ClearInactiveViews()
    {
        if (AreaVisualData_ObserverView != null)
        {
            bool hasFlags = IsArrayPropertyEmpty(AreaVisualData_ObserverView, "ContextFlags", MaxFlagCount);
            bool hasTeams = IsArrayPropertyEmpty(AreaVisualData_ObserverView, "ContextTeams", MaxTeamCount);
            if (!hasFlags && !hasTeams)
            {
                AreaVisualData_ObserverView = null;
            }
        }

        if (AreaVisualData_ForceShieldView != null)
        {
            bool hasShields = IsArrayPropertyEmpty(AreaVisualData_ForceShieldView, "ForceShields", MaxShieldCount);
            if (!hasShields)
            {
                AreaVisualData_ForceShieldView = null;
            }
        }

        if (AreaVisualData_LootObjectView != null)
        {
            bool hasLoot = IsArrayPropertyEmpty(AreaVisualData_LootObjectView, "LootObjects", MaxLootCount);
            if (!hasLoot)
            {
                AreaVisualData_LootObjectView = null;
            }
        }

        if (AreaVisualData_MapMarkerView != null)
        {
            bool hasMarkers = IsArrayPropertyEmpty(AreaVisualData_MapMarkerView, "MapMarkers", MaxMarkersCount);
            if (!hasMarkers)
            {
                AreaVisualData_MapMarkerView = null;
            }
        }

        if (AreaVisualData_ParticleEffectsView != null)
        {
            bool hasParticles = IsArrayPropertyEmpty(AreaVisualData_ParticleEffectsView, "ParticleEffects", MaxParticleCount);
            if (!hasParticles)
            {
                AreaVisualData_ParticleEffectsView = null;
            }
        }

        if (AreaVisualData_TinyObjectView != null)
        {
            bool hasTiny = IsArrayPropertyEmpty(AreaVisualData_TinyObjectView, "TinyObjects", MaxTinyCount);
            if (!hasTiny)
            {
                AreaVisualData_TinyObjectView = null;
            }
        }
    }

    public bool HasActiveViews()
    {
        bool haveObserver = AreaVisualData_ObserverView != null;
        bool haveShield = AreaVisualData_ForceShieldView != null;
        bool haveLoot = AreaVisualData_LootObjectView != null;
        bool haveMarker = AreaVisualData_MapMarkerView != null;
        bool haveParticle = AreaVisualData_ParticleEffectsView != null;
        bool haveTiny = AreaVisualData_TinyObjectView != null;
        return haveObserver || haveShield || haveLoot || haveMarker || haveParticle || haveTiny;
    }

    private void InitObserverView()
    {
        if (AreaVisualData_ObserverView != null)
        {
            return;
        }

        AreaVisualData_ObserverView = new ObserverView() { PositionProp = Position, ScopeBubbleInfoProp = ScopeBubble };
    }

    private void InitParticleEffectsView()
    {
        if (AreaVisualData_ParticleEffectsView != null)
        {
            return;
        }

        AreaVisualData_ParticleEffectsView = new ParticleEffectsView();
    }

    private void InitMapMarkerView()
    {
        if (AreaVisualData_MapMarkerView != null)
        {
            return;
        }

        AreaVisualData_MapMarkerView = new MapMarkerView() { PositionProp = Position };
    }

    private void InitTinyObjectView()
    {
        if (AreaVisualData_TinyObjectView != null)
        {
            return;
        }

        AreaVisualData_TinyObjectView = new TinyObjectView();
    }

    private void InitLootObjectView()
    {
        if (AreaVisualData_LootObjectView != null)
        {
            return;
        }

        AreaVisualData_LootObjectView = new LootObjectView();
    }

    private void InitForceShieldView()
    {
        if (AreaVisualData_ForceShieldView != null)
        {
            return;
        }

        AreaVisualData_ForceShieldView = new ForceShieldView() { PositionProp = Position };
    }

    private void SetIndexOfArrayProperty(IAeroViewInterface view, string propertyName, uint index, object data)
    {
        view.GetType().GetProperty($"{propertyName}_{index}Prop").SetValue(view, data, null);
    }

    private void ClearIndexOfArrayProperty(IAeroViewInterface view, string propertyName, uint index)
    {
        view.GetType().GetProperty($"{propertyName}_{index}Prop").SetValue(view, null, null);
    }

    private uint GetFreeIndexOfArrayProperty(IAeroViewInterface view, string propertyName, uint max)
    {
        uint firstFreeIndex = InvalidIndex;
        for (byte i = 0; i < max; i++)
        {
            var obj = view.GetType().GetProperty($"{propertyName}_{i}Prop").GetValue(view);
            if (obj == null)
            {
                if (firstFreeIndex == InvalidIndex)
                {
                    firstFreeIndex = i;
                    break;
                }
            }
        }

        return firstFreeIndex;
    }

    private bool IsArrayPropertyEmpty(IAeroViewInterface view, string propertyName, uint max)
    {
        bool result = false;
        for (byte i = 0; i < max; i++)
        {
            var obj = view.GetType().GetProperty($"{propertyName}_{i}Prop").GetValue(view);
            if (obj != null)
            {
                result = true;
                break;
            }
        }

        return result;
    }
}