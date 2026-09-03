using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AeroMessages.Common;
using AeroMessages.GSS;
using AeroMessages.GSS.Character;
using AeroMessages.GSS.Character.Controller;
using AeroMessages.GSS.Character.View;
using BepuUtilities;
using GameServer.Data;
using GameServer.Entities.Deployable;
using GameServer.Enums;
using GameServer.StaticDB;
using GameServer.StaticDB.Records.customdata;
using GameServer.StaticDB.Records.dbcharacter;
using GameServer.StaticDB.Records.dbitems;
using GameServer.StaticDB.Records.dbvisualrecords;
using GameServer.Systems.Aptitude;
using GameServer.Systems.Encounters;
using GameServer.Systems.MovementRelay;
using GameServer.Systems.WeaponSim;
using GameServer.Test;
using GrpcGameServerAPIClient;
using Serilog;
using GibVisuals = AeroMessages.GSS.Character.GibVisuals;
using LoadoutVisualType = AeroMessages.GSS.Character.LoadoutConfig_Visual.LoadoutVisualType;

namespace GameServer.Entities.Character;

/// <summary>
/// Base Character
/// </summary>
public sealed partial class CharacterEntity : BaseAptitudeEntity, IAptitudeTarget
{
    public const byte MaxMapMarkerCount = 64;
    private const int _maxMovementSamples = 32; // ~1.6s at 20 samples/s
    private const int _maxExtrapolationMs = 100; // How far to predict beyond the newest sample
    private const float _fallbackRunSpeed = 40.5f; // TODO: Derive from SDB/character stats
    private const float _fallbackSprintSpeed = 7.0f; // TODO: Derive from SDB/character stats
    private const float _fallbackCrouchSpeed = 2.5f; // TODO: Derive from SDB/character stats
    private readonly MapMarkerState[] _mapMarkers = new MapMarkerState[MaxMapMarkerCount];
    private readonly MovementSample[] _movementSamples = new MovementSample[_maxMovementSamples];
    private int _movementSampleCount;
    private int _movementSampleNewest;
    private ActiveWeaponDetails[,] _weaponDetailsCache;

    public CharacterEntity(IShard shard, ulong eid, CharacterEntity owner = null)
        : base(shard, eid, owner)
    {
        AeroEntityId = new EntityId() { Backing = EntityId, ControllerId = Controller.Character };

        CurrentStatModifiers = [];
        foreach (StatModifierIdentifier stat in Enum.GetValues(typeof(StatModifierIdentifier)))
        {
            CurrentStatModifiers.Add(stat, []);
        }

        InitFields();
        InitViews();
    }

    public BaseController Character_BaseController { get; set; }
    public CombatController Character_CombatController { get; set; }
    public MissionAndMarkerController Character_MissionAndMarkerController { get; set; }
    public LocalEffectsController Character_LocalEffectsController { get; set; }
    public SpectatorController Character_SpectatorController { get; set; }
    public ObserverView Character_ObserverView { get; set; }
    public EquipmentView Character_EquipmentView { get; set; }
    public CombatView Character_CombatView { get; set; }
    public MovementView Character_MovementView { get; set; }
    public TinyObjectView Character_TinyObjectView { get; set; }

    public new CharacterCollisionComponent Collision { get; set; }
    public INetworkPlayer Player { get; set; }
    public bool IsPlayerControlled => Player != null;
    public Vector3 Velocity { get; set; }
    public Vector3 AimDirection { get; set; }
    public short MovementState { get; set; }
    public ushort MovementShortTime { get; set; }
    public bool Alive { get; set; }
    public bool CanBleedout { get; set; }
    public short TimeSinceLastJump { get; set; }
    public bool IsAirborne { get; set; }
    public bool IsMoving { get => MovementStateContainer.Sprint || MovementStateContainer.Movement; }
    public bool IsCrouching { get => MovementStateContainer.Crouch; }
    public bool IsAttached { get => AttachedToEntity != null; }
    public bool IsAlive { get => CharacterState.State.Equals(CharacterStateData.CharacterStatus.Living); }

    public Dictionary<PermissionFlagsData.CharacterPermissionFlags, bool> CurrentPermissions { get; set; } = new Dictionary<PermissionFlagsData.CharacterPermissionFlags, bool>()
    {
        { PermissionFlagsData.CharacterPermissionFlags.movement, true },
        { PermissionFlagsData.CharacterPermissionFlags.sprint, true },
        { PermissionFlagsData.CharacterPermissionFlags.jump, true },
        { PermissionFlagsData.CharacterPermissionFlags.interact, true },
        { PermissionFlagsData.CharacterPermissionFlags.weapon, true },
        { PermissionFlagsData.CharacterPermissionFlags.melee, true },
        { PermissionFlagsData.CharacterPermissionFlags.abilities, true },
        { PermissionFlagsData.CharacterPermissionFlags.flashlight, true },
        { PermissionFlagsData.CharacterPermissionFlags.unk_8, false },
        { PermissionFlagsData.CharacterPermissionFlags.cheat_jump_midair, false },
        { PermissionFlagsData.CharacterPermissionFlags.glider, false },
        { PermissionFlagsData.CharacterPermissionFlags.unk_11, false },
        { PermissionFlagsData.CharacterPermissionFlags.jetpack, true },
        { PermissionFlagsData.CharacterPermissionFlags.map, true },
        { PermissionFlagsData.CharacterPermissionFlags.unk_14, true },
        { PermissionFlagsData.CharacterPermissionFlags.spectate_input, true },
        { PermissionFlagsData.CharacterPermissionFlags.new_character, false },
        { PermissionFlagsData.CharacterPermissionFlags.glider_hud, false },
        { PermissionFlagsData.CharacterPermissionFlags.crouch, true },
        { PermissionFlagsData.CharacterPermissionFlags.cheat_float, false },
        { PermissionFlagsData.CharacterPermissionFlags.detect_resources, false },
        { PermissionFlagsData.CharacterPermissionFlags.unk_21, true },
        { PermissionFlagsData.CharacterPermissionFlags.calldown_abilities, true },
        { PermissionFlagsData.CharacterPermissionFlags.unk_23, true },
        { PermissionFlagsData.CharacterPermissionFlags.emotes, true },
        { PermissionFlagsData.CharacterPermissionFlags.unk_25, true },
        { PermissionFlagsData.CharacterPermissionFlags.unk_26, true },
        { PermissionFlagsData.CharacterPermissionFlags.self_revive, true },
        { PermissionFlagsData.CharacterPermissionFlags.respawn_input, false },
        { PermissionFlagsData.CharacterPermissionFlags.free_repairs, false },
        { PermissionFlagsData.CharacterPermissionFlags.battleframe_abilities, true },
        { PermissionFlagsData.CharacterPermissionFlags.unk_31, true },
    };

    public ulong CurrentPermissionsValue => GetCurrentPermissionsValue();

    public StaticInfoData StaticInfo { get; set; }
    public ulong ArmyGUID { get; set; }
    public sbyte ArmyIsOfficer { get; set; }
    public CharacterStateData CharacterState { get; set; }
    public int TimePlayed { get; set; }
    public MaxVital MaxShields { get; private set; }
    public MaxVital MaxHealth { get; private set; }
    public int CurrentHealth { get; private set; }
    public int CurrentShields { get; private set; }
    public GibVisuals GibVisualsInfo { get; set; }
    public ProcessDelayData ProcessDelay { get; set; }
    public EmoteData Emote { get; set; }
    public DockedParamsData DockedParams { get; set; }
    public AssetOverridesField AssetOverrides { get; set; }
    public VisualOverridesField VisualOverrides { get; set; }
    public EquipmentData CurrentEquipment { get; set; }
    public CharacterStatsData CharacterStats { get; set; }
    public EnergyParamsData EnergyParams { get; set; }
    public ScopeBubbleInfoData ScopeBubble { get; set; }
    public CharacterSpawnPose SpawnPose { get; set; }
    public byte EffectsFlags { get; set; }
    public WeaponIndexData WeaponIndex { get; set; }
    public FireModeData FireMode_0 { get; set; }
    public FireModeData FireMode_1 { get; set; }
    public PermissionFlagsData PermissionFlags { get; set; }
    public AuthorizedTerminalData AuthorizedTerminal { get; set; } = new AuthorizedTerminalData { TerminalType = 0, TerminalId = 0, TerminalEntityId = 0 };
    public AttachedToData? AttachedTo { get; set; }
    public IEntity AttachedToEntity { get; set; }
    public int SelectedLoadout { get; set; }
    public List<DeployableEntity> OwnedDeployables { get; set; } = [];
    public RespawnTimesData? RespawnTimes { get; private set; }

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

    public CharacterLoadout CurrentLoadout { get; set; }

    public Dictionary<StatModifierIdentifier, Dictionary<uint, ActiveStatModifier>> CurrentStatModifiers { get; set; }
    public Dictionary<StatModifierIdentifier, float> BaseStatModifiers { get; set; } = new()
    {
        { StatModifierIdentifier.RunSpeedMult,         1.0f },
        { StatModifierIdentifier.FireRateModifier,     1.0f },
        { StatModifierIdentifier.FwdRunSpeedMult,      1.0f },
        { StatModifierIdentifier.JumpHeightMult,       1.0f },
        { StatModifierIdentifier.AirControlMult,       1.0f },
        { StatModifierIdentifier.ThrustStrengthMult,   1.0f },
        { StatModifierIdentifier.ThrustAirControl,     1.0f },
        { StatModifierIdentifier.Friction,             1.0f },
        { StatModifierIdentifier.AmmoConsumption,      1.0f },
        { StatModifierIdentifier.MaxTurnRate,          0.0f },
        { StatModifierIdentifier.TurnSpeed,            1.0f },
        { StatModifierIdentifier.TimeDilation,         1.0f },
        { StatModifierIdentifier.AccuracyModifier,     1.0f },
        { StatModifierIdentifier.GravityMult,          1.0f },
        { StatModifierIdentifier.AirResistanceMult,    1.0f },
        { StatModifierIdentifier.WeaponChargeupMod,    1.0f },
        { StatModifierIdentifier.WeaponDamageDealtMod, 1.0f },
    };

