using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AeroMessages.Common;
using AeroMessages.GSS.V66;
using AeroMessages.GSS.V66.Vehicle;
using AeroMessages.GSS.V66.Vehicle.Command;
using AeroMessages.GSS.V66.Vehicle.Controller;
using AeroMessages.GSS.V66.Vehicle.View;
using GameServer.Aptitude;
using GameServer.Controllers;
using GameServer.Data.SDB;
using GameServer.Entities.Character;
using GrpcGameServerAPIClient;

namespace GameServer.Entities.Vehicle;

public enum AttachmentRole : byte
{
    None = 0,
    Driver = 1,
    ActivePassenger = 3,
    PassivePassenger = 4,
    Turret = 5
}

public class SeatConfig
{
    public IEntity Occupant;
    public AttachmentRole Role;
    public byte Posture;
}

public class VehicleEntity : BaseAptitudeEntity, IAptitudeTarget
{
    public VehicleEntity(IShard shard, ulong eid)
        : base(shard, eid)
    {
        Interaction = new InteractionComponent()
        {
            Type = InteractionType.Vehicle
        };
        InitFields();
        InitViews();
        InitControllers();
    }

    public BaseController Vehicle_BaseController { get; set; }
    public CombatController Vehicle_CombatController { get; set; }
    public ObserverView Vehicle_ObserverView { get; set; }
    public CombatView Vehicle_CombatView { get; set; }
    public MovementView Vehicle_MovementView { get; set; }

    public INetworkPlayer ControllingPlayer { get; set; }
    public bool IsPlayerControlled => ControllingPlayer != null;
    public INetworkPlayer OwningPlayer { get; set; }
    public bool IsPlayerOwned => OwningPlayer != null;

    public Vector3 Position { get; set; } = new Vector3();
    public Quaternion Rotation { get; set; } = Quaternion.Identity;
    public Vector3 Velocity { get; set; } = new Vector3();
    public Vector3 AimDirection { get; set; } = new Vector3(0.70707911253f, 0.707134246826f, 0.000504541851114f); // Look kinda forward instead of up
    public short MovementState { get; set; } = unchecked((short)0x8000);
    public uint MovementTime { get; set; }
    public bool Alive { get; set; }
    public short TimeSinceLastJump { get; set; }
    public bool IsAirborne { get; set; }

    public ushort VehicleId { get; set; }
    public ConfigurationData Configuration { get; set; } = new ConfigurationData()
    {
        Data = new uint[8]
    };
    public byte[] Flags { get; set; } = { 0x00, 0x04, 0x00, 0x00 };
    public byte EngineState { get; set; } = 2; // TEMP
    public byte PathState { get; set; } = 1;
    public BaseEntity Owner { get; set; }
    public Dictionary<byte, SeatConfig> Occupants { get; set; } = new Dictionary<byte, SeatConfig>()
    {
        {
            0,
            new SeatConfig
            {
                Occupant = null,
                Role = AttachmentRole.None,
                Posture = 0,
            }
        },
        {
            1,
            new SeatConfig
            {
                Occupant = null,
                Role = AttachmentRole.None,
                Posture = 0,
            }
        },
        {
            2,
            new SeatConfig
            {
                Occupant = null,
                Role = AttachmentRole.None,
                Posture = 0,
            }
        },
        {
            3,
            new SeatConfig
            {
                Occupant = null,
                Role = AttachmentRole.None,
                Posture = 0,
            }
        },
        {
            4,
            new SeatConfig
            {
                Occupant = null,
                Role = AttachmentRole.None,
                Posture = 0,
            }
        },
        {
            5,
            new SeatConfig
            {
                Occupant = null,
                Role = AttachmentRole.None,
                Posture = 0,
            }
        },
    };
    public Dictionary<byte, DeployableIdsData> DeployableData { get; set; } = new Dictionary<byte, DeployableIdsData>()
    {
        { 0, new DeployableIdsData { Target = new AeroMessages.Common.EntityId { Backing = 0 }, Unk1 = 0, Unk2 = 0 } },
        { 1, new DeployableIdsData { Target = new AeroMessages.Common.EntityId { Backing = 0 }, Unk1 = 0, Unk2 = 0 } },
        { 2, new DeployableIdsData { Target = new AeroMessages.Common.EntityId { Backing = 0 }, Unk1 = 0, Unk2 = 0 } },
        { 3, new DeployableIdsData { Target = new AeroMessages.Common.EntityId { Backing = 0 }, Unk1 = 0, Unk2 = 0 } },
        { 4, new DeployableIdsData { Target = new AeroMessages.Common.EntityId { Backing = 0 }, Unk1 = 0, Unk2 = 0 } },
        { 5, new DeployableIdsData { Target = new AeroMessages.Common.EntityId { Backing = 0 }, Unk1 = 0, Unk2 = 0 } },
        { 6, new DeployableIdsData { Target = new AeroMessages.Common.EntityId { Backing = 0 }, Unk1 = 0, Unk2 = 0 } },
        { 7, new DeployableIdsData { Target = new AeroMessages.Common.EntityId { Backing = 0 }, Unk1 = 0, Unk2 = 0 } },
        { 8, new DeployableIdsData { Target = new AeroMessages.Common.EntityId { Backing = 0 }, Unk1 = 0, Unk2 = 0 } },
        { 9, new DeployableIdsData { Target = new AeroMessages.Common.EntityId { Backing = 0 }, Unk1 = 0, Unk2 = 0 } },
    };
    public Dictionary<byte, uint> Abilities { get; set; } = new Dictionary<byte, uint>()
    {
        { 0, 0 },
        { 1, 0 },
        { 2, 0 },
        { 3, 0 },
        { 4, 0 },
        { 5, 0 },
        { 6, 0 },
        { 7, 0 },
        { 8, 0 },
    };

