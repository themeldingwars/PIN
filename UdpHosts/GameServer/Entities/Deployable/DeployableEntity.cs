using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AeroMessages.Common;
using AeroMessages.GSS.V66;
using AeroMessages.GSS.V66.Deployable;
using AeroMessages.GSS.V66.Deployable.View;
using GameServer.Aptitude;

namespace GameServer.Entities.Deployable;

public class DeployableEntity : BaseAptitudeEntity, IAptitudeTarget
{
    // TODO: Add Deployable Hardpoint support
    public DeployableEntity(IShard shard, ulong eid, uint type, uint abilitySrcId)
        : base(shard, eid)
    {
        AeroEntityId = new EntityId() { Backing = EntityId, ControllerId = Controller.Deployable };
        Type = type;
        AbilitySrcId = abilitySrcId;
        InitFields();
        InitViews();
    }

    public ObserverView Deployable_ObserverView { get; set; }

    public INetworkPlayer Player { get; set; }
    public bool IsPlayerOwned => Player != null;
    public BaseEntity Owner { get; set; }
    public Quaternion Orientation { get; set; }
    public Vector3 AimPosition => Position;
    public Vector3 AimDirection { get; set; }
    public HostilityInfoData HostilityInfo { get; set; }

    public uint ConstructedTime { get; set; }
    public uint Type { get; set; }
    public uint AbilitySrcId { get; set; }
    public uint GibVisualsID { get; set; }
    public int MaxHealth { get; set; } = 0;

    public ushort StatusEffectsChangeTime_0 { get; set; }
    public ushort StatusEffectsChangeTime_1 { get; set; }
    public ushort StatusEffectsChangeTime_2 { get; set; }
    public ushort StatusEffectsChangeTime_3 { get; set; }
    public ushort StatusEffectsChangeTime_4 { get; set; }
    public ushort StatusEffectsChangeTime_5 { get; set; }
    public ushort StatusEffectsChangeTime_6 { get; set; }
    public ushort StatusEffectsChangeTime_7 { get; set; }
    public ushort StatusEffectsChangeTime_8 { get; set; }
    public ushort StatusEffectsChangeTime_9 { get; set; }
    public ushort StatusEffectsChangeTime_10 { get; set; }
    public ushort StatusEffectsChangeTime_11 { get; set; }
    public ushort StatusEffectsChangeTime_12 { get; set; }
    public ushort StatusEffectsChangeTime_13 { get; set; }
    public ushort StatusEffectsChangeTime_14 { get; set; }
    public ushort StatusEffectsChangeTime_15 { get; set; }
    public ushort StatusEffectsChangeTime_16 { get; set; }
    public ushort StatusEffectsChangeTime_17 { get; set; }
    public ushort StatusEffectsChangeTime_18 { get; set; }
    public ushort StatusEffectsChangeTime_19 { get; set; }
    public ushort StatusEffectsChangeTime_20 { get; set; }
    public ushort StatusEffectsChangeTime_21 { get; set; }
    public ushort StatusEffectsChangeTime_22 { get; set; }
    public ushort StatusEffectsChangeTime_23 { get; set; }
    public ushort StatusEffectsChangeTime_24 { get; set; }
    public ushort StatusEffectsChangeTime_25 { get; set; }
    public ushort StatusEffectsChangeTime_26 { get; set; }
    public ushort StatusEffectsChangeTime_27 { get; set; }
    public ushort StatusEffectsChangeTime_28 { get; set; }
    public ushort StatusEffectsChangeTime_29 { get; set; }
    public ushort StatusEffectsChangeTime_30 { get; set; }
    public ushort StatusEffectsChangeTime_31 { get; set; }
    public StatusEffectData? StatusEffects_0 { get; set; }
    public StatusEffectData? StatusEffects_1 { get; set; }
    public StatusEffectData? StatusEffects_2 { get; set; }
    public StatusEffectData? StatusEffects_3 { get; set; }
    public StatusEffectData? StatusEffects_4 { get; set; }
    public StatusEffectData? StatusEffects_5 { get; set; }
    public StatusEffectData? StatusEffects_6 { get; set; }
    public StatusEffectData? StatusEffects_7 { get; set; }
    public StatusEffectData? StatusEffects_8 { get; set; }
    public StatusEffectData? StatusEffects_9 { get; set; }
    public StatusEffectData? StatusEffects_10 { get; set; }
    public StatusEffectData? StatusEffects_11 { get; set; }
    public StatusEffectData? StatusEffects_12 { get; set; }
    public StatusEffectData? StatusEffects_13 { get; set; }
    public StatusEffectData? StatusEffects_14 { get; set; }
    public StatusEffectData? StatusEffects_15 { get; set; }
    public StatusEffectData? StatusEffects_16 { get; set; }
    public StatusEffectData? StatusEffects_17 { get; set; }
    public StatusEffectData? StatusEffects_18 { get; set; }
    public StatusEffectData? StatusEffects_19 { get; set; }
    public StatusEffectData? StatusEffects_20 { get; set; }
    public StatusEffectData? StatusEffects_21 { get; set; }
    public StatusEffectData? StatusEffects_22 { get; set; }
    public StatusEffectData? StatusEffects_23 { get; set; }
    public StatusEffectData? StatusEffects_24 { get; set; }
    public StatusEffectData? StatusEffects_25 { get; set; }
    public StatusEffectData? StatusEffects_26 { get; set; }
    public StatusEffectData? StatusEffects_27 { get; set; }
    public StatusEffectData? StatusEffects_28 { get; set; }
    public StatusEffectData? StatusEffects_29 { get; set; }
    public StatusEffectData? StatusEffects_30 { get; set; }
    public StatusEffectData? StatusEffects_31 { get; set; }