    internal MovementStateContainer MovementStateContainer { get; set; } = new();

    public override string ToString()
    {
        return IsPlayerControlled ? StaticInfo.DisplayName : base.ToString();
    }

    public void LoadMonster(uint typeId)
    {
        // TODO: GetMonsterVisualOptions
        var monsterInfo = SDBInterface.GetMonster(typeId);
        var chassisWarpaint = SDBUtils.GetChassisWarpaint(monsterInfo.ChassisId, monsterInfo.FullbodyWarpaintPaletteId, monsterInfo.ArmorWarpaintPaletteId, monsterInfo.BodysuitWarpaintPaletteId, monsterInfo.GlowWarpaintPaletteId);

        // TODO: Consider internalizing into the CharacterLoadout instead?
        var loadout = new CharacterLoadout
        {
            ChassisID = monsterInfo.ChassisId,
            BackpackID = monsterInfo.BackpackId,
            ChassisWarpaint = chassisWarpaint
        };
        loadout.SlottedItems[LoadoutSlotType.Primary] = monsterInfo.Weapon1Id;
        loadout.SlottedItems[LoadoutSlotType.Secondary] = monsterInfo.Weapon2Id;

        var ornaments = new List<uint>();
        if (monsterInfo.OrnamentsMapGroupId_1 != 0)
        {
            ornaments.Add(monsterInfo.OrnamentsMapGroupId_1);
        }

        if (monsterInfo.OrnamentsMapGroupId_2 != 0)
        {
            ornaments.Add(monsterInfo.OrnamentsMapGroupId_2);
        }

        if (monsterInfo.OrnamentsMapGroupId_3 != 0)
        {
            ornaments.Add(monsterInfo.OrnamentsMapGroupId_3);
        }

        if (monsterInfo.OrnamentsMapGroupId_4 != 0)
        {
            ornaments.Add(monsterInfo.OrnamentsMapGroupId_4);
        }

        SetStaticInfo(new StaticInfoData()
        {
            DisplayName = "_noname",
            UniqueName = string.Empty,
            Gender = (byte)(monsterInfo.Gender == 'F' ? 1 : 0),
            Race = monsterInfo.Race,
            CharInfoId = monsterInfo.CharinfoId,
            HeadMain = monsterInfo.HeadId,
            Eyes = monsterInfo.EyesId,
            Unk_1 = 0xff,
            TargetFlags = TargetFlags.IsNPC,
            StaffFlags = 0,
            CharacterTypeId = monsterInfo.Id,
            VoiceSet = monsterInfo.VoiceSet,
            TitleId = monsterInfo.Title,
            NameLocalizationId = monsterInfo.LocalizedNameId,
            HeadAccessories = [monsterInfo.HeadAcc1Id, monsterInfo.HeadAcc2Id],
            LoadoutVehicle = 0,
            LoadoutGlider = 0,
            Visuals = new VisualsBlock
            {
                Decals = [],
                Gradients = [],
                Colors =
                [
                    monsterInfo.SkinColor,
                    monsterInfo.LipColor,
                    monsterInfo.EyeColor,
                    monsterInfo.HairColor,
                    monsterInfo.FacialHairColor
                ],
                Palettes = [],
                Patterns = [],
                OrnamentGroupIds = [.. ornaments],
                CziMapAssetIds = [],
                MorphWeights = [],
                Overlays = []
            },
            ArmyTag = string.Empty
        });

        SetHostilityInfo(new HostilityInfoData
        {
            Flags = 0 | HostilityInfoData.HostilityFlags.Faction,
            FactionId = (byte)monsterInfo.FactionId
        });

        ApplyLoadout(loadout);

        // Temp hack to equip weapon
        if (monsterInfo.Weapon1Id != 0)
        {
            SetWeaponIndex(new WeaponIndexData()
            {
                Index = 1, Unk1 = 1, Unk2 = 0, Time = Shard.CurrentTime
            });
        }
        else if (monsterInfo.Weapon2Id != 0)
        {
            SetWeaponIndex(new WeaponIndexData()
            {
                Index = 2, Unk1 = 1, Unk2 = 0, Time = Shard.CurrentTime
            });
        }
    }

    public void LoadRemote(CharacterAndBattleframeVisuals remoteData)
    {
        Load(new BasicCharacterData()
        {
            CharacterInfo = new Data.BasicCharacterInfo()
            {
                Name = remoteData.CharacterInfo.Name,
                Gender = (byte)remoteData.CharacterInfo.Gender,
                Race = (byte)remoteData.CharacterInfo.Race,
                TitleId = (ushort)remoteData.CharacterInfo.TitleId,
                CurrentBattleframeSDBId = remoteData.CharacterInfo.CurrentBattleframeSDBId,
                ArmyGuid = remoteData.CharacterInfo.ArmyGuid,
                ArmyTag = remoteData.CharacterInfo.ArmyTag,
                ArmyIsOfficer = remoteData.CharacterInfo.ArmyIsOfficer,
                TimePlayed = (int)remoteData.CharacterInfo.TimePlayed,
            },
            CharacterVisuals = new Data.BasicCharacterVisuals()
            {
                Vehicle = (uint)remoteData.CharacterVisuals.Vehicle.Id,
                Glider = (uint)remoteData.CharacterVisuals.Glider.Id,

                Head = (uint)remoteData.CharacterVisuals.Head.Id,
                Eyes = (uint)remoteData.CharacterVisuals.Eyes.Id,
                VoiceSet = (uint)remoteData.CharacterVisuals.VoiceSet.Id,

                HeadAccessories = [.. remoteData.CharacterVisuals.HeadAccessories.ToList<WebIdValueColor>().Select(item => (uint)item.Id)],
                Ornaments = [.. remoteData.CharacterVisuals.Ornaments.ToList<WebId>().Select(item => (uint)item.Id)],

                SkinColor = remoteData.CharacterVisuals.SkinColor.Value.Color,
                LipColor = remoteData.CharacterVisuals.LipColor.Value.Color,
                EyeColor = remoteData.CharacterVisuals.EyeColor.Value.Color,
                HairColor = remoteData.CharacterVisuals.HairColor.Value.Color,
                FacialHairColor = remoteData.CharacterVisuals.FacialHairColor.Value.Color
            }
        });
    }

    public void Load(BasicCharacterData data)
    {
        var info = data.CharacterInfo;
        var visuals = data.CharacterVisuals;

        SetStaticInfo(new StaticInfoData
        {
            DisplayName = info.Name,
            UniqueName = info.Name,
            Gender = (byte)info.Gender,
            Race = (byte)info.Race,
            TitleId = info.TitleId,

            CharInfoId = 1,
            Unk_1 = 0xff,
            TargetFlags = 0,
            StaffFlags = 0x3,
            CharacterTypeId = 0,
            NameLocalizationId = 0,

            HeadMain = visuals.Head,
            Eyes = visuals.Eyes,
            VoiceSet = visuals.VoiceSet,
            HeadAccessories = visuals.HeadAccessories,
            LoadoutVehicle = visuals.Vehicle,
            LoadoutGlider = visuals.Glider,
            Visuals = new VisualsBlock
            {
                Decals = [],
                Gradients = [],
                Colors =
                [
                    visuals.SkinColor,
                    visuals.LipColor,
                    visuals.EyeColor,
                    visuals.HairColor,
                    visuals.FacialHairColor
                ],
                Palettes = [],
                Patterns = [],
                OrnamentGroupIds = visuals.Ornaments,
                CziMapAssetIds = [],
                MorphWeights = [],
                Overlays = []
            },
            ArmyTag = DataUtils.FormatArmyTag(info.ArmyTag)
        });

        SetTimePlayed(info.TimePlayed);
        SetArmyGUID(info.ArmyGuid);
        SetArmyIsOfficer((sbyte)(info.ArmyIsOfficer ? 1 : 0));
    }