    public SpawnPoseData SpawnPose { get; set; }
    public Vector3 SpawnVelocity { get; set; } = Vector3.Zero;
    public CurrentPoseData CurrentPose { get; set; }
    public HostilityInfoData HostilityInfo { get; set; }
    public ProcessDelayData ProcessDelay { get; set; }
    public ScopeBubbleInfoData ScopeBubble { get; set; }
    public uint ScalingLevel { get; set; }

    public uint CurrentHealth { get; set; }
    public uint MaxHealth { get; set; }
    public uint CurrentShields { get; set; } = 0;
    public uint MaxShields { get; set; } = 0;
    public uint CurrentResources { get; set; } = 0;
    public uint MaxResources { get; set; } = 0;
    public byte WaterLevelAndDesc { get; set; }
    public byte EffectsFlags { get; set; }
    public byte SinFlags { get; set; }
    public byte SinFlagsPrivate { get; set; }

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

    public uint SpawnAbility { get; set; } = 0;
    public uint DespawnAbility { get; set; } = 0;
    public uint DeathAbility { get; set; } = 0;
    
    public void Load(VehicleInfoResult vehicleInfo)
    {
        VehicleId = vehicleInfo.VehicleId;
        SpawnAbility = vehicleInfo.SpawnAbility;
        DespawnAbility = vehicleInfo.DespawnAbility;
        DeathAbility = vehicleInfo.DeathAbility;
        CurrentHealth = (uint)vehicleInfo.MaxHitPoints;
        MaxHealth = (uint)vehicleInfo.MaxHitPoints;

        // Create seat configuration
        if (vehicleInfo.HasDriverSeat)
        {
            Occupants[0].Role = AttachmentRole.Driver;
            Occupants[0].Posture = vehicleInfo.DriverPosture;
        }

        if (vehicleInfo.MaxPassengers > 0)
        {
            byte firstIdx = (byte)(Occupants[0].Role == AttachmentRole.None ? 0 : 1);
            for (byte idx = firstIdx; idx < vehicleInfo.MaxPassengers - 1; idx++)
            {
                Occupants[idx].Role = vehicleInfo.HasActivePassenger ? AttachmentRole.ActivePassenger : AttachmentRole.PassivePassenger;
                Occupants[idx].Posture = vehicleInfo.PassengerPosture;
            }
        }

        // TODO: Handle Turrets, Deployables, Abilities

        // Hack to just refresh everything by recreating views.
        InitViews();
        InitControllers();
    }

    public void SetAimDirection(Vector3 newDirection)
    {
        AimDirection = newDirection;
        RefreshCurrentPose();
    }

