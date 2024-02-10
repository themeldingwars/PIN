using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AeroMessages.Common;
using AeroMessages.GSS.V66;
using AeroMessages.GSS.V66.Character;
using AeroMessages.GSS.V66.Character.Command;
using AeroMessages.GSS.V66.Character.Controller;
using AeroMessages.GSS.V66.Character.View;
using GameServer.Aptitude;
using GameServer.Data.SDB;
using GrpcGameServerAPIClient;

namespace GameServer.Entities.Character;

/// <summary>
/// Base Character
/// </summary>
public class CharacterEntity : BaseAptitudeEntity, IAptitudeTarget
{
    public CharacterEntity(IShard shard, ulong eid)
        : base(shard, eid)
    {
        AeroEntityId = new EntityId() { Backing = EntityId, ControllerId = Controller.Character };
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

    public INetworkPlayer Player { get; set; }
    public bool IsPlayerControlled => Player != null;
    public Data.Character CharData { get; set; }
    public Vector3 Position { get; set; }
    public Quaternion Rotation { get; set; }
    public Vector3 Velocity { get; set; }
    public Vector3 AimDirection { get; set; }
    public short MovementState { get; set; }
    public ushort MovementShortTime { get; set; }
    public bool Alive { get; set; }
    public short TimeSinceLastJump { get; set; }
    public bool IsAirborne { get; set; }
    public Dictionary<PermissionFlagsData.CharacterPermissionFlags, bool> CurrentPermissions { get; set; } = new Dictionary<PermissionFlagsData.CharacterPermissionFlags, bool>()
    {
        { PermissionFlagsData.CharacterPermissionFlags.movement, true },
        { PermissionFlagsData.CharacterPermissionFlags.sprint, true },
        { PermissionFlagsData.CharacterPermissionFlags.jump, true },
        { PermissionFlagsData.CharacterPermissionFlags.unk_3, true },
        { PermissionFlagsData.CharacterPermissionFlags.weapon, true },
        { PermissionFlagsData.CharacterPermissionFlags.unk_5, true },
        { PermissionFlagsData.CharacterPermissionFlags.abilities, true },
        { PermissionFlagsData.CharacterPermissionFlags.unk_7, true },
        { PermissionFlagsData.CharacterPermissionFlags.unk_8, false },
        { PermissionFlagsData.CharacterPermissionFlags.cheat_jump_midair, false },
        { PermissionFlagsData.CharacterPermissionFlags.glider, false },
        { PermissionFlagsData.CharacterPermissionFlags.unk_11, false },
        { PermissionFlagsData.CharacterPermissionFlags.jetpack, true },
        { PermissionFlagsData.CharacterPermissionFlags.unk_13, true },
        { PermissionFlagsData.CharacterPermissionFlags.unk_14, true },
        { PermissionFlagsData.CharacterPermissionFlags.unk_15, true },
        { PermissionFlagsData.CharacterPermissionFlags.new_character, false },
        { PermissionFlagsData.CharacterPermissionFlags.glider_hud, false },
        { PermissionFlagsData.CharacterPermissionFlags.crouch, true },
        { PermissionFlagsData.CharacterPermissionFlags.cheat_float, false },
        { PermissionFlagsData.CharacterPermissionFlags.detect_resources, false },
        { PermissionFlagsData.CharacterPermissionFlags.unk_21, true },
        { PermissionFlagsData.CharacterPermissionFlags.calldown_abilities, true },
        { PermissionFlagsData.CharacterPermissionFlags.unk_23, true },
        { PermissionFlagsData.CharacterPermissionFlags.unk_24, true },
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
    public CharacterStateData CharacterState { get; set; }
    public HostilityInfoData HostilityInfo { get; set; }
    public MaxVital MaxShields { get; set; }
    public MaxVital MaxHealth { get; set; }
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
    public AttachedToData? AttachedTo { get; set; } = null;
    public IEntity AttachedToEntity { get; set; } = null;

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

    internal MovementStateContainer MovementStateContainer { get; set; } = new();

    public void SetStaticInfo(StaticInfoData value)
    {
        StaticInfo = value;
        Character_ObserverView.StaticInfoProp = StaticInfo;
        if (Character_BaseController != null)
        {
            Character_BaseController.StaticInfoProp = StaticInfo;
        }
    }

    public void SetCurrentEquipment(EquipmentData value)
    {
        CurrentEquipment = value;
        Character_EquipmentView.CurrentEquipmentProp = CurrentEquipment;
        if (Character_BaseController != null)
        {
            Character_BaseController.CurrentEquipmentProp = CurrentEquipment;
        }
    }

    public void LoadMonster(uint typeId)
    {
        // TODO: GetMonsterVisualOptions
        var monsterInfo = SDBInterface.GetMonster(typeId);
        var chassisWarpaint = SDBUtils.GetChassisWarpaint(monsterInfo.ChassisId, monsterInfo.FullbodyWarpaintPaletteId, monsterInfo.ArmorWarpaintPaletteId, monsterInfo.BodysuitWarpaintPaletteId, monsterInfo.GlowWarpaintPaletteId);
        var chassisBackpackId = monsterInfo.BackpackId;

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
            IsNPC = 1,
            StaffFlags = 0,
            CharacterTypeId = monsterInfo.Id,
            VoiceSet = monsterInfo.VoiceSet,
            TitleId = monsterInfo.Title,
            NameLocalizationId = monsterInfo.LocalizedNameId,
            HeadAccessories = new uint[] { monsterInfo.HeadAcc1Id, monsterInfo.HeadAcc2Id },
            LoadoutVehicle = 0,
            LoadoutGlider = 0,
            Visuals = new VisualsBlock
            {
                Decals = Array.Empty<VisualsDecalsBlock>(),
                Gradients = Array.Empty<uint>(),
                Colors = new uint[5]
                {
                    monsterInfo.SkinColor,
                    monsterInfo.LipColor,
                    monsterInfo.EyeColor,
                    monsterInfo.HairColor,
                    monsterInfo.FacialHairColor
                },
                Palettes = Array.Empty<VisualsPaletteBlock>(),
                Patterns = Array.Empty<VisualsPatternBlock>(),
                OrnamentGroupIds = ornaments.ToArray(),
                CziMapAssetIds = Array.Empty<uint>(),
                MorphWeights = Array.Empty<HalfFloat>(),
                Overlays = Array.Empty<VisualsOverlayBlock>()
            },
            ArmyTag = string.Empty
        });

        SetCurrentEquipment(new EquipmentData()
        {
            Chassis = new SlottedItem
            {
                SdbId = monsterInfo.ChassisId,
                SlotIndex = 0,
                Flags = 0,
                Unk2 = 0,
                Modules = Array.Empty<SlottedModule>(),
                Visuals = new VisualsBlock
                {
                    Decals = Array.Empty<VisualsDecalsBlock>(),
                    Gradients = chassisWarpaint.Gradients,
                    Colors = chassisWarpaint.Colors,
                    Palettes = chassisWarpaint.Palettes,
                    Patterns = Array.Empty<VisualsPatternBlock>(),
                    OrnamentGroupIds = Array.Empty<uint>(),
                    CziMapAssetIds = Array.Empty<uint>(),
                    MorphWeights = Array.Empty<HalfFloat>(),
                    Overlays = Array.Empty<VisualsOverlayBlock>()
                }
            },
            Backpack = new SlottedItem
            {
                SdbId = chassisBackpackId,
                SlotIndex = 0,
                Flags = 0,
                Unk2 = 0,
                Modules = Array.Empty<SlottedModule>(),
                Visuals = new VisualsBlock
                {
                    Decals = Array.Empty<VisualsDecalsBlock>(),
                    Gradients = Array.Empty<uint>(),
                    Colors = Array.Empty<uint>(),
                    Palettes = Array.Empty<VisualsPaletteBlock>(),
                    Patterns = Array.Empty<VisualsPatternBlock>(),
                    OrnamentGroupIds = Array.Empty<uint>(),
                    CziMapAssetIds = Array.Empty<uint>(),
                    MorphWeights = Array.Empty<HalfFloat>(),
                    Overlays = Array.Empty<VisualsOverlayBlock>()
                }
            },
            PrimaryWeapon = new SlottedWeapon
            {
                Item = new SlottedItem
                {
                    SdbId = monsterInfo.Weapon1Id,
                    SlotIndex = 0,
                    Flags = 0,
                    Unk2 = 0,
                    Modules = Array.Empty<SlottedModule>(),
                    Visuals = new VisualsBlock
                    {
                        Decals = Array.Empty<VisualsDecalsBlock>(),
                        Gradients = Array.Empty<uint>(),
                        Colors = Array.Empty<uint>(),
                        Palettes = Array.Empty<VisualsPaletteBlock>(),
                        Patterns = Array.Empty<VisualsPatternBlock>(),
                        OrnamentGroupIds = Array.Empty<uint>(),
                        CziMapAssetIds = Array.Empty<uint>(),
                        MorphWeights = Array.Empty<HalfFloat>(),
                        Overlays = Array.Empty<VisualsOverlayBlock>()
                    }
                },
                Unk1 = 0,
                Unk2 = 0
            },
            SecondaryWeapon = new SlottedWeapon
            {
                Item = new SlottedItem
                {
                    SdbId = monsterInfo.Weapon2Id,
                    SlotIndex = 0,
                    Flags = 0,
                    Unk2 = 0,
                    Modules = Array.Empty<SlottedModule>(),
                    Visuals = new VisualsBlock
                    {
                        Decals = Array.Empty<VisualsDecalsBlock>(),
                        Gradients = Array.Empty<uint>(),
                        Colors = Array.Empty<uint>(),
                        Palettes = Array.Empty<VisualsPaletteBlock>(),
                        Patterns = Array.Empty<VisualsPatternBlock>(),
                        OrnamentGroupIds = Array.Empty<uint>(),
                        CziMapAssetIds = Array.Empty<uint>(),
                        MorphWeights = Array.Empty<HalfFloat>(),
                        Overlays = Array.Empty<VisualsOverlayBlock>()
                    }
                },
                Unk1 = 0,
                Unk2 = 0
            },
            EndUnk1 = 0,
            EndUnk2 = 0
        });
        
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
        Data.Character staticData = Data.Character.Load(0);
        CharData = staticData; // Necessary for InitControllers

        // Temp: Hacky chassis colors until proper loadout structure
        var chassisWarpaint = SDBUtils.GetChassisWarpaint(remoteData.CharacterInfo.CurrentBattleframeSDBId, 0, 0, 0, 0);
        var chassisBackpackId = SDBUtils.GetChassisDefaultBackpack(remoteData.CharacterInfo.CurrentBattleframeSDBId);

        SetStaticInfo(new StaticInfoData
        {
            DisplayName = remoteData.CharacterInfo.Name,
            UniqueName = remoteData.CharacterInfo.Name,
            Gender = (byte)remoteData.CharacterInfo.Gender,
            Race = (byte)remoteData.CharacterInfo.Race,
            CharInfoId = staticData.CharInfoID, // 1 for players
            HeadMain = (uint)remoteData.CharacterVisuals.Head.Id,
            Eyes = (uint)remoteData.CharacterVisuals.Eyes.Id,
            Unk_1 = 0xff,
            IsNPC = 0,
            StaffFlags = 0x3,
            CharacterTypeId = staticData.CharVisuals.CharTypeID,
            VoiceSet = (uint)remoteData.CharacterVisuals.VoiceSet.Id,
            TitleId = (ushort)remoteData.CharacterInfo.TitleId,
            NameLocalizationId = staticData.NameLocalizationID, // Not relevant for players
            HeadAccessories = remoteData.CharacterVisuals.HeadAccessories.ToList<WebIdValueColor>().Select(item => (uint)item.Id).ToArray(),
            LoadoutVehicle = (uint)remoteData.CharacterVisuals.Vehicle.Id,
            LoadoutGlider = (uint)remoteData.CharacterVisuals.Glider.Id,
            Visuals = new VisualsBlock
            {
                Decals = Array.Empty<VisualsDecalsBlock>(),
                Gradients = Array.Empty<uint>(),
                Colors = new uint[5]
                {
                    remoteData.CharacterVisuals.SkinColor.Value.Color,
                    remoteData.CharacterVisuals.LipColor.Value.Color,
                    remoteData.CharacterVisuals.EyeColor.Value.Color,
                    remoteData.CharacterVisuals.HairColor.Value.Color,
                    remoteData.CharacterVisuals.FacialHairColor.Value.Color
                },
                Palettes = Array.Empty<VisualsPaletteBlock>(),
                Patterns = Array.Empty<VisualsPatternBlock>(),
                OrnamentGroupIds = remoteData.CharacterVisuals.Ornaments.ToList<WebId>().Select(item => (uint)item.Id).ToArray(),
                CziMapAssetIds = Array.Empty<uint>(),
                MorphWeights = Array.Empty<HalfFloat>(),
                Overlays = Array.Empty<VisualsOverlayBlock>()
            },
            ArmyTag = staticData.Army.Name
        });

        SetCurrentEquipment(new EquipmentData
        {
            Chassis = new SlottedItem
            {
                SdbId = remoteData.CharacterInfo.CurrentBattleframeSDBId, // staticData.Loadout.ChassisID,
                SlotIndex = 0,
                Flags = 0,
                Unk2 = 0,
                Modules = Array.Empty<SlottedModule>(),
                Visuals = new VisualsBlock
                {
                    Decals = Array.Empty<VisualsDecalsBlock>(),
                    Gradients = chassisWarpaint.Gradients,
                    Colors = chassisWarpaint.Colors,
                    Palettes = chassisWarpaint.Palettes,
                    Patterns = Array.Empty<VisualsPatternBlock>(),
                    OrnamentGroupIds = Array.Empty<uint>(),
                    CziMapAssetIds = Array.Empty<uint>(),
                    MorphWeights = Array.Empty<HalfFloat>(),
                    Overlays = Array.Empty<VisualsOverlayBlock>()
                }
            },
            Backpack = new SlottedItem
            {
                SdbId = chassisBackpackId,
                SlotIndex = 0,
                Flags = 0,
                Unk2 = 0,
                Modules = new SlottedModule[]
                {
                    new SlottedModule
                    {
                        SdbId = 101940, // Afterburner,
                        SlotIndex = 0,
                        Flags = 0,
                        Unk2 = 0,
                    }
                },
                Visuals = new VisualsBlock
                {
                    Decals = Array.Empty<VisualsDecalsBlock>(),
                    Gradients = Array.Empty<uint>(),
                    Colors = Array.Empty<uint>(),
                    Palettes = Array.Empty<VisualsPaletteBlock>(),
                    Patterns = Array.Empty<VisualsPatternBlock>(),
                    OrnamentGroupIds = Array.Empty<uint>(),
                    CziMapAssetIds = Array.Empty<uint>(),
                    MorphWeights = Array.Empty<HalfFloat>(),
                    Overlays = Array.Empty<VisualsOverlayBlock>()
                }
            },
            PrimaryWeapon = new SlottedWeapon
            {
                Item = new SlottedItem
                {
                    SdbId = staticData.Loadout.PrimaryWeaponID,
                    SlotIndex = 0,
                    Flags = 0,
                    Unk2 = 0,
                    Modules = Array.Empty<SlottedModule>(),
                    Visuals = new VisualsBlock
                    {
                        Decals = Array.Empty<VisualsDecalsBlock>(),
                        Gradients = Array.Empty<uint>(),
                        Colors = new uint[] { 0x322c0000, 0x543110a2, 0x65b42104 },
                        Palettes = Array.Empty<VisualsPaletteBlock>(),
                        Patterns = Array.Empty<VisualsPatternBlock>(),
                        OrnamentGroupIds = Array.Empty<uint>(),
                        CziMapAssetIds = Array.Empty<uint>(),
                        MorphWeights = Array.Empty<HalfFloat>(),
                        Overlays = Array.Empty<VisualsOverlayBlock>()
                    }
                },
                Unk1 = 0,
                Unk2 = 0
            },
            SecondaryWeapon = new SlottedWeapon
            {
                Item = new SlottedItem
                {
                    SdbId = staticData.Loadout.SecondaryWeaponID,
                    SlotIndex = 0,
                    Flags = 0,
                    Unk2 = 0,
                    Modules = Array.Empty<SlottedModule>(),
                    Visuals = new VisualsBlock
                    {
                        Decals = Array.Empty<VisualsDecalsBlock>(),
                        Gradients = Array.Empty<uint>(),
                        Colors = new uint[] { 0x322c0000, 0x543110a2, 0x65b42104 },
                        Palettes = Array.Empty<VisualsPaletteBlock>(),
                        Patterns = Array.Empty<VisualsPatternBlock>(),
                        OrnamentGroupIds = Array.Empty<uint>(),
                        CziMapAssetIds = Array.Empty<uint>(),
                        MorphWeights = Array.Empty<HalfFloat>(),
                        Overlays = Array.Empty<VisualsOverlayBlock>()
                    }
                },
                Unk1 = 0,
                Unk2 = 0
            },
            EndUnk1 = 0,
            EndUnk2 = 0
        });
    }