    public void ApplyLoadout(CharacterLoadout loadout)
    {
        Shard.Admin.ApplyEquipmentOverrides(Player, loadout);
        CurrentLoadout = loadout;
        RebuildWeaponDetailsCache(loadout);

        var emptyVisuals = new VisualsBlock
        {
            Decals = [],
            Gradients = [],
            Colors = [],
            Palettes = [],
            Patterns = [],
            OrnamentGroupIds = [],
            CziMapAssetIds = [],
            MorphWeights = [],
            Overlays = []
        };

        var chassis = new SlottedItem
        {
            SdbId = loadout.ChassisID,
            SlotIndex = 255,
            Flags = 0,
            Unk2 = 0,
            Modules = loadout.GetChassisModules(),
            Visuals = loadout.GetChassisVisuals()
        };
        var backpack = new SlottedItem
        {
            SdbId = loadout.BackpackID,
            SlotIndex = 255,
            Flags = 0,
            Unk2 = 0,
            Modules = loadout.GetBackpackModules(),
            Visuals = emptyVisuals
        };
        var primary = new SlottedWeapon
        {
            Item = new SlottedItem
            {
                SdbId = loadout.SlottedItems.GetValueOrDefault(LoadoutSlotType.Primary),
                SlotIndex = 255,
                Flags = 0,
                Unk2 = 0,
                Modules = [],
                Visuals = new VisualsBlock
                {
                    Decals = [],
                    Gradients = [],
                    Colors = [0x322c0000, 0x543110a2, 0x65b42104],
                    Palettes = [],
                    Patterns = [],
                    OrnamentGroupIds = [],
                    CziMapAssetIds = [],
                    MorphWeights = [],
                    Overlays = []
                }
            },
            Unk1 = 0,
            Unk2 = 0
        };
        var secondary = new SlottedWeapon
        {
            Item = new SlottedItem
            {
                SdbId = loadout.SlottedItems.GetValueOrDefault(LoadoutSlotType.Secondary, 0u),
                SlotIndex = 255,
                Flags = 0,
                Unk2 = 0,
                Modules = [],
                Visuals = new VisualsBlock
                {
                    Decals = [],
                    Gradients = [],
                    Colors = [0x322c0000, 0x543110a2, 0x65b42104],
                    Palettes = [],
                    Patterns = [],
                    OrnamentGroupIds = [],
                    CziMapAssetIds = [],
                    MorphWeights = [],
                    Overlays = []
                }
            },
            Unk1 = 0,
            Unk2 = 0
        };

        SetCurrentEquipment(new EquipmentData
        {
            Chassis = chassis,
            Backpack = backpack,
            PrimaryWeapon = primary,
            SecondaryWeapon = secondary,
            EndUnk1 = 0,
            EndUnk2 = 0
        });

        SetCharacterStats(new CharacterStatsData
        {
            ItemAttributes = loadout.GetItemAttributes(),
            Unk1 = 0,
            WeaponA = loadout.GetPrimaryWeaponAttributes(),
            Unk2 = 0,
            WeaponB = loadout.GetSecondaryWeaponAttributes(),
            Unk3 = 0,
            AttributeCategories1 = loadout.GetItemModuleScalars(), // TODO: Compare with capture
            AttributeCategories2 = loadout.GetItemCharacterScalars()
        });

        SelectedLoadout = loadout.LoadoutID;
        Character_BaseController?.SelectedLoadoutProp = SelectedLoadout;

        if (chassis.SdbId != 0)
        {
            var gender = StaticInfo.Gender;
            var race = StaticInfo.Race;
            var charInfoId = StaticInfo.CharInfoId;

            CharInfo charInfo;
            Battleframe battleframeRecord;
            PoseType poseTypeRecord;
            List<BattleframeVisuals> battleframeVisualGroupRecords;
            BattleframeVisuals battleframeVisualGroupRecord = null;
            VisualRecord battleframeVisualRecord = null;

            try
            {
                charInfo = SDBInterface.GetCharInfo(charInfoId);
                battleframeRecord = SDBInterface.GetBattleframe(chassis.SdbId);
                poseTypeRecord = SDBInterface.GetPoseType(battleframeRecord.PosetypeId);
                battleframeVisualGroupRecords = SDBInterface.GetBattleframeVisuals(battleframeRecord.VisualGroup);

                // Find the appropriate visual record
                byte retries = 3;
                do
                {
                    foreach (var record in battleframeVisualGroupRecords)
                    {
                        bool matchesRace = record.Race == race;
                        bool matchesAnyRace = record.Race == 255;
                        bool matchesGender = (record.Gender == 'F' && gender == 1) || (record.Gender == 'M' && gender == 0);
                        bool matchesAnyGender = record.Gender == 'X';

                        bool valid = true;
                        switch (retries)
                        {
                            case 3:
                                // Pick exact match if found
                                valid = matchesRace && matchesGender;
                                break;
                            case 2:
                                // Otherwise, pick fallback if found
                                valid = matchesAnyRace && matchesAnyGender;
                                break;
                            case 1:
                                // Try to pick something reasonable
                                valid = matchesRace || matchesGender;
                                break;
                            case 0:
                                // Pick first result
                                valid = true;
                                break;
                        }

                        if (valid)
                        {
                            if (retries < 2)
                            {
                                Log.Warning("Picking uncertain Battleframe VisualRecord {recordId} of group {visualGroup} for chassi {chassiId}.", record.VisualrecId, battleframeRecord.VisualGroup, chassis.SdbId);
                            }

                            Log.Debug("Selected Battleframe VisualRecord {recordId} of group {visualGroup} for chassi {chassiId} (Had Gender {genderChar}, Race {raceId} ({raceStr}))", record.VisualrecId, battleframeRecord.VisualGroup, chassis.SdbId, gender == 1 ? "F" : "M", race, (CharacterRace)race);

                            battleframeVisualGroupRecord = record;
                            break;
                        }
                    }

                    retries--;
                }
                while (battleframeVisualRecord == null && retries > 0);

                battleframeVisualRecord = SDBInterface.GetVisualRecord(battleframeVisualGroupRecord.VisualrecId);
            }
            catch
            {
                Log.Error("Failed to get pose or visualrecord for chassi {chassiId}", chassis.SdbId);
                throw;
            }

            // We should have the data now since we survived
            Log.Debug(
                "ApplyLoadout Collision Debug | CharInfo: {id} ({name}) | RequiresRagdoll: {requiresRagdoll} | ChassisId: {chassisId} | PoseType: {poseId} | Physics: (R={radius}, H={height}, M={mass}) | VisualGroup: {visualGroup} | VisualRecord: {visualRecord} | StandingCollisionId: {standingCollisionId} | HitboxCollisionId: {hitboxCollisionId} | RagdollCollisionId: {ragdollCollisionId}",
                charInfo.Id,
                charInfo.Name,
                charInfo.RequiresRagdoll,
                chassis.SdbId,
                poseTypeRecord.PoseId,
                poseTypeRecord.PhysicsRadius,
                poseTypeRecord.PhysicsHeight,
                poseTypeRecord.PhysicsMass,
                battleframeRecord.VisualGroup,
                battleframeVisualRecord.Id,
                poseTypeRecord.StandingCollisionid,
                battleframeVisualRecord.HitboxCollisionId,
                battleframeVisualRecord.RagdollCollisionId);

            // Scale
            // max_rand_scale, min_rand_scale
            if (battleframeRecord.MinRandScale != battleframeRecord.MaxRandScale)
            {
                Log.Warning("Wtf battleframe {battleframe} has random scale: min: {min}, max: {max}", battleframeRecord.Id, battleframeRecord.MinRandScale, battleframeRecord.MaxRandScale);
            }

            Collision = new CharacterCollisionComponent
            {
                RequiresRagdoll = charInfo.RequiresRagdoll == 1,
                PoseTypeRecord = poseTypeRecord,
                RagdollCollisionId = battleframeVisualRecord.RagdollCollisionId,
                HitboxCollisionId = battleframeVisualRecord.HitboxCollisionId,
                Scale = battleframeRecord.MinRandScale,
            };
        }
    }

    public float GetItemAttribute(ushort id) => CurrentLoadout.ItemAttributes.GetValueOrDefault(id);

    public void AddStatModifier(uint reference, ActiveStatModifier mod)
    {
        CurrentStatModifiers[mod.Stat][reference] = mod;
        RefreshStatModifier(mod.Stat);
    }

    public void RemoveStatModifier(uint reference, StatModifierIdentifier stat)
    {
        if (CurrentStatModifiers[stat].ContainsKey(reference))
        {
            CurrentStatModifiers[stat].Remove(reference);
            RefreshStatModifier(stat);
        }
    }

    public void RefreshStatModifier(StatModifierIdentifier stat)
    {
        if (Character_CombatController != null)
        {
            StatMultiplierData value = new()
            {
                Value = GetCurrentStatModifierValue(stat),
                Time = Shard.CurrentTime,
            };

            Logger.Debug("StatModifier {Stat} set to {Value}", stat, value.Value);

            switch (stat)
            {
                case StatModifierIdentifier.RunSpeedMult:
                    Character_CombatController.RunSpeedMultProp = value;
                    break;
                case StatModifierIdentifier.FireRateModifier:
                    Character_CombatController.FireRateModifierProp = value;
                    break;
                case StatModifierIdentifier.FwdRunSpeedMult:
                    Character_CombatController.FwdRunSpeedMultProp = value;
                    break;
                case StatModifierIdentifier.JumpHeightMult:
                    Character_CombatController.JumpHeightMultProp = value;
                    break;
                case StatModifierIdentifier.AirControlMult:
                    Character_CombatController.AirControlMultProp = value;
                    break;
                case StatModifierIdentifier.ThrustStrengthMult:
                    Character_CombatController.ThrustStrengthMultProp = value;
                    break;
                case StatModifierIdentifier.ThrustAirControl:
                    Character_CombatController.ThrustAirControlProp = value;
                    break;
                case StatModifierIdentifier.Friction:
                    Character_CombatController.FrictionProp = value;
                    break;
                case StatModifierIdentifier.AmmoConsumption:
                    Character_CombatController.AmmoConsumptionProp = value;
                    break;
                case StatModifierIdentifier.MaxTurnRate:
                    Character_CombatController.MaxTurnRateProp = value;
                    break;
                case StatModifierIdentifier.TurnSpeed:
                    Character_CombatController.TurnSpeedProp = value;
                    break;
                case StatModifierIdentifier.TimeDilation:
                    Character_CombatController.TimeDilationProp = value;
                    break;
                case StatModifierIdentifier.AccuracyModifier:
                    Character_CombatController.AccuracyModifierProp = value;
                    break;
                case StatModifierIdentifier.GravityMult:
                    Character_CombatController.GravityMultProp = value;
                    break;
                case StatModifierIdentifier.AirResistanceMult:
                    Character_CombatController.AirResistanceMultProp = value;
                    break;
                case StatModifierIdentifier.WeaponChargeupMod:
                    Character_CombatController.WeaponChargeupModProp = value;
                    break;
                case StatModifierIdentifier.WeaponDamageDealtMod:
                    Character_CombatController.WeaponDamageDealtModProp = value;
                    break;
            }
        }
    }