    public void SetControllingPlayer(INetworkPlayer player)
    {
        if (player == null)
        {
            if (ControllingPlayer != null)
            {
                ControllingPlayer.AssignedShard.EntityMan.RemoveControllers(ControllingPlayer, this);
                ControllingPlayer = null;
            }
        }
        else
        {
            ControllingPlayer = player;
            InitControllers();
        }
    }

    public void SetOwningPlayer(INetworkPlayer player)
    {
        OwningPlayer = player;
    }

    public void SetOwner(BaseEntity entity)
    {
        Owner = entity;

        Vehicle_ObserverView.OwnerIdProp = new EntityId() { Backing = entity.EntityId };
        Vehicle_ObserverView.OwnerNameProp = string.Empty;
        Vehicle_ObserverView.OwnerLocalStringProp = 0;

        if (Vehicle_BaseController != null)
        {
            Vehicle_BaseController.OwnerIdProp = new EntityId() { Backing = entity.EntityId };
            Vehicle_BaseController.OwnerNameProp = string.Empty; // FIXME
            Vehicle_BaseController.OwnerLocalStringProp = 0; // FIXME
        }
    }

    public void SetPoseData(MovementInput poseData)
    {
        Position = poseData.Position;
        Rotation = poseData.Rotation;
        AimDirection = poseData.Direction;
        MovementState = (short)poseData.MovementState;
        MovementTime = poseData.Time;
        RefreshCurrentPose();
    }

    public void SetPosition(Vector3 newPosition)
    {
        Position = newPosition;
        RefreshCurrentPose();
    }

    public void SetRotation(Quaternion newRotation)
    {
        Rotation = newRotation;
        RefreshCurrentPose();
    }

    public void SetSpawnPose(SpawnPoseData newValue)
    {
        SpawnPose = newValue;
        if (Vehicle_BaseController != null)
        {
            Vehicle_BaseController.SpawnPoseProp = SpawnPose;
        }
    }

    public void SetSpawnVelocity(Vector3 newValue)
    {
        SpawnVelocity = newValue;
        Vehicle_MovementView.SpawnVelocityProp = SpawnVelocity;
        if (Vehicle_BaseController != null)
        {
            Vehicle_BaseController.SpawnVelocityProp = SpawnVelocity;
        }
    }

    public void SetWaterLevelAndDesc(byte newValue)
    {
        WaterLevelAndDesc = newValue;
        Vehicle_ObserverView.WaterLevelAndDescProp = WaterLevelAndDesc;
        if (Vehicle_BaseController != null)
        { 
            Vehicle_BaseController.WaterLevelAndDescProp = WaterLevelAndDesc;
        }
    }

    public void SetEffectsFlags(byte newValue)
    {
        EffectsFlags = newValue;
        Vehicle_ObserverView.EffectsFlagsProp = EffectsFlags;
    }

    public override void SetStatusEffect(byte index, ushort time, StatusEffectData data)
    {
        Console.WriteLine($"Vehicle.SetStatusEffect Index {index}, Time {time}, Id {data.Id}");

        // Member
        this.GetType().GetProperty($"StatusEffectsChangeTime_{index}").SetValue(this, time, null);
        this.GetType().GetProperty($"StatusEffects_{index}").SetValue(this, data, null);
        
        // CombatController
        Vehicle_CombatController.GetType().GetProperty($"StatusEffectsChangeTime_{index}Prop").SetValue(Vehicle_CombatController, time, null);
        Vehicle_CombatController.GetType().GetProperty($"StatusEffects_{index}Prop").SetValue(Vehicle_CombatController, data, null);
        
        // CombatView
        Vehicle_CombatView.GetType().GetProperty($"StatusEffectsChangeTime_{index}Prop").SetValue(Vehicle_CombatView, time, null);
        Vehicle_CombatView.GetType().GetProperty($"StatusEffects_{index}Prop").SetValue(Vehicle_CombatView, data, null);
    }

