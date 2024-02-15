using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AeroMessages.Common;
using AeroMessages.GSS.V66;
using AeroMessages.GSS.V66.CarryableObject;
using AeroMessages.GSS.V66.CarryableObject.View;
using GameServer.Aptitude;
using GameServer.Enums;

namespace GameServer.Entities.Carryable;

public class CarryableEntity : BaseAptitudeEntity, IAptitudeTarget
{
    public CarryableEntity(IShard shard, ulong eid, uint type)
        : base(shard, eid)
    {
        AeroEntityId = new EntityId() { Backing = EntityId, ControllerId = Controller.Carryable };
        Type = type;
        InitFields();
        InitViews();
    }

    public ObserverView CarryableObject_ObserverView { get; set; }

    public INetworkPlayer Player { get; set; }
    public bool IsPlayerCarried => Player != null;
    public BaseEntity Carrier { get; set; }
    public Vector3 Position { get; set; }
    public Quaternion Orientation { get; set; }
    public HostilityInfoData HostilityInfo { get; set; }

    public uint Type { get; set; }
    public string Name { get; set; }
    public byte VisualInfoGroupIndex { get; set; }
    public byte AllowFriendlyPickup { get; set; }
    public byte AllowHostilePickup { get; set; }
    public ScopeBubbleInfoData ScopeBubble { get; set; } = new ScopeBubbleInfoData()
    {
        Unk1 = 0,
        Unk2 = 1
    };

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
        Console.WriteLine($"Carryable.SetStatusEffect Index {index}, Time {time}, Id {data.Id}");

        // Member
        this.GetType().GetProperty($"StatusEffectsChangeTime_{index}").SetValue(this, time, null);
        this.GetType().GetProperty($"StatusEffects_{index}").SetValue(this, data, null);

        // ObserverView
        CarryableObject_ObserverView.GetType().GetProperty($"StatusEffectsChangeTime_{index}Prop").SetValue(CarryableObject_ObserverView, time, null);
        CarryableObject_ObserverView.GetType().GetProperty($"StatusEffects_{index}Prop").SetValue(CarryableObject_ObserverView, data, null);
    }

    public override void ClearStatusEffect(byte index, ushort time, uint debugEffectId)
    {
        Console.WriteLine($"Carryable.ClearStatusEffect Index {index}, Time {time}, Id {debugEffectId}");

        // Member
        this.GetType().GetProperty($"StatusEffectsChangeTime_{index}").SetValue(this, time, null);
        this.GetType().GetProperty($"StatusEffects_{index}").SetValue(this, null, null);

        // ObserverView
        CarryableObject_ObserverView.GetType().GetProperty($"StatusEffectsChangeTime_{index}Prop").SetValue(CarryableObject_ObserverView, time, null);
        CarryableObject_ObserverView.GetType().GetProperty($"StatusEffects_{index}Prop").SetValue(CarryableObject_ObserverView, null, null);
    }

    public void SetPosition(Vector3 newPosition)
    {
        Position = newPosition;
        CarryableObject_ObserverView.PositionProp = Position;
    }

    public void SetRotation(Quaternion newRotation)
    {
        Orientation = newRotation;
        CarryableObject_ObserverView.OrientationProp = Orientation;
    }

    public override bool IsInteractable()
    {
        return Interaction != null ? Interaction.Type != 0 : false;
    }

    public override bool CanBeInteractedBy(IEntity other)
    {
        return IsInteractable();
    }

    public void SetCarrier(BaseEntity entity)
    {
        Carrier = entity;
        if (entity.GetType() == typeof(Entities.Character.CharacterEntity))
        {
            var character = entity as Entities.Character.CharacterEntity;

            if (character.IsPlayerControlled)
            {
                Player = character.Player;
            }
        }

        CarryableObject_ObserverView.CarryingCharacterIdProp = Carrier.AeroEntityId;
    }

    public void ClearCarrier()
    {
        Carrier = null;
        Player = null;
        CarryableObject_ObserverView.CarryingCharacterIdProp = null;
    }

    private void InitFields()
    {
        Name = string.Empty;
        Position = new Vector3();
        Orientation = Quaternion.Identity;
        HostilityInfo = new HostilityInfoData { Flags = 0 | HostilityInfoData.HostilityFlags.Faction, FactionId = 1 };
    }

    private void InitViews()
    {
        CarryableObject_ObserverView = new ObserverView
        {
            CarryableObjectTypeIdProp = Type,
            PositionProp = Position,
            OrientationProp = Orientation,
            NameProp = Name,
            VisualInfoGroupIndexProp = VisualInfoGroupIndex,
            HostilityProp = HostilityInfo,
            AllowFriendlyPickupProp = AllowFriendlyPickup,
            AllowHostilePickupProp = AllowHostilePickup,
            PersonalFactionStanceProp = null,
            ScopeBubbleInfoProp = ScopeBubble

        };
    }
}