    public float GetCurrentStatModifierValue(StatModifierIdentifier stat)
    {
        float value = 0;
        try
        {
            value = BaseStatModifiers[stat];
        }
        catch
        {
            Logger.Warning("MISSING BaseStatModifier for {Stat}", stat);
        }

        foreach (ActiveStatModifier mod in CurrentStatModifiers[stat].Values)
        {
            if (mod.Op == 1)
            {
                value += mod.Value;
            }
            else if (mod.Op == 2)
            {
                value = (value * mod.Value) / 100;
            }
            else
            {
                Logger.Warning("GetCurrentStatModifierValue Unknown Op {Op}", mod.Op);
            }
        }

        return value;
    }

    public void SetCharacterStats(CharacterStatsData value)
    {
        CharacterStats = value;
        Character_EquipmentView.CharacterStatsProp = value;
        Character_BaseController?.CharacterStatsProp = value;
    }

    public void SetStaticInfo(StaticInfoData value)
    {
        StaticInfo = value;
        Character_ObserverView.StaticInfoProp = StaticInfo;
        Character_BaseController?.StaticInfoProp = StaticInfo;
    }

    public void SetTimePlayed(int value)
    {
        TimePlayed = value;
        Character_BaseController?.TimePlayedProp = TimePlayed;
    }

    public void SetArmyGUID(ulong value)
    {
        ArmyGUID = value;
        Character_ObserverView.ArmyGUIDProp = ArmyGUID;
        Character_BaseController?.ArmyGUIDProp = ArmyGUID;
    }

    public void SetArmyIsOfficer(sbyte value)
    {
        ArmyIsOfficer = value;
        Character_BaseController?.ArmyIsOfficerProp = ArmyIsOfficer;
    }

    public void SetCurrentEquipment(EquipmentData value)
    {
        CurrentEquipment = value;
        Character_EquipmentView.CurrentEquipmentProp = CurrentEquipment;
        Character_BaseController?.CurrentEquipmentProp = CurrentEquipment;
    }

    public void SetAimDirection(Vector3 newDirection)
    {
        AimDirection = newDirection;
        RefreshMovementView();
    }

    public void SetCharacterState(CharacterStateData.CharacterStatus characterStatus, uint time)
    {
        CharacterState = new CharacterStateData
        {
            State = characterStatus, Time = time
        };
        Character_ObserverView.CharacterStateProp = CharacterState;
        Character_BaseController?.CharacterStateProp = CharacterState;
    }

    public void SetControllingPlayer(INetworkPlayer player)
    {
        Player = player;
        InitControllers();
    }

    public void SetEffectsFlags(byte value)
    {
        EffectsFlags = value;
        Character_ObserverView.EffectsFlagsProp = EffectsFlags;
    }

    public void SetEmote(EmoteData value)
    {
        Emote = value;
        Character_ObserverView.EmoteIDProp = value;
        Character_BaseController?.EmoteIDProp = value;
    }

    public void SetFireBurst(uint time)
    {
        Character_CombatView.WeaponBurstFiredProp = time;
    }

    public void SetFireCancel(uint time)
    {
        Character_CombatView.WeaponBurstCancelledProp = time;
    }

    public void SetFireEnd(uint time)
    {
        Character_CombatView.WeaponBurstEndedProp = time;
    }

    public void SetFireMode(byte index, FireModeData value)
    {
        switch (index)
        {
            case 0:
                FireMode_0 = value;
                Character_CombatView.FireMode_0Prop = FireMode_0;
                Character_CombatController?.FireMode_0Prop = FireMode_0;

                break;
            case 1:
                FireMode_1 = value;
                Character_CombatView.FireMode_1Prop = FireMode_1;
                Character_CombatController?.FireMode_1Prop = FireMode_1;

                break;
        }
    }

    public void SetPoseData(MovementPoseData poseData, ushort shortTime)
    {
        Position = poseData.PosRotState.Pos;
        Orientation = poseData.PosRotState.Rot;
        MovementState = poseData.PosRotState.MovementState;
        Velocity = poseData.Velocity;
        AimDirection = poseData.Aim;
        MovementShortTime = shortTime;
        RefreshMovementView();
    }

    public void SetPosition(Vector3 newPosition)
    {
        Position = newPosition;
        RefreshMovementView();
    }

    public void SetWeaponReloaded(uint time)
    {
        Character_CombatView.WeaponReloadedProp = time;
    }

    public void SetWeaponReloadCancelled(uint time)
    {
        Character_CombatView.WeaponReloadCancelledProp = time;
    }

    public void SetOrientation(Quaternion newOrientation)
    {
        Orientation = newOrientation;
        RefreshMovementView();
    }

    public void PositionAtSpawnPoint(SpawnPoint spawnPoint)
    {
        Position = spawnPoint.Position;
        Orientation = spawnPoint.Orientation;
        AimDirection = spawnPoint.AimDirection;
        RefreshMovementView();
    }

    public void SetSpawnPose()
    {
        SpawnPose = new CharacterSpawnPose
        {
            Time = Shard.CurrentTime,
            Position = Position,
            Rotation = Orientation,
            AimDirection = AimDirection,
            Velocity = Velocity,
            MovementState = 0x1000,
            Unk1 = 0,
            Unk2 = 0,
            JetpackEnergy = 0x639c,
            AirGroundTimer = 0,
            JumpTimer = 0,
            HaveDebugData = 0
        };
        Character_ObserverView.SpawnTimeProp = Shard.CurrentTime;
        if (Character_BaseController != null)
        {
            Character_BaseController.SpawnPoseProp = SpawnPose;
            Character_BaseController.SpawnTimeProp = Shard.CurrentTime;
        }
    }

    public void SetSpawnTime(uint time)
    {
        Character_ObserverView.SpawnTimeProp = time;
        Character_BaseController?.SpawnTimeProp = time;
    }

    public void SetNpcType(ushort npcType)
    {
        Character_ObserverView.NPCTypeProp = npcType;
    }

    public void SetWeaponIndex(WeaponIndexData value)
    {
        WeaponIndex = value;
        Character_CombatView.WeaponIndexProp = value;

        Character_CombatController?.WeaponIndexProp = value;
    }

    public void SetPermissionFlag(PermissionFlagsData.CharacterPermissionFlags flag, bool value)
    {
        CurrentPermissions[flag] = value;
        PermissionFlags = new PermissionFlagsData
        {
            Time = Shard.CurrentTime,
            Value = (PermissionFlagsData.CharacterPermissionFlags)GetCurrentPermissionsValue(),
        };

        Character_CombatController?.PermissionFlagsProp = PermissionFlags;
    }

    public void SetGliderProfileId(uint profileId)
    {
        Character_CombatController?.GliderProfileIdProp = profileId;
    }

    public void SetHoverProfileId(uint profileId)
    {
        Character_CombatController?.HoverProfileIdProp = profileId;
    }

    public void SetAuthorizedTerminal(AuthorizedTerminalData value)
    {
        AuthorizedTerminal = value;

        Character_BaseController?.AuthorizedTerminalProp = AuthorizedTerminal;
    }

    public override void SetStatusEffect(byte index, ushort time, StatusEffectData data)
    {
        Logger.Debug("Character.SetStatusEffect Index {Index}, Time {Time}, Id {Id}", index, time, data.Id);

        // Member
        GetType().GetProperty($"StatusEffectsChangeTime_{index}").SetValue(this, time, null);
        GetType().GetProperty($"StatusEffects_{index}").SetValue(this, data, null);

        // CombatController
        if (Character_CombatController != null)
        {
            Character_CombatController.GetType().GetProperty($"StatusEffectsChangeTime_{index}Prop").SetValue(Character_CombatController, time, null);
            Character_CombatController.GetType().GetProperty($"StatusEffects_{index}Prop").SetValue(Character_CombatController, data, null);
        }

        // CombatView
        Character_CombatView.GetType().GetProperty($"StatusEffectsChangeTime_{index}Prop").SetValue(Character_CombatView, time, null);
        Character_CombatView.GetType().GetProperty($"StatusEffects_{index}Prop").SetValue(Character_CombatView, data, null);
    }

    public override void ClearStatusEffect(byte index, ushort time, uint debugEffectId)
    {
        Logger.Debug("Character.ClearStatusEffect Index {Index}, Time {Time}, Id {DebugEffectId}", index, time, debugEffectId);

        // Member
        GetType().GetProperty($"StatusEffectsChangeTime_{index}").SetValue(this, time, null);
        GetType().GetProperty($"StatusEffects_{index}").SetValue(this, null, null);

        // CombatController
        if (Character_CombatController != null)
        {
            Character_CombatController.GetType().GetProperty($"StatusEffectsChangeTime_{index}Prop").SetValue(Character_CombatController, time, null);
            Character_CombatController.GetType().GetProperty($"StatusEffects_{index}Prop").SetValue(Character_CombatController, null, null);
        }

        // CombatView
        Character_CombatView.GetType().GetProperty($"StatusEffectsChangeTime_{index}Prop").SetValue(Character_CombatView, time, null);
        Character_CombatView.GetType().GetProperty($"StatusEffects_{index}Prop").SetValue(Character_CombatView, null, null);
    }

    public void SetAttachedTo(AttachedToData newValue, IEntity entity, uint pose, Vector3 poseOffset)
    {
        AttachedToEntity = entity;
        AttachedTo = newValue;
        Collision.AttachmentPoseId = pose;
        Collision.AttachmentPoseOffset = poseOffset;
        Character_ObserverView.AttachedToProp = AttachedTo;
        Character_BaseController?.AttachedToProp = AttachedTo;
    }

    public void ClearAttachedTo()
    {
        AttachedToEntity = null;
        AttachedTo = null;
        Collision.AttachmentPoseId = 0;
        Collision.AttachmentPoseOffset = Vector3.Zero;
        Character_ObserverView.AttachedToProp = AttachedTo;
        Character_ObserverView.SnapMountProp = 0;
        if (Character_BaseController != null)
        {
            Character_BaseController.AttachedToProp = AttachedTo;
            Character_BaseController.SnapMountProp = 0;
        }
    }