    public override void ClearStatusEffect(byte index, ushort time, uint debugEffectId)
    {
        Console.WriteLine($"Character.ClearStatusEffect Index {index}, Time {time}, Id {debugEffectId}");

        // Member
        this.GetType().GetProperty($"StatusEffectsChangeTime_{index}").SetValue(this, time, null);
        this.GetType().GetProperty($"StatusEffects_{index}").SetValue(this, null, null);
        
        // CombatController
        Vehicle_CombatController.GetType().GetProperty($"StatusEffectsChangeTime_{index}Prop").SetValue(Vehicle_CombatController, time, null);
        Vehicle_CombatController.GetType().GetProperty($"StatusEffects_{index}Prop").SetValue(Vehicle_CombatController, null, null);
        
        // CombatView
        Vehicle_CombatView.GetType().GetProperty($"StatusEffectsChangeTime_{index}Prop").SetValue(Vehicle_CombatView, time, null);
        Vehicle_CombatView.GetType().GetProperty($"StatusEffects_{index}Prop").SetValue(Vehicle_CombatView, null, null);
    }

    public byte GetNumberOfFreeSeats()
    {
        byte count = 0;
        foreach (var seat in Occupants.Values)
        {
            if (seat.Occupant == null && seat.Role != AttachmentRole.None)
            {
                count++;
            }
        }

        return count;
    }

    public bool IsEntitySeated(IEntity entity)
    {
        foreach (var seat in Occupants.Values)
        {
            if (seat.Occupant == entity)
            {
                return true;
            }
        }

        return false;
    }

    public override bool IsInteractable()
    {
        foreach (var seat in Occupants.Values)
        {
            if (seat.Occupant == null && seat.Role != AttachmentRole.None)
            {
                return true;
            }
        }

        return false;
    }

    public override bool CanBeInteractedBy(IEntity other)
    {
        /*
        * Require that other entity is a Character, since no other entities should be in the seats.
        * Require that there are free seats since the interaction is going to want to mount them.
        * If owned by a player and other entity is not that player, require that there is a free seat for the owner. This ensures 1 seat vehicles can't be stolen from a player.
        */
        var isInteractable = IsInteractable();
        var isCharacter = other.GetType() == typeof(Entities.Character.CharacterEntity);
        var numFreeSeats = GetNumberOfFreeSeats();
        if (IsPlayerOwned && other != Owner)
        {
            var isPlayerSeated = IsEntitySeated(Owner);
            if (!isPlayerSeated)
            {
                return isInteractable && isCharacter && numFreeSeats > 1;
            }
        }

        return isInteractable && isCharacter && numFreeSeats > 0;
    }

    public SeatConfig AddOccupant(CharacterEntity character)
    {
        SeatConfig seatConfig = null;
        foreach (var seat in Occupants.Values)
        {
            if (seat.Occupant == null && seat.Role != AttachmentRole.None)
            {
                seatConfig = seat;
                break;
            }
        }

        if (seatConfig == null)
        {
            return null;
        }
        else
        {
            seatConfig.Occupant = character;
            if (seatConfig.Role == AttachmentRole.Driver && character.IsPlayerControlled)
            {
                SetControllingPlayer(character.Player);
            }

            RefreshOccupants();
            return seatConfig;
        }
    }

    public void RemoveOccupant(CharacterEntity character)
    {
        foreach (var seat in Occupants.Values)
        {
            if (seat.Occupant == character)
            {
                Console.WriteLine($"RemoveOccupant found character");
                seat.Occupant = null;
                RefreshOccupants();
                if (character.IsPlayerControlled && character.Player == ControllingPlayer)
                {
                    Console.WriteLine($"RemoveOccupant set controlling player null");
                    SetControllingPlayer(null);
                }

                break;
            }
        }
    }

    private void InitFields()
    {
        HostilityInfo = new HostilityInfoData { Flags = 0 | HostilityInfoData.HostilityFlags.Faction, FactionId = 1 };
        ProcessDelay = new ProcessDelayData { Unk1 = 30721, Unk2 = 236 };
        ScopeBubble = new ScopeBubbleInfoData { Unk1 = 0, Unk2 = 0 };
    }