    public override void SetStatusEffect(byte index, ushort time, StatusEffectData data)
    {
        Console.WriteLine($"Deployable.SetStatusEffect Index {index}, Time {time}, Id {data.Id}");

        // Member
        this.GetType().GetProperty($"StatusEffectsChangeTime_{index}").SetValue(this, time, null);
        this.GetType().GetProperty($"StatusEffects_{index}").SetValue(this, data, null);

        // ObserverView
        Deployable_ObserverView.GetType().GetProperty($"StatusEffectsChangeTime_{index}Prop").SetValue(Deployable_ObserverView, time, null);
        Deployable_ObserverView.GetType().GetProperty($"StatusEffects_{index}Prop").SetValue(Deployable_ObserverView, data, null);
    }

    public override void ClearStatusEffect(byte index, ushort time, uint debugEffectId)
    {
        Console.WriteLine($"Deployable.ClearStatusEffect Index {index}, Time {time}, Id {debugEffectId}");

        // Member
        this.GetType().GetProperty($"StatusEffectsChangeTime_{index}").SetValue(this, time, null);
        this.GetType().GetProperty($"StatusEffects_{index}").SetValue(this, null, null);

        // ObserverView
        Deployable_ObserverView.GetType().GetProperty($"StatusEffectsChangeTime_{index}Prop").SetValue(Deployable_ObserverView, time, null);
        Deployable_ObserverView.GetType().GetProperty($"StatusEffects_{index}Prop").SetValue(Deployable_ObserverView, null, null);
    }

    public void SetPosition(Vector3 newPosition)
    {
        Position = newPosition;
        Deployable_ObserverView.PositionProp = Position;
    }

    public void SetRotation(Quaternion newRotation)
    {
        Orientation = newRotation;
        Deployable_ObserverView.OrientationProp = Orientation;
    }

    public void SetOrientation(Quaternion newRotation) => SetRotation(newRotation);

    public void SetAimDirection(Vector3 newDirection)
    {
        AimDirection = newDirection;
        Deployable_ObserverView.AimDirectionProp = AimDirection;
    }

    public override bool IsInteractable()
    {
        return Interaction != null ? Interaction.Type != 0 : false;
    }

    public override bool CanBeInteractedBy(IEntity other)
    {
        return IsInteractable();
    }

    private void InitFields()
    {
        Position = new Vector3();
        Orientation = Quaternion.Identity;
        AimDirection = new Vector3(0.70707911253f, 0.707134246826f, 1f);
        HostilityInfo = new HostilityInfoData { Flags = 0 | HostilityInfoData.HostilityFlags.Faction, FactionId = 1 };
        ConstructedTime = Shard.CurrentTime;
    }

    private void InitViews()
    {
        Deployable_ObserverView = new ObserverView
        {
            TypeProp = Type,
            OwningEntityProp = Owner?.AeroEntityId ?? new EntityId { Backing = 0 },
            AbilitySrcIdProp = AbilitySrcId,
            PositionProp = Position,
            OrientationProp = Orientation,
            AimPositionProp = AimPosition,
            AimDirectionProp = AimDirection,
            ConstructedTimeProp = ConstructedTime,
            CurrentHealthPctProp = 100,
            MaxHealthProp = MaxHealth,
            LevelProp = 0,
            ScalingLevelProp = 0,
            GibVisualsIDProp = GibVisualsID,
            HostilityInfoProp = HostilityInfo,
            AttachedToProp = new EntityId { Backing = 0 },
            CharacterStatsProp = new CharacterStatsData
            {
                ItemAttributes = Array.Empty<StatsData>(),
                Unk1 = 0,
                WeaponA = Array.Empty<StatsData>(),
                Unk2 = 0,
                WeaponB = Array.Empty<StatsData>(),
                Unk3 = 0,
                AttributeCategories1 = Array.Empty<StatsData>(),
                AttributeCategories2 = Array.Empty<StatsData>()
            },
            WarpaintColorsProp = Array.Empty<ushort>(),
            PersonalFactionStanceProp = null,
            SinFlagsProp = 0,
            SinFactionsAcquiredByProp = null,
            SinTeamsAcquiredByProp = null,
            SinCardTypeProp = 0,
            SinCardFields_0Prop = null,
            SinCardFields_1Prop = null,
            SinCardFields_2Prop = null,
            SinCardFields_3Prop = null,
            SinCardFields_4Prop = null,
            SinCardFields_5Prop = null,
            SinCardFields_6Prop = null,
            SinCardFields_7Prop = null,
            SinCardFields_8Prop = null,
            SinCardFields_9Prop = null,
            SinCardFields_10Prop = null,
            SinCardFields_11Prop = null,
            SinCardFields_12Prop = null,
            SinCardFields_13Prop = null,
            SinCardFields_14Prop = null,
            SinCardFields_15Prop = null,
            SinCardFields_16Prop = null,
            SinCardFields_17Prop = null,
            SinCardFields_18Prop = null,
            SinCardFields_19Prop = null,
        };
    }
}