    public void HackClearAllStatusEffects()
    {
        var time = Shard.CurrentShortTime;
        for (int index = 0; index < 32; index++)
        {
            Logger.Debug("Character.ClearStatusEffect Index {Index}, Time {Time}", index, time);

            // Member
            GetType().GetProperty($"StatusEffectsChangeTime_{index}").SetValue(this, time, null);
            GetType().GetProperty($"StatusEffects_{index}").SetValue(this, null, null);

            // CombatController
            if (Character_CombatController != null)
            {
                Character_CombatController.GetType().GetProperty($"StatusEffectsChangeTime_{index}Prop").SetValue(Character_CombatController, time, null);
                Character_CombatController.GetType().GetProperty($"StatusEffects_{index}Prop").SetValue(Character_CombatController, null, null);
            }

            // CombatView
            Character_CombatView.GetType().GetProperty($"StatusEffectsChangeTime_{index}Prop").SetValue(Character_CombatView, time, null);
            Character_CombatView.GetType().GetProperty($"StatusEffects_{index}Prop").SetValue(Character_CombatView, null, null);
        }

        Shard.EntityMan.FlushChanges(this);
    }

    public void AddMapMarker(ulong encounterId, PersonalMapMarkerData data)
    {
        byte firstFreeIndex = InvalidIndex;
        for (byte i = 0; i < MaxMapMarkerCount; i++)
        {
            if (_mapMarkers[i] == null)
            {
                firstFreeIndex = i;
                break;
            }
        }

        if (firstFreeIndex == InvalidIndex)
        {
            Logger.Warning("AddMapMarkers but there are too many active map markers!");
            firstFreeIndex = MaxMapMarkerCount - 1; // Lets not crash
        }

        var state = new MapMarkerState
        {
            EncounterId = data.EncounterId,
            EncounterMarkerId = data.EncounterMarkerId,
        };

        _mapMarkers[firstFreeIndex] = state;

        SetMapMarker(firstFreeIndex, data);
    }

    public void RemoveEncounterMapMarkers(ulong encounterId)
    {
        for (byte i = 0; i < _mapMarkers.Length; i++)
        {
            if (_mapMarkers[i] == null)
            {
                continue;
            }

            if (_mapMarkers[i].EncounterId.Backing == encounterId)
            {
                SetMapMarker(i, null);
            }
        }
    }

    public void SetCombatFlags(CombatFlagsData value)
    {
        Character_CombatController.CombatFlagsProp = value;
        Character_CombatView.CombatFlagsProp = value;
    }

    public void EquipItemByGUID(int loadoutId, LoadoutSlotType slot, ulong guid)
    {
        Player.Inventory.EquipItemByGUID(loadoutId, slot, guid);
        ApplyLoadout(CurrentLoadout);
    }

    public void EquipVisualBySdbId(int loadoutId, LoadoutVisualType visualSlot, LoadoutSlotType slot, uint sdb_id)
    {
        Player.Inventory.EquipVisualBySdbId(loadoutId, visualSlot, slot, sdb_id);
        Player.CharacterEntity.CurrentLoadout.GliderID = sdb_id;
        ApplyLoadout(CurrentLoadout);
    }

#nullable enable

    public Dictionary<ushort, float> GetActiveWeaponAttributes()
    {
        var details = GetActiveWeaponDetails();
        return details?.Attributes ?? [];
    }

    public ActiveWeaponDetails? GetActiveWeaponDetails()
    {
        if (_weaponDetailsCache == null)
        {
            return null;
        }

        byte index = WeaponIndex.Index;
        if (index > 2)
        {
            Logger.Warning("GetActiveWeaponDetails failed because invalid selected weapon index {Index}", index);
            return null;
        }

        return _weaponDetailsCache[index, IsAltFireMode() ? 1 : 0];
    }

    public byte GetActiveFireModeIndex()
    {
        return (byte)(IsAltFireMode() ? 1 : 0);
    }

    public ActiveWeaponDetails? GetWeaponDetails(byte modeIndex)
    {
        if (_weaponDetailsCache == null)
        {
            return null;
        }

        byte index = WeaponIndex.Index;
        if (index > 2 || modeIndex > 1)
        {
            return null;
        }

        return _weaponDetailsCache[index, modeIndex];
    }
    #nullable disable

    public Vector3 GetProjectileOrigin()
    {
        return GetProjectileOrigin(AimDirection);
    }

    /// <summary>
    ///     Returns the muzzle height for NPC projectiles. It is the top of
    ///     the scaled physics capsule of the rig. The AIEngine value
    ///     NpcMuzzleFraction adjusts it.
    /// </summary>
    public float GetNpcMuzzleHeight()
    {
        var height = 1.62f;
        if (Collision is CharacterCollisionComponent { PoseTypeRecord: not null } collision
            && collision.PoseTypeRecord.PhysicsHeight > 0.1f)
        {
            height = collision.PoseTypeRecord.PhysicsHeight * Math.Max(collision.Scale, 0.1f);
        }

        return Math.Clamp(height * AIEngine.NpcMuzzleFraction, 0.3f, 4f);
    }

    public Vector3 GetProjectileOrigin(Vector3 aimDirection)
    {
        return CalculateProjectileOrigin(Position, Orientation, IsCrouching, aimDirection);
    }

    /// <summary>
    ///     Gets the projectile origin at the given client time, using the interpolated/predicted pose
    /// </summary>
    /// <param name="clientTimeMs">Client time in ms to evaluate the pose at</param>
    /// <param name="aimDirection">The aim direction at the time of firing</param>
    /// <param name="shooterVelocity">Optional shooter velocity at the time of firing, used to seed the prediction</param>
    /// <returns>The world space projectile origin</returns>
    public Vector3 GetProjectileOrigin(uint clientTimeMs, Vector3 aimDirection, Vector3? shooterVelocity = null)
    {
        TryGetInterpolatedPose(clientTimeMs, shooterVelocity, out var position, out var orientation, out var movementState);
        var crouching = new MovementStateContainer { MovementStateValue = (ushort)movementState }.Crouch;
        return CalculateProjectileOrigin(position, orientation, crouching, aimDirection);
    }

    /// <summary>
    ///     Records a movement sample from a client MovementInput command.
    ///     Only called on the shard thread. Samples older than the newest recorded one are ignored.
    /// </summary>
    /// <param name="sample">The movement sample to record</param>
    public void RecordMovementSample(MovementSample sample)
    {
        if (_movementSampleCount > 0)
        {
            var newest = _movementSamples[_movementSampleNewest];
            if (MovementSample.MsSince(newest.ShortTime, sample.ShortTime) > 0x8000)
            {
                return; // Out-of-order or stale sample
            }
        }

        var index = (_movementSampleNewest + 1) % _maxMovementSamples;
        _movementSamples[index] = sample;
        _movementSampleNewest = index;
        if (_movementSampleCount < _maxMovementSamples)
        {
            _movementSampleCount++;
        }
    }

    /// <summary>
    ///     Gets the character position at the given client time, interpolating between the two newest
    ///     movement samples when the time falls between them, or extrapolating from the newest sample
    ///     when the time is in the future relative to it.
    /// </summary>
    /// <param name="clientTimeMs">Client time in ms to evaluate the position at</param>
    /// <param name="shooterVelocity">Optional shooter velocity at the given time, used to seed the extrapolation</param>
    /// <returns>The estimated character position at the given time</returns>
    public Vector3 GetInterpolatedPosition(uint clientTimeMs, Vector3? shooterVelocity = null)
    {
        TryGetInterpolatedPose(clientTimeMs, shooterVelocity, out var position, out _, out _);
        return position;
    }

    /// <summary>
    ///     Gets the character pose (position, orientation, movement state) at the given client time,
    ///     using the same interpolation/extrapolation rules as <see cref="GetInterpolatedPosition" />.
    ///     Falls back to the current entity state when no samples have been recorded.
    /// </summary>
    /// <param name="clientTimeMs">Client time in ms to evaluate the pose at</param>
    /// <param name="shooterVelocity">Optional shooter velocity at the given time, used to seed the extrapolation</param>
    /// <param name="position">The estimated position at the given time</param>
    /// <param name="orientation">The estimated orientation at the given time</param>
    /// <param name="movementState">The estimated movement state value at the given time</param>
    /// <returns>True when the pose was derived from recorded samples, false when falling back to the current entity state</returns>
    public bool TryGetInterpolatedPose(uint clientTimeMs, Vector3? shooterVelocity, out Vector3 position, out Quaternion orientation, out short movementState)
    {
        position = Position;
        orientation = Orientation;
        movementState = MovementState;

        if (_movementSampleCount == 0)
        {
            return false;
        }

        var target = (ushort)clientTimeMs;
        var newest = _movementSamples[_movementSampleNewest];
        var previous = _movementSampleCount > 1 ? _movementSamples[(_movementSampleNewest + _maxMovementSamples - 1) % _maxMovementSamples] : default;
        var ageSinceNewest = MovementSample.MsSince(newest.ShortTime, target);

        if (ageSinceNewest > 0x8000)
        {
            // Target is in the past relative to the newest sample (e.g. fire packet arrived before the next movement packet)
            if (_movementSampleCount > 1)
            {
                var ageSincePrevious = MovementSample.MsSince(previous.ShortTime, target);
                if (ageSincePrevious <= 0x8000)
                {
                    // Target lies between the two newest samples: interpolate
                    var span = MovementSample.MsSince(previous.ShortTime, newest.ShortTime);
                    var alpha = span > 0 ? ageSincePrevious / (float)span : 1f;
                    position = Vector3.Lerp(previous.Position, newest.Position, alpha);
                    QuaternionEx.Slerp(previous.Orientation, newest.Orientation, alpha, out orientation);
                    movementState = ageSincePrevious * 2 < span ? previous.MovementState : newest.MovementState;
                    return true;
                }

                // Target is before all buffered samples: use the oldest one
                var oldest = _movementSamples[(_movementSampleNewest - _movementSampleCount + 1 + _maxMovementSamples) % _maxMovementSamples];
                position = oldest.Position;
                orientation = oldest.Orientation;
                movementState = oldest.MovementState;
                return true;
            }

            position = newest.Position;
            orientation = newest.Orientation;
            movementState = newest.MovementState;
            return true;
        }

        // Target is at or after the newest sample: extrapolate, clamped to MaxExtrapolationMs
        var elapsedMs = Math.Min((int)ageSinceNewest, _maxExtrapolationMs);
        if (elapsedMs > 0)
        {
            var velocity = shooterVelocity ?? newest.Velocity;
            if (velocity.LengthSquared() < 0.25f)
            {
                var estimated = EstimateInputVelocity(newest);
                if (estimated != Vector3.Zero)
                {
                    velocity = estimated;
                }
            }

            position = newest.Position + (velocity * (elapsedMs / 1000f));
        }
        else
        {
            position = newest.Position;
        }

        orientation = newest.Orientation;
        movementState = newest.MovementState;
        return true;
    }