    private void InitControllers()
    {
        Vehicle_BaseController = new BaseController()
        {
            VehicleIdProp = VehicleId,
            ConfigurationProp = Configuration,
            FlagsProp = Flags,
            EngineStateProp = EngineState,
            PathStateProp = PathState,
            OwnerIdProp = new EntityId() { Backing = Owner?.EntityId ?? 0 },
            OwnerNameProp = string.Empty,
            OwnerLocalStringProp = 0,
            OccupantIds_0Prop = new EntityId() { Backing = Occupants[0].Occupant?.EntityId | 0x01 ?? 0 },
            OccupantIds_1Prop = new EntityId() { Backing = Occupants[1].Occupant?.EntityId | 0x01 ?? 0 },
            OccupantIds_2Prop = new EntityId() { Backing = Occupants[2].Occupant?.EntityId | 0x01 ?? 0 },
            OccupantIds_3Prop = new EntityId() { Backing = Occupants[3].Occupant?.EntityId | 0x01 ?? 0 },
            OccupantIds_4Prop = new EntityId() { Backing = Occupants[4].Occupant?.EntityId | 0x01 ?? 0 },
            OccupantIds_5Prop = new EntityId() { Backing = Occupants[5].Occupant?.EntityId | 0x01 ?? 0 },
            DeployableIds_0Prop = DeployableData[0],
            DeployableIds_1Prop = DeployableData[1],
            DeployableIds_2Prop = DeployableData[2],
            DeployableIds_3Prop = DeployableData[3],
            DeployableIds_4Prop = DeployableData[4],
            DeployableIds_5Prop = DeployableData[5],
            DeployableIds_6Prop = DeployableData[6],
            DeployableIds_7Prop = DeployableData[7],
            DeployableIds_8Prop = DeployableData[8],
            DeployableIds_9Prop = DeployableData[9],
            SnapMountProp = 0,
            SpawnPoseProp = SpawnPose,
            SpawnVelocityProp = SpawnVelocity,
            CurrentPoseProp = CurrentPose,
            ProcessDelayProp = ProcessDelay,
            HostilityInfoProp = HostilityInfo,
            PersonalFactionStanceProp = null,
            CurrentHealthProp = CurrentHealth,
            MaxHealthProp = MaxHealth,
            CurrentShieldsProp = CurrentShields,
            MaxShieldsProp = MaxShields,
            CurrentResourcesProp = CurrentResources,
            MaxResourcesProp = MaxResources,
            SinFlagsProp = SinFlags,
            SinFactionsAcquiredByProp = null,
            SinTeamsAcquiredByProp = null,
            WaterLevelAndDescProp = WaterLevelAndDesc,
            SinCardTypeProp = 0,
            ScopeBubbleInfoProp = ScopeBubble,
            ScalingLevelProp = ScalingLevel
        };

        Vehicle_CombatController = new CombatController()
        {
            SlottedAbility_0Prop = Abilities[0],
            SlottedAbility_1Prop = Abilities[1],
            SlottedAbility_2Prop = Abilities[2],
            SlottedAbility_3Prop = Abilities[3],
            SlottedAbility_4Prop = Abilities[4],
            SlottedAbility_5Prop = Abilities[5],
            SlottedAbility_6Prop = Abilities[6],
            SlottedAbility_7Prop = Abilities[7],
            SlottedAbility_8Prop = Abilities[8],
        };
    }