    public void Load(ulong characterId)
    {
        CharData = Data.Character.Load(characterId);

        var cd = CharData;
        StaticInfo = new StaticInfoData
        {
            DisplayName = cd.Name,
            UniqueName = cd.Name,
            Gender = (byte)cd.Gender,
            Race = (byte)cd.Race,
            CharInfoId = cd.CharInfoID,
            HeadMain = cd.CharVisuals.HeadMain,
            Eyes = cd.CharVisuals.Eyes,
            Unk_1 = 0xff,
            IsNPC = 0,
            StaffFlags = 0x3,
            CharacterTypeId = cd.CharVisuals.CharTypeID,
            VoiceSet = cd.VoiceSet,
            TitleId = cd.TitleID,
            NameLocalizationId = cd.NameLocalizationID,
            HeadAccessories = ((List<uint>)cd.CharVisuals.HeadAccessories).ToArray(),
            LoadoutVehicle = cd.Loadout.VehicleID,
            LoadoutGlider = cd.Loadout.GliderID,
            Visuals = new VisualsBlock
            {
                Decals = Array.Empty<VisualsDecalsBlock>(),
                Gradients = Array.Empty<uint>(),
                Colors = ((List<uint>)cd.CharVisuals.Colors).ToArray(),
                Palettes = Array.Empty<VisualsPaletteBlock>(),
                Patterns = Array.Empty<VisualsPatternBlock>(),
                OrnamentGroupIds = ((List<uint>)cd.CharVisuals.OrnamentGroups).ToArray(),
                CziMapAssetIds = Array.Empty<uint>(),
                MorphWeights = Array.Empty<HalfFloat>(),
                Overlays = Array.Empty<VisualsOverlayBlock>()
            },
            ArmyTag = cd.Army.Name
        };
        Character_ObserverView.StaticInfoProp = StaticInfo;

        CurrentEquipment = new EquipmentData
        {
            Chassis = new SlottedItem
            {
                SdbId = cd.Loadout.ChassisID,
                SlotIndex = 0,
                Flags = 0,
                Unk2 = 0,
                Modules = Array.Empty<SlottedModule>(),
                Visuals = new VisualsBlock
                {
                    Decals = new VisualsDecalsBlock[]
                    {
                        new()
                        {
                            DecalId = 10000,
                            Color = 4294967295,
                            Usage = 255,
                            Transform = new HalfVector4[]
                            {
                                new()
                                {
                                    X = new HalfFloat { Value = 10935 },
                                    Y = new HalfFloat { Value = 9478 },
                                    Z = new HalfFloat { Value = 0 },
                                    W = new HalfFloat { Value = 8106 }
                                },
                                new()
                                {
                                    X = new HalfFloat { Value = 42272 },
                                    Y = new HalfFloat { Value = 43680 },
                                    Z = new HalfFloat { Value = 9380 },
                                    W = new HalfFloat { Value = 43573 }
                                },
                                new()
                                {
                                    X = new HalfFloat { Value = 9592 },
                                    Y = new HalfFloat { Value = 12012 },
                                    Z = new HalfFloat { Value = 44736 },
                                    W = new HalfFloat { Value = 15867 }
                                }
                            }
                        }
                    },
                    Gradients = Array.Empty<uint>(),
                    Colors = ((List<uint>)cd.ChassisVisuals.Colors).ToArray(),
                    Palettes = new VisualsPaletteBlock[] { new() { PaletteId = 85163, PaletteType = 0 } },
                    Patterns = new VisualsPatternBlock[]
                    {
                        new()
                        {
                            PatternId = 10022,
                            TransformValues = new HalfVector4
                            {
                                X = new HalfFloat { Value = 0 },
                                Y = new HalfFloat { Value = 16384 },
                                Z = new HalfFloat { Value = 0 },
                                W = new HalfFloat { Value = 0 }
                            }
                        }
                    },
                    OrnamentGroupIds = Array.Empty<uint>(),
                    CziMapAssetIds = Array.Empty<uint>(),
                    MorphWeights = Array.Empty<HalfFloat>(),
                    Overlays = Array.Empty<VisualsOverlayBlock>()
                }
            },
            Backpack = new SlottedItem
            {
                SdbId = cd.Loadout.BackpackID,
                SlotIndex = 0,
                Flags = 0,
                Unk2 = 0,
                Modules = Array.Empty<SlottedModule>(),
                Visuals = new VisualsBlock
                {
                    Decals = Array.Empty<VisualsDecalsBlock>(),
                    Gradients = Array.Empty<uint>(),
                    Colors = Array.Empty<uint>(),
                    Palettes = Array.Empty<VisualsPaletteBlock>(),
                    Patterns = Array.Empty<VisualsPatternBlock>(),
                    OrnamentGroupIds = Array.Empty<uint>(),
                    CziMapAssetIds = Array.Empty<uint>(),
                    MorphWeights = Array.Empty<HalfFloat>(),
                    Overlays = Array.Empty<VisualsOverlayBlock>()
                }
            },
            PrimaryWeapon = new SlottedWeapon
            {
                Item = new SlottedItem
                {
                    SdbId = cd.Loadout.PrimaryWeaponID,
                    SlotIndex = 0,
                    Flags = 0,
                    Unk2 = 0,
                    Modules = Array.Empty<SlottedModule>(),
                    Visuals = new VisualsBlock
                    {
                        Decals = Array.Empty<VisualsDecalsBlock>(),
                        Gradients = Array.Empty<uint>(),
                        Colors = new uint[] { 0x322c0000, 0x543110a2, 0x65b42104 },
                        Palettes = Array.Empty<VisualsPaletteBlock>(),
                        Patterns = Array.Empty<VisualsPatternBlock>(),
                        OrnamentGroupIds = Array.Empty<uint>(),
                        CziMapAssetIds = Array.Empty<uint>(),
                        MorphWeights = Array.Empty<HalfFloat>(),
                        Overlays = Array.Empty<VisualsOverlayBlock>()
                    }
                },
                Unk1 = 0,
                Unk2 = 0
            },
            SecondaryWeapon = new SlottedWeapon
            {
                Item = new SlottedItem
                {
                    SdbId = cd.Loadout.SecondaryWeaponID,
                    SlotIndex = 0,
                    Flags = 0,
                    Unk2 = 0,
                    Modules = Array.Empty<SlottedModule>(),
                    Visuals = new VisualsBlock
                    {
                        Decals = Array.Empty<VisualsDecalsBlock>(),
                        Gradients = Array.Empty<uint>(),
                        Colors = new uint[] { 0x322c0000, 0x543110a2, 0x65b42104 },
                        Palettes = Array.Empty<VisualsPaletteBlock>(),
                        Patterns = Array.Empty<VisualsPatternBlock>(),
                        OrnamentGroupIds = Array.Empty<uint>(),
                        CziMapAssetIds = Array.Empty<uint>(),
                        MorphWeights = Array.Empty<HalfFloat>(),
                        Overlays = Array.Empty<VisualsOverlayBlock>()
                    }
                },
                Unk1 = 0,
                Unk2 = 0
            },
            EndUnk1 = 0,
            EndUnk2 = 0
        };
        Character_EquipmentView.CurrentEquipmentProp = CurrentEquipment;
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
        if (Character_BaseController != null)
        {
            Character_BaseController.CharacterStateProp = CharacterState;
        }
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
        if (Character_BaseController != null)
        {
            Character_BaseController.EmoteIDProp = value;
        }
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
                if (Character_CombatController != null)
                {
                    Character_CombatController.FireMode_0Prop = FireMode_0;
                }

                break;
            case 1:
                FireMode_1 = value;
                Character_CombatView.FireMode_1Prop = FireMode_1;
                if (Character_CombatController != null)
                {
                    Character_CombatController.FireMode_1Prop = FireMode_1;
                }

                break;
        }
    }

    public void SetPoseData(MovementPoseData poseData, ushort shortTime)
    {
        Position = poseData.PosRotState.Pos;
        Rotation = poseData.PosRotState.Rot;
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

    public void SetRotation(Quaternion newRotation)
    {
        Rotation = newRotation;
        RefreshMovementView();
    }

    public void SetSpawnPose()
    {
        SpawnPose = new CharacterSpawnPose
        {
            Time = Shard.CurrentTime,
            Position = Position,
            Rotation = Rotation,
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
        if (Character_BaseController != null)
        {
            Character_BaseController.SpawnTimeProp = time;
        }
    }

    public void SetWeaponIndex(WeaponIndexData value)
    {
        WeaponIndex = value;
        Character_CombatView.WeaponIndexProp = value;
        
        if (Character_CombatController != null)
        {
            Character_CombatController.WeaponIndexProp = value;
        }
    }

    public void SetPermissionFlag(PermissionFlagsData.CharacterPermissionFlags flag, bool value)
    {
        CurrentPermissions[flag] = value;
        PermissionFlags = new PermissionFlagsData
        {
            Time = Shard.CurrentTime,
            Value = (PermissionFlagsData.CharacterPermissionFlags)GetCurrentPermissionsValue(),
        };

        if (Character_CombatController != null)
        {
            Character_CombatController.PermissionFlagsProp = PermissionFlags;
        }
    }

    public void SetGliderProfileId(uint profileId)
    {
        if (Character_CombatController != null)
        {
            Character_CombatController.GliderProfileIdProp = profileId;
        }
    }

    public void SetAuthorizedTerminal(AuthorizedTerminalData value)
    {
        AuthorizedTerminal = value;

        if (Character_BaseController != null)
        {
            Character_BaseController.AuthorizedTerminalProp = AuthorizedTerminal;
        }
    }

    public override void SetStatusEffect(byte index, ushort time, StatusEffectData data)
    {
        Console.WriteLine($"Character.SetStatusEffect Index {index}, Time {time}, Id {data.Id}");

        // Member
        this.GetType().GetProperty($"StatusEffectsChangeTime_{index}").SetValue(this, time, null);
        this.GetType().GetProperty($"StatusEffects_{index}").SetValue(this, data, null);
        
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
        Console.WriteLine($"Character.ClearStatusEffect Index {index}, Time {time}, Id {debugEffectId}");

        // Member
        this.GetType().GetProperty($"StatusEffectsChangeTime_{index}").SetValue(this, time, null);
        this.GetType().GetProperty($"StatusEffects_{index}").SetValue(this, null, null);
        
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

    public void SetAttachedTo(AttachedToData newValue, IEntity entity)
    {
        AttachedToEntity = entity;
        AttachedTo = newValue;
        Character_ObserverView.AttachedToProp = AttachedTo;
        if (Character_BaseController != null)
        {
            Character_BaseController.AttachedToProp = AttachedTo;
        }
    }

    public void ClearAttachedTo()
    {
        AttachedToEntity = null;
        AttachedTo = null;
        Character_ObserverView.AttachedToProp = AttachedTo;
        Character_ObserverView.SnapMountProp = 0;
        if (Character_BaseController != null)
        {
            Character_BaseController.AttachedToProp = AttachedTo;
            Character_BaseController.SnapMountProp = 0;
        }
    }

    private void InitFields()
    {
        Position = new Vector3();
        Rotation = Quaternion.Identity;
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
        MaxShields = new MaxVital { Value = 0, Time = Shard.CurrentTime };
        MaxHealth = new MaxVital { Value = 19192, Time = Shard.CurrentTime };
        GibVisualsInfo = new GibVisuals { Id = 0, Time = Shard.CurrentTime };
        ProcessDelay = new ProcessDelayData { Unk1 = 30721, Unk2 = 236 };
        Emote = new EmoteData { Id = 0, Time = 0 };
        DockedParams = new DockedParamsData { Unk1 = new EntityId { Backing = 0 }, Unk2 = Vector3.Zero, Unk3 = 0 };
        AssetOverrides = new AssetOverridesField { Ids = Array.Empty<uint>() };
        VisualOverrides = new VisualOverridesField { Data = Array.Empty<VisualOverridesData>() };
        CurrentEquipment = new EquipmentData { };
        CharacterStats = new CharacterStatsData
        {
            ItemAttributes = new StatsData[]
            {
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
            },
            Unk1 = 0,
            WeaponA = Array.Empty<StatsData>(),
            Unk2 = 0,
            WeaponB = Array.Empty<StatsData>(),
            Unk3 = 0,
            AttributeCategories1 = Array.Empty<StatsData>(),
            AttributeCategories2 = Array.Empty<StatsData>()
        };

        EnergyParams = new EnergyParamsData { Max = 1000.0f, Delay = 500, Recharge = 156.0f, Time = Shard.CurrentTime };
        ScopeBubble = new ScopeBubbleInfoData { Unk1 = 0, Unk2 = 0 };
        SpawnPose = new CharacterSpawnPose
        {
            Time = Shard.CurrentTime,
            Position = Position,
            Rotation = Rotation,
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
        var cd = CharData;

        Character_BaseController = new BaseController
        {
            TimePlayedProp = 0,
            CurrentWeightProp = 0,
            EncumberedWeightProp = 255,
            AuthorizedTerminalProp = AuthorizedTerminal,
            PingTimeProp = 0, // Shard.CurrentTime,
            StaticInfoProp = StaticInfo,
            SpawnTimeProp = Shard.CurrentTime,
            VisualOverridesProp = VisualOverrides,
            CurrentEquipmentProp = CurrentEquipment,
            SelectedLoadoutProp = 184538131, // cd.Loadout.ChassisID,
            SelectedLoadoutIsPvPProp = 0,
            GibVisualsIdProp = GibVisualsInfo,
            SpawnPoseProp = SpawnPose,
            ProcessDelayProp = ProcessDelay,
            SpectatorModeProp = 0,
            CinematicCameraProp = null,
            CharacterStateProp = CharacterState,
            HostilityInfoProp = HostilityInfo,
            PersonalFactionStanceProp = null,
            CurrentHealthProp = 0,
            CurrentShieldsProp = 0,
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
            ArmyGUIDProp = cd.Army.GUID,
            ArmyIsOfficerProp = 0,
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
            PermanentStatusEffectsProp = new PermanentStatusEffectsData { Effects = Array.Empty<PermanentStatusEffectsInnerData>() },
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
            LevelProp = cd.Level,
            EffectiveLevelProp = cd.Level,
            LevelResetCountProp = 0,
            OldestDeployablesProp = new OldestDeployablesField { Data = Array.Empty<OldestDeployablesData>() },
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
            NemesesProp = new NemesesData { Values = Array.Empty<ulong>() },
            SuperChargeProp = new SuperChargeData { Value = 0, Op = 0 }
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
            OwnerIdProp = 0,
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
            MovementProp = new AeroMessages.GSS.V66.Character.MovementData
            {
                Position = Position,
                Rotation = Rotation,
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
            Rotation = Rotation,
            Aim = AimDirection,
            MovementState = (ushort)MovementState,
            Time = Shard.CurrentTime
        };
    }

    private void RefreshAllStatusEffects()
    {
        for (int i = 0; i < 31; i++)
        {
            var sourceTime = this.GetType().GetProperty($"StatusEffectsChangeTime_{i}").GetValue(this);
            var sourceData = this.GetType().GetProperty($"StatusEffects_{i}").GetValue(this);

            Character_CombatView.GetType().GetProperty($"StatusEffectsChangeTime_{i}Prop").SetValue(Character_CombatView, sourceTime, null);
            Character_CombatView.GetType().GetProperty($"StatusEffects_{i}Prop").SetValue(Character_CombatView, sourceData, null);

            if (Character_CombatController != null)
            {
                Character_CombatController.GetType().GetProperty($"StatusEffectsChangeTime_{i}Prop").SetValue(Character_CombatController, sourceTime, null);
                Character_CombatController.GetType().GetProperty($"StatusEffects_{i}Prop").SetValue(Character_CombatController, sourceData, null);
            }
        }
    }

    private ulong GetCurrentPermissionsValue()
    {
        ulong result = 0ul;
        foreach(var pair in CurrentPermissions)
        {
            if (pair.Value)
            {
                result += (ulong)pair.Key;
            }
        }

        return result;
    }
}