    public void SetHostilityInfo(HostilityInfoData newValue)
    {
        HostilityInfo = newValue;
        Character_ObserverView?.HostilityInfoProp = HostilityInfo;
        Character_BaseController?.HostilityInfoProp = HostilityInfo;
    }

    public void SetMaxHealth(int newValue, bool resetCurrent)
    {
        MaxHealth = new()
        {
            Value = newValue,
            Time = Shard.CurrentTime,
        };

        Character_ObserverView?.MaxHealthProp = MaxHealth;
        Character_BaseController?.MaxHealthProp = MaxHealth;

        if (resetCurrent)
        {
            SetCurrentHealth(MaxHealth.Value);
        }
        else
        {
            SetCurrentHealth(Math.Min(MaxHealth.Value, CurrentHealth));
        }
    }

    public void SetMaxShields(int newValue, bool resetCurrent)
    {
        MaxShields = new()
        {
            Value = newValue,
            Time = Shard.CurrentTime,
        };

        Character_BaseController?.MaxShieldsProp = MaxShields;

        if (resetCurrent)
        {
            SetCurrentShields(MaxShields.Value);
        }
        else
        {
            SetCurrentShields(Math.Min(MaxShields.Value, CurrentShields));
        }
    }

    public void SetCurrentHealth(int newValue)
    {
        CurrentHealth = Math.Min(Math.Max(0, newValue), MaxHealth.Value);
        byte pct = MaxHealth.Value > 0 ? (byte)(((float)CurrentHealth / MaxHealth.Value) * 100) : (byte)0;

        Character_ObserverView?.CurrentHealthPctProp = pct;
        Character_BaseController?.CurrentHealthProp = CurrentHealth;
    }

    public void SetCurrentShields(int newValue)
    {
        CurrentShields = Math.Min(Math.Max(0, newValue), MaxShields.Value);
        Character_BaseController?.CurrentShieldsProp = CurrentShields;
    }

    public void SetRespawnTimes(RespawnTimesData newValue)
    {
        RespawnTimes = newValue;
        Character_ObserverView?.RespawnTimesProp = RespawnTimes;
        Character_BaseController?.RespawnTimesProp = RespawnTimes;
    }

    public void SetGibVisualsInfo(uint gibVisualsId, uint time)
    {
        GibVisualsInfo = new GibVisuals { Id = gibVisualsId, Time = time };
        Character_ObserverView.GibVisualsIDProp = GibVisualsInfo;
        Character_BaseController?.GibVisualsIdProp = GibVisualsInfo;
    }

    public bool TryGetGibVisualsId(out uint gibVisualsId)
    {
        gibVisualsId = 0;
        uint chassisId = CurrentLoadout?.ChassisID ?? 0;
        if (chassisId == 0)
        {
            return false;
        }

        var battleframe = SDBInterface.GetBattleframe(chassisId);
        if (battleframe == null || battleframe.GibsetId == 0)
        {
            return false;
        }

        gibVisualsId = battleframe.GibsetId;
        return true;
    }

    public ulong GetCurrentPermissionsValue()
    {
        ulong result = 0ul;
        foreach (var pair in CurrentPermissions)
        {
            if (pair.Value)
            {
                result += (ulong)pair.Key;
            }
        }

        return result;
    }

    private static Vector3 CalculateProjectileOrigin(Vector3 position, Quaternion orientation, bool crouching, Vector3 aimDirection)
    {
        var muzzleBase = new Vector3(0.2f, 0.0f, 1.62f); // TODO: Should probably vary by character
        if (crouching)
        {
            muzzleBase.Z = 1.08f;
        }

        var muzzleBaseWorld = QuaternionEx.Transform(muzzleBase, QuaternionEx.Inverse(orientation)); // Match the characters orientation
        var muzzleOffset = new Vector3(aimDirection.X, aimDirection.Y, aimDirection.Z) * 0.1f; // Offset like a sphere based on aim
        var muzzleOffsetWorld = muzzleBaseWorld + muzzleOffset; // Apply offset to base in world
        return position + muzzleOffsetWorld; // Translate to character
    }

    /// <summary>
    ///     Estimates the velocity the character is starting to move at, based on the movement input
    ///     axes and modifiers of a sample, for use when the recorded velocity is near zero
    /// </summary>
    private static Vector3 EstimateInputVelocity(MovementSample sample)
    {
        var magnitude = MathF.Sqrt((sample.HorizontalInput * sample.HorizontalInput) + (sample.VerticalInput * sample.VerticalInput));
        if (magnitude < 1f)
        {
            return Vector3.Zero;
        }

        var state = new MovementStateContainer { MovementStateValue = (ushort)sample.MovementState };
        var speed = state.Sprint ? _fallbackSprintSpeed : state.Crouch ? _fallbackCrouchSpeed : _fallbackRunSpeed;

        // Match the orientation convention used by CalculateProjectileOrigin (local offset transformed by the inverse orientation)
        var forward = QuaternionEx.Transform(new Vector3(0f, 0f, 1f), QuaternionEx.Inverse(sample.Orientation));
        var right = QuaternionEx.Transform(new Vector3(1f, 0f, 0f), QuaternionEx.Inverse(sample.Orientation));
        var direction = (forward * (sample.VerticalInput / magnitude)) + (right * (sample.HorizontalInput / magnitude));
        return Vector3.Normalize(direction) * speed;
    }

    private static void BuildWeaponSlotDetails(ActiveWeaponDetails[,] cache, int index, uint weaponId, StatsData[] weaponAttributes)
    {
        if (weaponId == 0)
        {
            cache[index, 0] = ActiveWeaponDetails.Empty;
            cache[index, 1] = ActiveWeaponDetails.Empty;
            return;
        }

        var weaponDetails = SDBUtils.GetDetailedWeaponInfo(weaponId);
        if (weaponDetails == null)
        {
            cache[index, 0] = ActiveWeaponDetails.Empty;
            cache[index, 1] = ActiveWeaponDetails.Empty;
            return;
        }

        var attributes = weaponAttributes.ToDictionary(p => p.Id, p => p.Value);

        // Weapons missing the Weapon Spread attribute fall back to MinSpread/MaxSpread in WeaponSpreadProfile.Build.
        float? weaponAttributeSpread = attributes.TryGetValue((ushort)ItemAttributeId.WeaponSpread, out var spreadAttr) ? spreadAttr : null;
        float weaponAttributeRateOfFire = attributes.GetValueOrDefault((ushort)ItemAttributeId.RateOfFire, 1f);

        // Base/other are built for the active fire mode (Main or Alt/Underbarrel) with the
        // main weapon's Weapon Spread attribute scale, matching the client pipeline.
        float msPerBurstOverride = attributes.TryGetValue((ushort)ItemAttributeId.RateOfFire, out var rofAttr) ? rofAttr : 0f;

        var main = new ActiveWeaponDetails()
        {
            Weapon = weaponDetails.Main,
            WeaponId = weaponId,
            SpreadProfile = WeaponSpreadProfile.Build(weaponDetails.Main, weaponId, weaponAttributeSpread, weaponDetails.Main.MaxSpread, msPerBurstOverride),
            RateOfFire = weaponAttributeRateOfFire,
            Attributes = attributes,
        };

        cache[index, 0] = main;
        cache[index, 1] = weaponDetails.Alt != null
            ? new ActiveWeaponDetails()
            {
                Weapon = weaponDetails.Alt,
                WeaponId = weaponId,
                SpreadProfile = WeaponSpreadProfile.Build(weaponDetails.Alt, weaponId, weaponAttributeSpread, weaponDetails.Main.MaxSpread, msPerBurstOverride),
                RateOfFire = weaponAttributeRateOfFire,
                Attributes = attributes,
            }
            : main;
    }

    private bool IsAltFireMode()
    {
        return FireMode_0.Mode != 0 || FireMode_1.Mode != 0;
    }

    private void RebuildWeaponDetailsCache(CharacterLoadout loadout)
    {
        var cache = new ActiveWeaponDetails[3, 2];
        cache[0, 0] = ActiveWeaponDetails.Empty;
        cache[0, 1] = ActiveWeaponDetails.Empty;

        BuildWeaponSlotDetails(cache, 1, loadout.SlottedItems.GetValueOrDefault(LoadoutSlotType.Primary), loadout.GetPrimaryWeaponAttributes());
        BuildWeaponSlotDetails(cache, 2, loadout.SlottedItems.GetValueOrDefault(LoadoutSlotType.Secondary), loadout.GetSecondaryWeaponAttributes());

        _weaponDetailsCache = cache;
    }