    private void InitViews()
    {
        Vehicle_ObserverView = new ObserverView()
        {
            VehicleTypeProp = VehicleId,
            ConfigurationProp = Configuration,
            FlagsProp = Flags,
            EngineStateProp = EngineState,
            PathStateProp = PathState,
            OwnerIdProp = new EntityId() { Backing = Owner?.EntityId ?? 0 },
            OwnerNameProp = string.Empty,
            OwnerLocalStringProp = 0,
            OccupantIds_0Prop = new EntityId() { Backing = Occupants[0].Occupant?.EntityId | 0x01 ?? 0 },
            OccupantIds_1Prop = new EntityId() { Backing = Occupants[1].Occupant?.EntityId | 0x01 ?? 0 },
            OccupantIds_2Prop = new EntityId() { Backing = Occupants[2].Occupant?.EntityId | 0x01 ?? 0 },
            OccupantIds_3Prop = new EntityId() { Backing = Occupants[3].Occupant?.EntityId | 0x01 ?? 0 },
            OccupantIds_4Prop = new EntityId() { Backing = Occupants[4].Occupant?.EntityId | 0x01 ?? 0 },
            OccupantIds_5Prop = new EntityId() { Backing = Occupants[5].Occupant?.EntityId | 0x01 ?? 0 },
            DeployableIds_0Prop = DeployableData[0],
            DeployableIds_1Prop = DeployableData[1],
            DeployableIds_2Prop = DeployableData[2],
            DeployableIds_3Prop = DeployableData[3],
            DeployableIds_4Prop = DeployableData[4],
            DeployableIds_5Prop = DeployableData[5],
            DeployableIds_6Prop = DeployableData[6],
            DeployableIds_7Prop = DeployableData[7],
            DeployableIds_8Prop = DeployableData[8],
            DeployableIds_9Prop = DeployableData[9],
            SnapMountProp = 0,
            ProcessDelayProp = ProcessDelay,
            HostilityInfoProp = HostilityInfo,
            PersonalFactionStanceProp = null,
            CurrentHealthProp = CurrentHealth,
            MaxHealthProp = MaxHealth,
            CurrentShieldsProp = CurrentShields,
            MaxShieldsProp = MaxShields,
            SinFlagsProp = SinFlags,
            SinFactionsAcquiredByProp = null,
            SinTeamsAcquiredByProp = null,
            WaterLevelAndDescProp = WaterLevelAndDesc,
            EffectsFlagsProp = EffectsFlags,
            SinCardTypeProp = 0,
            ScopeBubbleInfoProp = ScopeBubble,
            ScalingLevelProp = ScalingLevel
        };

        Vehicle_CombatView = new CombatView()
        {  
        };

        Vehicle_MovementView = new MovementView()
        {
            CurrentPoseProp = CurrentPose,
            SpawnVelocityProp = SpawnVelocity
        };
    }

    private void RefreshCurrentPose()
    {
        CurrentPose = new CurrentPoseData()
        {
            Position = Position,
            Rotation = Rotation,
            Direction = AimDirection,
            State = (ushort)MovementState,
            Time = Shard.CurrentTime + 200
        };

        Vehicle_BaseController.CurrentPoseProp = CurrentPose;
        Vehicle_MovementView.CurrentPoseProp = CurrentPose;
    }

    private void RefreshOccupants()
    {
        // Setting typecode byte to Character (as opposed to Generic) is neccessary
        var occupantIds0 = new EntityId() { Backing = Occupants[0].Occupant?.EntityId | 0x01 ?? 0 };
        var occupantIds1 = new EntityId() { Backing = Occupants[1].Occupant?.EntityId | 0x01 ?? 0 };
        var occupantIds2 = new EntityId() { Backing = Occupants[2].Occupant?.EntityId | 0x01 ?? 0 };
        var occupantIds3 = new EntityId() { Backing = Occupants[3].Occupant?.EntityId | 0x01 ?? 0 };
        var occupantIds4 = new EntityId() { Backing = Occupants[4].Occupant?.EntityId | 0x01 ?? 0 };
        var occupantIds5 = new EntityId() { Backing = Occupants[5].Occupant?.EntityId | 0x01 ?? 0 };

        Vehicle_ObserverView.OccupantIds_0Prop = occupantIds0;
        Vehicle_ObserverView.OccupantIds_1Prop = occupantIds1;
        Vehicle_ObserverView.OccupantIds_2Prop = occupantIds2;
        Vehicle_ObserverView.OccupantIds_3Prop = occupantIds3;
        Vehicle_ObserverView.OccupantIds_4Prop = occupantIds4;
        Vehicle_ObserverView.OccupantIds_5Prop = occupantIds5;
        Vehicle_ObserverView.SnapMountProp = 0;

        if (Vehicle_BaseController != null)
        {
            Vehicle_BaseController.OccupantIds_0Prop = occupantIds0;
            Vehicle_BaseController.OccupantIds_1Prop = occupantIds1;
            Vehicle_BaseController.OccupantIds_2Prop = occupantIds2;
            Vehicle_BaseController.OccupantIds_3Prop = occupantIds3;
            Vehicle_BaseController.OccupantIds_4Prop = occupantIds4;
            Vehicle_BaseController.OccupantIds_5Prop = occupantIds5;
            Vehicle_BaseController.SnapMountProp = 0;
        }
    }
}