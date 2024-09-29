using System;
using System.Numerics;
using AeroMessages.Common;
using AeroMessages.GSS.V66;
using AeroMessages.GSS.V66.ResourceNode.View;
using GameServer.Aptitude;
using GameServer.Data.SDB.Records.aptfs;
using GameServer.Entities.Character;
using GameServer.Enums;

namespace GameServer.Entities.Thumper;

public class ThumperEntity : BaseAptitudeEntity, IAptitudeTarget
{
    public ThumperEntity(
        IShard shard,
        ulong eid,
        uint nodeType,
        Vector3 position,
        BaseEntity owner,
        ResourceNodeBeaconCalldownCommandDef commandDef)
        : base(shard, eid)
    {
        AeroEntityId = new EntityId() { Backing = EntityId, ControllerId = Controller.ResourceNode };
        NodeType = nodeType;
        BeaconType = commandDef.ResourceNodeBeaconId;
        Scoping = new ScopingComponent() { Global = true };
        Position = position;
        Owner = owner;
        LandedAbility = commandDef.LandedAbility;
        CompletedAbility = commandDef.CompletedAbility;
        CalldownTimeMs = commandDef.CalldownTimeMs;
        MaxHealth = (uint)commandDef.Health;
        Interaction = new InteractionComponent()
          {
              Type = InteractionType.GenericHold,
              CompletedAbilityId = CompletedAbility,
          };
        InitFields();
        InitViews();
    }

    public ObserverView ResourceNode_ObserverView { get; set; }

    public INetworkPlayer Player { get; set; }
    public bool IsPlayerOwned => Player != null;
    public BaseEntity Owner { get; set; }
    public HostilityInfoData HostilityInfo { get; set; }

    public uint NodeType { get; set; }
    public uint BeaconType { get; set; }
    public float Progress { get; set; } = 0.00f;
    public ThumpingCharacterInfoStruct ThumpingCharacterInfo { get; set; }
    public StateInfoStruct StateInfo { get; set; }
    public ScopeBubbleInfoData ScopeBubble { get; set; } = new ScopeBubbleInfoData()
    {
        Layer = 0,
        Unk2 = 1
    };

    public uint LandedAbility { get; set; } = 0;
    public uint CompletedAbility { get; set; } = 0;
    public uint CalldownTimeMs { get; set; } = 0;
    public uint MaxHealth { get; set; } = 0;

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
        Console.WriteLine($"Thumper.SetStatusEffect Index {index}, Time {time}, Id {data.Id}");

        // Member
        this.GetType().GetProperty($"StatusEffectsChangeTime_{index}").SetValue(this, time, null);
        this.GetType().GetProperty($"StatusEffects_{index}").SetValue(this, data, null);

        // ObserverView
        ResourceNode_ObserverView.GetType().GetProperty($"StatusEffectsChangeTime_{index}Prop").SetValue(ResourceNode_ObserverView, time, null);
        ResourceNode_ObserverView.GetType().GetProperty($"StatusEffects_{index}Prop").SetValue(ResourceNode_ObserverView, data, null);
    }

    public override void ClearStatusEffect(byte index, ushort time, uint debugEffectId)
    {
        Console.WriteLine($"Thumper.ClearStatusEffect Index {index}, Time {time}, Id {debugEffectId}");

        // Member
        this.GetType().GetProperty($"StatusEffectsChangeTime_{index}").SetValue(this, time, null);
        this.GetType().GetProperty($"StatusEffects_{index}").SetValue(this, null, null);

        // ObserverView
        ResourceNode_ObserverView.GetType().GetProperty($"StatusEffectsChangeTime_{index}Prop").SetValue(ResourceNode_ObserverView, time, null);
        ResourceNode_ObserverView.GetType().GetProperty($"StatusEffects_{index}Prop").SetValue(ResourceNode_ObserverView, null, null);
    }

    public void SetProgress(float newProgress)
    {
        Progress = newProgress;
        ResourceNode_ObserverView.ProgressProp = Progress;
    }

    public void TransitionToState(ThumperState newState, uint? countdownOverride = null)
    {
        var countdownTime = countdownOverride ?? newState.CountdownTime();
        StateInfo = new StateInfoStruct()
                    {
                        State = (byte)newState,
                        Time = Shard.CurrentTime,
                        CountdownTime = Shard.CurrentTime + countdownTime,
                    };
        ResourceNode_ObserverView.StateInfoProp = StateInfo;
    }

    public override bool IsInteractable()
    {
        return StateInfo.State is (byte)ThumperState.THUMPING or (byte)ThumperState.COMPLETED;
    }

    public override bool CanBeInteractedBy(IEntity other)
    {
        return IsInteractable()
               && other is CharacterEntity character
               && Encounter.Instance.Participants.Contains(character.Player);
    }

    private void InitFields()
    {
        var charOwner = (CharacterEntity)Owner;
        HostilityInfo = new HostilityInfoData { Flags = 0 | HostilityInfoData.HostilityFlags.Faction, FactionId = 1 };
        ThumpingCharacterInfo = new ThumpingCharacterInfoStruct
        {
            OwnerId1 = Owner.AeroEntityId,
            OwnerId2 = Owner.AeroEntityId,
            Owner = charOwner.ToString(),
            Unk = 0f,
        };
        StateInfo = new StateInfoStruct
        {
            State = (byte)ThumperState.LANDING,
            Time = Shard.CurrentTime,
            CountdownTime = Shard.CurrentTime + CalldownTimeMs
        };
    }

    private void InitViews()
    {
        ResourceNode_ObserverView = new ObserverView
        {
            NodeTypeProp = NodeType,
            BeaconIdProp = BeaconType,
            PositionProp = Position,
            ThumpingCharacterInfoProp = ThumpingCharacterInfo,
            StateInfoProp = StateInfo,
            CurrentHealthPctProp = 100,
            MaxHealthProp = MaxHealth,
            ProgressProp = Progress,
            HostilityInfoProp = HostilityInfo,
            PersonalFactionStanceProp = null,
            ScopeBubbleInfoProp = ScopeBubble
        };
    }
}