    private void InitFields()
    {
        Position = new Vector3();
        Orientation = Quaternion.Identity;
        Velocity = new Vector3();
        AimDirection = new Vector3(0.70707911253f, 0.707134246826f, 0.000504541851114f); // Look kinda forward instead of up
        MovementState = 0x1000;
        MovementShortTime = Shard.CurrentShortTime;

        Alive = false;
        TimeSinceLastJump = 0;
        IsAirborne = false;

        StaticInfo = new StaticInfoData();
        CharacterState = new CharacterStateData { State = CharacterStateData.CharacterStatus.Living, Time = Shard.CurrentTime };
        HostilityInfo = new HostilityInfoData { Flags = 0 | HostilityInfoData.HostilityFlags.Faction, FactionId = 1 };
        SetMaxShields(0, true);
        SetMaxHealth(19192, true);
        GibVisualsInfo = new GibVisuals { Id = 0, Time = Shard.CurrentTime };
        ProcessDelay = new ProcessDelayData { Unk1 = 30721, Unk2 = 236 };
        Emote = new EmoteData { Id = 0, Time = 0 };
        DockedParams = new DockedParamsData { Unk1 = new EntityId { Backing = 0 }, Unk2 = Vector3.Zero, Unk3 = 0 };
        AssetOverrides = new AssetOverridesField { Ids = [] };
        VisualOverrides = new VisualOverridesField { Data = [] };
        CurrentEquipment = new EquipmentData { };
        CharacterStats = new CharacterStatsData
        {
            ItemAttributes =
            [
                new() { Id = 5, Value = 156.414169f }, new() { Id = 6, Value = 1037.8347f }, new() { Id = 7, Value = 177.44128f }, new() { Id = 12, Value = 16.250000f }, new() { Id = 35, Value = 300 },
                new() { Id = 36, Value = 250 }, new() { Id = 37, Value = 2.092090f }, new() { Id = 142, Value = 12.55f }, new() { Id = 143, Value = 1136 }, new() { Id = 144, Value = 18.433180f },
                new() { Id = 173, Value = 10 }, new() { Id = 186, Value = 11.40f }, new() { Id = 959, Value = 1 }, new() { Id = 1050, Value = 34.5f }, new() { Id = 1051, Value = 13.824884f },
                new() { Id = 1052, Value = 5.5f }, new() { Id = 1121, Value = 150 }, new() { Id = 1146, Value = 10.0f }, new() { Id = 1367, Value = 85 }, new() { Id = 1368, Value = 100 },
                new() { Id = 1370, Value = 65 }, new() { Id = 1371, Value = 120 }, new() { Id = 1372, Value = 140 }, new() { Id = 1377, Value = 140.531250f }, new() { Id = 1395, Value = 75 },
                new() { Id = 1419, Value = 32.769249f }, new() { Id = 1420, Value = 16901.744141f }, new() { Id = 1439, Value = 15279.667969f }, new() { Id = 1451, Value = 681 },
                new() { Id = 1583, Value = 1 }, new() { Id = 1620, Value = 5049.767090f }, new() { Id = 1622, Value = 8 }, new() { Id = 1733, Value = 1.800000f }, new() { Id = 1736, Value = 60 },
                new() { Id = 1737, Value = 5486.919434f }, new() { Id = 1746, Value = 9.320923f }, new() { Id = 1785, Value = 1.084000f }, new() { Id = 1835, Value = 5932.512207f },
                new() { Id = 1904, Value = 4 }, new() { Id = 1905, Value = 2 }, new() { Id = 1987, Value = 8 }, new() { Id = 2034, Value = 22 }, new() { Id = 2037, Value = 9887.518555f },
                new() { Id = 2039, Value = 9 }, new() { Id = 2042, Value = 12.252850f }
            ],
            Unk1 = 0,
            WeaponA = [],
            Unk2 = 0,
            WeaponB = [],
            Unk3 = 0,
            AttributeCategories1 = [],
            AttributeCategories2 = []
        };

        EnergyParams = new EnergyParamsData { Max = 1000.0f, Delay = 500, Recharge = 156.0f, Time = Shard.CurrentTime };
        ScopeBubble = new ScopeBubbleInfoData { Layer = 0, Unk2 = 0 };
        SpawnPose = new CharacterSpawnPose
        {
            Time = Shard.CurrentTime,
            Position = Position,
            Rotation = Orientation,
            AimDirection = AimDirection,
            Velocity = Velocity,
            MovementState = 0x1000,
            Unk1 = 0,
            Unk2 = 0,
            JetpackEnergy = 0x639c,
            AirGroundTimer = 0,
            JumpTimer = 0,
            HaveDebugData = 0
        };

        EffectsFlags = 0;
        FireMode_0 = new FireModeData { Mode = 0, Time = Shard.CurrentTime };
        FireMode_1 = new FireModeData { Mode = 0, Time = Shard.CurrentTime };
        WeaponIndex = new WeaponIndexData { Index = 0, Unk1 = 1, Unk2 = 0, Time = Shard.CurrentTime };

        PermissionFlags = new PermissionFlagsData
        {
            Time = Shard.CurrentTime,
            Value = (PermissionFlagsData.CharacterPermissionFlags)GetCurrentPermissionsValue(),
        };
    }

    private void InitControllers()
    {
        Character_BaseController = new BaseController
        {
            TimePlayedProp = TimePlayed,
            CurrentWeightProp = 0,
            EncumberedWeightProp = 255,
            AuthorizedTerminalProp = AuthorizedTerminal,
            PingTimeProp = 0, // Shard.CurrentTime,
            StaticInfoProp = StaticInfo,
            SpawnTimeProp = Shard.CurrentTime,
            VisualOverridesProp = VisualOverrides,
            CurrentEquipmentProp = CurrentEquipment,
            SelectedLoadoutProp = SelectedLoadout,
            SelectedLoadoutIsPvPProp = 0,
            GibVisualsIdProp = GibVisualsInfo,
            SpawnPoseProp = SpawnPose,
            ProcessDelayProp = ProcessDelay,
            SpectatorModeProp = 0,
            CinematicCameraProp = null,
            CharacterStateProp = CharacterState,
            HostilityInfoProp = HostilityInfo,
            PersonalFactionStanceProp = null,
            CurrentHealthProp = CurrentHealth,
            CurrentShieldsProp = CurrentShields,
            MaxShieldsProp = MaxShields,
            MaxHealthProp = MaxHealth,
            CurrentDurabilityPctProp = 100,
            EnergyParamsProp = EnergyParams,
            CharacterStatsProp = CharacterStats,
            EmoteIDProp = Emote,
            AttachedToProp = null,
            SnapMountProp = 0,
            SinFlagsProp = 0,
            SinFlagsPrivateProp = 0,
            SinFactionsAcquiredByProp = null,
            SinTeamsAcquiredByProp = null,
            ArmyGUIDProp = ArmyGUID,
            ArmyIsOfficerProp = ArmyIsOfficer,
            EncounterPartyTupleProp = null,
            DockedParamsProp = DockedParams,
            LookAtTargetProp = null,
            ZoneUnlocksProp = 0,
            RegionUnlocksProp = 0,
            ChatPartyLeaderIdProp = new EntityId { Backing = 0 },
            ScopeBubbleInfoProp = ScopeBubble,
            CarryableObjects_0Prop = null,
            CarryableObjects_1Prop = null,
            CarryableObjects_2Prop = null,
            CachedAssetsProp = null,
            RespawnTimesProp = null,
            ProgressionXpProp = 0,
            PermanentStatusEffectsProp = new PermanentStatusEffectsData { Effects = [] },
            XpBoostModifierProp = new StatModifierData { ModifierId = 0, StatValue = 0.0f },
            XpPermanentModifierProp = new StatModifierData { ModifierId = 0, StatValue = 0.0f },
            XpZoneModifierProp = new StatModifierData { ModifierId = 0, StatValue = 0.0f },
            XpVipModifierProp = new StatModifierData { ModifierId = 0, StatValue = 0.0f },
            XpEventModifierProp = new StatModifierData { ModifierId = 0, StatValue = 0.0f },
            ResourceBoostModifierProp = new StatModifierData { ModifierId = 0, StatValue = 0.0f },
            ResourcePermanentModifierProp = new StatModifierData { ModifierId = 0, StatValue = 0.0f },
            ResourceZoneModifierProp = new StatModifierData { ModifierId = 0, StatValue = 0.0f },
            ResourceVipModifierProp = new StatModifierData { ModifierId = 0, StatValue = 0.0f },
            ResourceEventModifierProp = new StatModifierData { ModifierId = 0, StatValue = 0.0f },
            MoneyBoostModifierProp = new StatModifierData { ModifierId = 0, StatValue = 0.0f },
            MoneyPermanentModifierProp = new StatModifierData { ModifierId = 0, StatValue = 0.0f },
            MoneyZoneModifierProp = new StatModifierData { ModifierId = 0, StatValue = 0.0f },
            MoneyVipModifierProp = new StatModifierData { ModifierId = 0, StatValue = 0.0f },
            MoneyEventModifierProp = new StatModifierData { ModifierId = 0, StatValue = 0.0f },
            ReputationBoostModifierProp = new StatModifierData { ModifierId = 0, StatValue = 0.0f },
            ReputationPermanentModifierProp = new StatModifierData { ModifierId = 0, StatValue = 0.0f },
            ReputationZoneModifierProp = new StatModifierData { ModifierId = 0, StatValue = 0.0f },
            ReputationVipModifierProp = new StatModifierData { ModifierId = 0, StatValue = 0.0f },
            ReputationEventModifierProp = new StatModifierData { ModifierId = 0, StatValue = 0.0f },
            WalletProp = new WalletData { Beans = 999, Epoch = 1462889864 },
            LoyaltyProp = new LoyaltyData { Current = 0, Lifetime = 0, Tier = 0 },
            LevelProp = HardcodedCharacterData.Level,
            EffectiveLevelProp = HardcodedCharacterData.EffectiveLevel,
            LevelResetCountProp = 0,
            OldestDeployablesProp = new OldestDeployablesField { Data = [] },
            PerkRespecsProp = 0,
            ArcStatusProp = null,
            LeaveZoneTimeProp = null,
            ChatMuteStatusProp = 0,
            TimedDailyRewardProp = new TimedDailyRewardData
                                {
                                    Stage = 0,
                                    State = 0,
                                    RollNumber = 0,
                                    MaxRolls = 0,
                                    CountdownToTime = 0
                                },
            TimedDailyRewardResultProp = null,
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
            SinCardFields_20Prop = null,
            SinCardFields_21Prop = null,
            SinCardFields_22Prop = null,
            AssetOverridesProp = AssetOverrides,
            FriendCountProp = 0, // :'(
            CAISStatusProp = new CAISStatusData { State = CAISStatusData.CAISState.None, Elapsed = 0 },
            ScalingLevelProp = 0,
            PvPRankProp = 0,
            PvPRankPointsProp = 0,
            PvPTokensProp = 0,
            BountyPointsLastClaimedProp = 0,
            EliteLevelProp = 1
        };

        Character_CombatController = new CombatController
        {
            RunSpeedMultProp = new StatMultiplierData { Value = 1.0f, Time = Shard.CurrentTime },
            FwdRunSpeedMultProp = new StatMultiplierData { Value = 1.0f, Time = Shard.CurrentTime },
            JumpHeightMultProp = new StatMultiplierData { Value = 1.0f, Time = Shard.CurrentTime },
            AirControlMultProp = new StatMultiplierData { Value = 1.0f, Time = Shard.CurrentTime },
            ThrustStrengthMultProp = new StatMultiplierData { Value = 1.0f, Time = Shard.CurrentTime },
            ThrustAirControlProp = new StatMultiplierData { Value = 1.0f, Time = Shard.CurrentTime },
            FrictionProp = new StatMultiplierData { Value = 1.0f, Time = Shard.CurrentTime },
            AmmoConsumptionProp = new StatMultiplierData { Value = 1.0f, Time = Shard.CurrentTime },
            MaxTurnRateProp = new StatMultiplierData { Value = 0f, Time = Shard.CurrentTime },
            TurnSpeedProp = new StatMultiplierData { Value = 1.0f, Time = Shard.CurrentTime },
            TimeDilationProp = new StatMultiplierData { Value = 1.0f, Time = Shard.CurrentTime },
            FireRateModifierProp = new StatMultiplierData { Value = 1.0f, Time = Shard.CurrentTime },
            AccuracyModifierProp = new StatMultiplierData { Value = 1.0f, Time = Shard.CurrentTime },
            GravityMultProp = new StatMultiplierData { Value = 1.0f, Time = Shard.CurrentTime },
            AirResistanceMultProp = new StatMultiplierData { Value = 1.0f, Time = Shard.CurrentTime },
            WeaponChargeupModProp = new StatMultiplierData { Value = 1.0f, Time = Shard.CurrentTime },
            WeaponDamageDealtModProp = new StatMultiplierData { Value = 1.0f, Time = Shard.CurrentTime },
            FireMode_0Prop = FireMode_0,
            FireMode_1Prop = FireMode_1,
            WeaponIndexProp = WeaponIndex,
            WeaponFireBaseTimeProp = new WeaponFireBaseTimeData { ChangeTime = 0, Unk = 0 },
            WeaponAgilityModProp = 1.0f,
            CombatFlagsProp = new CombatFlagsData { Value = 0, Time = Shard.CurrentTime },
            PermissionFlagsProp = PermissionFlags,
            NemesesProp = new NemesesData { Values = [] },
            SuperChargeProp = new SuperChargeData { Value = 100, Op = 0 }
        };
        Character_MissionAndMarkerController = new MissionAndMarkerController();
        Character_LocalEffectsController = new LocalEffectsController();
    }

    private void InitViews()
    {
        Character_ObserverView = new ObserverView
        {
            StaticInfoProp = StaticInfo,
            SpawnTimeProp = Shard.CurrentTime,
            EffectsFlagsProp = EffectsFlags,
            GibVisualsIDProp = GibVisualsInfo,
            ProcessDelayProp = ProcessDelay,
            CharacterStateProp = CharacterState,
            HostilityInfoProp = HostilityInfo,
            PersonalFactionStanceProp = null,
            CurrentHealthPctProp = 100,
            MaxHealthProp = MaxHealth,
            EmoteIDProp = Emote,
            AttachedToProp = null,
            SnapMountProp = 0,
            SinFlagsProp = 0,
            SinFactionsAcquiredByProp = null,
            SinTeamsAcquiredByProp = null,
            ArmyGUIDProp = 0,
            OwnerIdProp = Owner?.EntityId ?? 0,
            NPCTypeProp = 0,
            DockedParamsProp = DockedParams,
            LookAtTargetProp = null,
            WaterLevelAndDescProp = 0,
            CarryableObjects_0Prop = null,
            CarryableObjects_1Prop = null,
            CarryableObjects_2Prop = null,
            RespawnTimesProp = null,
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
            SinCardFields_20Prop = null,
            SinCardFields_21Prop = null,
            SinCardFields_22Prop = null,
            AssetOverridesProp = AssetOverrides
        };
        Character_EquipmentView = new EquipmentView
        {
            VisualOverridesProp = VisualOverrides,
            CurrentEquipmentProp = CurrentEquipment,
            LevelProp = 1,
            CurrentDurabilityPctProp = 100,
            CharacterStatsProp = CharacterStats,
            ScalingLevelProp = 1,
            PvPRankProp = 0,
            EliteLevelProp = 0
        };
        Character_CombatView = new CombatView
        {
            FireMode_0Prop = FireMode_0,
            FireMode_1Prop = FireMode_1,
            WeaponIndexProp = WeaponIndex,
            WeaponAgilityModProp = 1.0f,
            CombatFlagsProp = new CombatFlagsData { Value = 0, Time = Shard.CurrentTime },
            MimicParentProp = new EntityId { Backing = 0 },
            MimicOffsetProp = Vector3.Zero,

            ClipEmptyBeginProp = Shard.CurrentTime,
            ClipEmptyEndProp = Shard.CurrentTime,
            WeaponBurstFiredProp = Shard.CurrentTime,
            WeaponBurstEndedProp = Shard.CurrentTime,
            WeaponBurstCancelledProp = Shard.CurrentTime,
            WeaponReloadedProp = Shard.CurrentTime,
            WeaponReloadCancelledProp = Shard.CurrentTime,
            AbilityCooldownEndMs_0Prop = Shard.CurrentTime,
            AbilityCooldownEndMs_1Prop = Shard.CurrentTime,
            AbilityCooldownEndMs_2Prop = Shard.CurrentTime,
            AbilityCooldownEndMs_3Prop = Shard.CurrentTime,
            EquipmentLoadTimeProp = Shard.CurrentTime,
            Ammo_0Prop = 88,
            Ammo_1Prop = 88,
            AltAmmo_0Prop = 52,
            AltAmmo_1Prop = 52,
        };
        Character_MovementView = new MovementView
        {
            MovementProp = new AeroMessages.GSS.Character.MovementData
            {
                Position = Position,
                Rotation = Orientation,
                Aim = AimDirection,
                MovementState = (ushort)MovementState,
                Time = Shard.CurrentTime
            }
        };
    }

    private void RefreshMovementView()
    {
        Character_MovementView.MovementProp = new MovementData
        {
            Position = Position,
            Rotation = Orientation,
            Aim = AimDirection,
            MovementState = (ushort)MovementState,
            Time = Shard.CurrentTime
        };
    }

    private void RefreshAllStatusEffects()
    {
        for (int i = 0; i < 32; i++)
        {
            var sourceTime = GetType().GetProperty($"StatusEffectsChangeTime_{i}").GetValue(this);
            var sourceData = GetType().GetProperty($"StatusEffects_{i}").GetValue(this);

            Character_CombatView.GetType().GetProperty($"StatusEffectsChangeTime_{i}Prop").SetValue(Character_CombatView, sourceTime, null);
            Character_CombatView.GetType().GetProperty($"StatusEffects_{i}Prop").SetValue(Character_CombatView, sourceData, null);

            if (Character_CombatController != null)
            {
                Character_CombatController.GetType().GetProperty($"StatusEffectsChangeTime_{i}Prop").SetValue(Character_CombatController, sourceTime, null);
                Character_CombatController.GetType().GetProperty($"StatusEffects_{i}Prop").SetValue(Character_CombatController, sourceData, null);
            }
        }
    }

    private void SetMapMarker(byte index, PersonalMapMarkerData? data)
    {
        Character_MissionAndMarkerController?.GetType().GetProperty($"PersonalMapMarkers_{index}Prop")
                                            ?.SetValue(Character_MissionAndMarkerController, data);
    }

    public class ActiveStatModifier
    {
        public StatModifierIdentifier Stat { get; set; }
        public byte Op { get; set; }
        public float Value { get; set; }
    }

    public class ActiveWeaponDetails
    {
        public static readonly ActiveWeaponDetails Empty = new()
        {
            Weapon = null,
            WeaponId = 0,
            SpreadProfile = default,
            RateOfFire = 0,
            Attributes = [],
        };

        public WeaponTemplateResult Weapon;
        public uint WeaponId;
        public WeaponSpreadProfile SpreadProfile;
        public float RateOfFire;
        public Dictionary<ushort, float> Attributes = [];

        public bool IsEmpty => Weapon == null;

        public string DisplayName => Weapon?.DebugName ?? "(not a weapon)";

        public float Spread => SpreadProfile.OtherSpreadPct;
    }
}