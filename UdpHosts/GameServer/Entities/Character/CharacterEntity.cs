using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Aero.Gen;
using AeroMessages.Common;
using AeroMessages.GSS;
using AeroMessages.GSS.Character;
using AeroMessages.GSS.Character.Controller;
using AeroMessages.GSS.Character.View;
using BepuUtilities;
using GameServer.Data;
using GameServer.Entities.Deployable;
using GameServer.Entities.Turret;
using GameServer.Enums;
using GameServer.StaticDB;
using GameServer.StaticDB.Records.customdata;
using GameServer.StaticDB.Records.dbcharacter;
using GameServer.StaticDB.Records.dbitems;
using GameServer.StaticDB.Records.dbvisualrecords;
using GameServer.Systems.Ai;
using GameServer.Systems.Ai.Behavior;
using GameServer.Systems.Aptitude;
using GameServer.Systems.Combat;
using GameServer.Systems.Encounters;
using GameServer.Systems.MovementRelay;
using GameServer.Systems.WeaponSim;
using GameServer.Test;
using GrpcGameServerAPIClient;
using Serilog;
using Serilog.Events;

// The GameServerAPI proto has its own CharacterLoadout message, so the bare name has to pick a side.
using CharacterLoadout = GameServer.Data.CharacterLoadout;
using GibVisuals = AeroMessages.GSS.Character.GibVisuals;
using LoadoutVisualType = AeroMessages.GSS.Character.LoadoutConfig_Visual.LoadoutVisualType;

namespace GameServer.Entities.Character;

/// <summary>
/// Base Character
/// </summary>
public sealed partial class CharacterEntity : BaseAptitudeEntity, IAptitudeTarget
{
    public const byte MaxMapMarkerCount = 64;
    public const byte MaxTinyCount = 32;
    public const byte DefaultLevel = 1;
    public const uint DefaultEliteLevel = 0;
    private const int _maxMovementSamples = 32; // ~1.6s at 20 samples/s
    private const int _maxAimSamples = 8; // aim samples only arrive while firing, so a short history is enough
    private const int _maxExtrapolationMs = 100; // How far to predict beyond the newest sample
    private const float _fallbackRunSpeed = 4.5f; // TODO: Derive from SDB/character stats
    private const float _fallbackSprintSpeed = 7.0f; // TODO: Derive from SDB/character stats
    private const float _fallbackCrouchSpeed = 2.5f; // TODO: Derive from SDB/character stats
    private const int _maxAbilityProjectilePending = 8; // safety cap for queued ability projectiles
    private const uint _abilityProjectileStaleMs = 1000; // a queued ability projectile older than this is dropped
    private readonly MapMarkerState[] _mapMarkers = new MapMarkerState[MaxMapMarkerCount];
    private readonly MovementSample[] _movementSamples = new MovementSample[_maxMovementSamples];
    private readonly AimSample[] _aimSamples = new AimSample[_maxAimSamples];
    private readonly Dictionary<AptitudeStat, ActiveStatModifier> _statOverrides = new();
    private readonly Queue<PendingAbilityProjectile> _pendingAbilityProjectiles = new();
    private readonly HashSet<uint> _activatedDefaultAbilities = new();
    private readonly Dictionary<uint, AbilityInput> _abilityInputs = [];
    private int _movementSampleCount;
    private int _movementSampleNewest;
    private int _aimSampleCount;
    private int _aimSampleNewest;
    private ActiveWeaponDetails[,] _weaponDetailsCache;
    private uint _monsterTypeId;

    public CharacterEntity(IShard shard, ulong eid, CharacterEntity owner = null)
        : base(shard, eid, owner)
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

    public bool IsTinyObjectActive => Character_TinyObjectView != null;

    public new CharacterCollisionComponent Collision { get; set; }
    public INetworkPlayer Player { get; set; }
    public bool IsPlayerControlled => Player != null;
    public Vector3 Velocity { get; set; }
    public Vector3 AimDirection { get; set; }
    public Vector2 Heading { get; set; }
    public bool IsNpc { get; private set; }

    /// <summary>
    ///     Server-only character: never scoped to any client, no physics body, no AI movement. Net captures
    ///     show something like this for NPC-piloted turrets through <c>Turret::ObserverView.GunnerId</c>.
    /// </summary>
    public bool IsHidden { get; private set; }

    /// <summary>
    ///     Set by TurretControlCommand (status effect 1368). The NPC stops acting on its own and instead drives the
    ///     turret's aim and weapon through the TurretSystem.
    /// </summary>
    public TurretEntity TurretControlledBy { get; set; }
#nullable enable
    public NpcMovementComponent? NpcMovement { get; set; }
#nullable disable
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
    public float GetMass => NpcSpeedCalculator.BodyMass(StaticInfo.CharacterTypeId, CurrentLoadout?.ChassisID ?? 0u);

    public StaticInfoData StaticInfo { get; set; }
    public ulong ArmyGUID { get; set; }
    public sbyte ArmyIsOfficer { get; set; }
    public CharacterStateData CharacterState { get; set; }
    public int TimePlayed { get; set; }
    public MaxVital MaxShields { get; private set; }
    public MaxVital MaxHealth { get; private set; }
    public int CurrentHealth { get; private set; }
    public int CurrentShields { get; private set; }

    // The most recent damage instance taken, used to pick a matching dbcharacter::HitFeedbackEntry result on death.
    public DamageInfo LastDamage { get; private set; }

    public byte Level { get; set; }
    public byte EffectiveLevel { get; set; }
    public uint EliteLevel { get; set; }

    // True when an ability has queued a projectile that has not been fired or expired yet.
    public bool HasPendingAbilityProjectiles => _pendingAbilityProjectiles.Count > 0;

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
    public IEntity InteractionTarget { get; set; }
    public Guid? InteractionExecutionId { get; set; }

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

    public Dictionary<AptitudeStat, float> BaseStatModifiers { get; set; } = new()
    {
        { AptitudeStat.Speed,         1.0f },
        { AptitudeStat.FireRateModifier,     1.0f },
        { AptitudeStat.ForwardSpeed,      1.0f },
        { AptitudeStat.JumpHeight,       1.0f },
        { AptitudeStat.AirControl,       1.0f },
        { AptitudeStat.ThrustStrength,   1.0f },
        { AptitudeStat.ThrustAirControl,     1.0f },
        { AptitudeStat.Friction,             1.0f },
        { AptitudeStat.AmmoConsumption,      1.0f },
        { AptitudeStat.MaxTurnRate,          0.0f },
        { AptitudeStat.TurnSpeed,            1.0f },
        { AptitudeStat.TimeDilation,         1.0f },
        { AptitudeStat.AccuracyModifier,     1.0f },
        { AptitudeStat.Gravity,          1.0f },
        { AptitudeStat.AirResistance,    1.0f },
        { AptitudeStat.WeaponChargeupMod,    1.0f },
        { AptitudeStat.WeaponDamageDealtMod, 1.0f },
    };

    internal MovementStateContainer MovementStateContainer { get; set; } = new();

    public override string ToString()
    {
        return IsPlayerControlled ? StaticInfo.DisplayName : $"Character {_monsterTypeId} ({EntityId:X8})";
    }

    public void LoadMonster(uint typeId, byte level = DefaultLevel)
    {
        var monsterInfo = SDBInterface.GetMonster(typeId);
        SelectedMonsterVisuals monsterVisuals = LoadMonsterVisuals(typeId);
        IsNpc = true;
        ApplyProgressionLevels(level, DefaultEliteLevel);
        _monsterTypeId = typeId;

        // TODO: Consider internalizing into the CharacterLoadout instead?
        var loadout = new CharacterLoadout
        {
            ChassisID = monsterVisuals.ChassisId,
            BackpackID = monsterVisuals.BackpackId,
            ChassisWarpaint = monsterVisuals.ChassiWarpaint,
            ChassisWarpaintPatterns = monsterVisuals.ChassisWarpaintPatterns,
            ChassisWarpaintTatoos = monsterVisuals.ChassisWarpaintTatoos,
        };
        loadout.SlottedItems[LoadoutSlotType.Primary] = monsterInfo.Weapon1Id;
        loadout.SlottedItems[LoadoutSlotType.Secondary] = monsterInfo.Weapon2Id;

        if (monsterVisuals.VisualsGroupId != 0)
        {
            SetVisualOverrides(new()
            {
                Data = [
                    new()
                    {
                        SlotType = 131, // Battleframe Visuals Override
                        VisualsGroupId = monsterVisuals.VisualsGroupId
                    }
                ]
            });
        }

        var ornaments = new List<uint>();
        if (monsterVisuals.OrnamentsMapGroupId1 != 0)
        {
            ornaments.Add(monsterVisuals.OrnamentsMapGroupId1);
        }

        if (monsterVisuals.OrnamentsMapGroupId2 != 0)
        {
            ornaments.Add(monsterVisuals.OrnamentsMapGroupId2);
        }

        if (monsterVisuals.OrnamentsMapGroupId3 != 0)
        {
            ornaments.Add(monsterVisuals.OrnamentsMapGroupId3);
        }

        if (monsterVisuals.OrnamentsMapGroupId4 != 0)
        {
            ornaments.Add(monsterVisuals.OrnamentsMapGroupId4);
        }

        var staticInfo = new StaticInfoData()
        {
            DisplayName = "_noname",
            UniqueName = string.Empty,
            Gender = (byte)(monsterVisuals.Sex == 'F' ? 1 : 0),
            Race = monsterInfo.Race,
            CharInfoId = monsterInfo.CharinfoId,
            HeadMain = monsterVisuals.HeadId,
            Eyes = monsterInfo.EyesId, // !
            Unk_1 = 0xff,
            TargetFlags = TargetFlags.IsNPC,
            StaffFlags = 0,
            CharacterTypeId = monsterInfo.Id,
            VoiceSet = monsterVisuals.VoiceSet,
            TitleId = monsterInfo.Title, // !
            NameLocalizationId = monsterVisuals.NameLocalizationId,
            HeadAccessories = [monsterVisuals.HeadAcc1Id, monsterVisuals.HeadAcc2Id],
            LoadoutVehicle = 0,
            LoadoutGlider = 0,
            Visuals = new VisualsBlock
            {
                Decals = [],
                Gradients = [],
                Colors =
                [
                    monsterVisuals.SkinColor,
                    monsterVisuals.EyeColor,
                    monsterVisuals.LipColor,
                    monsterVisuals.HairColor,
                    monsterVisuals.FacialHairColor
                ],
                Palettes = [],
                Patterns = [],
                OrnamentGroupIds = [.. ornaments],
                CziMapAssetIds = [],
                MorphWeights = [],
                Overlays = []
            },
            ArmyTag = string.Empty
        };

        SetStaticInfo(staticInfo);

        SetHostilityInfo(new HostilityInfoData
        {
            Flags = 0 | HostilityInfoData.HostilityFlags.Faction,
            FactionId = (byte)monsterInfo.FactionId
        });

        ApplyLoadout(loadout);

        if (monsterInfo.DamageResponseId != 0)
        {
            DamageResponse = monsterInfo.DamageResponseId;
        }

        // Temp hack to equip weapon
        if (monsterInfo.Weapon1Id != 0)
        {
            SetWeaponIndex(new WeaponIndexData()
            {
                Index = 1, PreviousIndex = 1, PlaySwapAnimation = 0, Time = Shard.CurrentTime
            });
        }
        else if (monsterInfo.Weapon2Id != 0)
        {
            SetWeaponIndex(new WeaponIndexData()
            {
                Index = 2, PreviousIndex = 1, PlaySwapAnimation = 0, Time = Shard.CurrentTime
            });
        }

        // Set up the AI movement component (navmesh path steering, parsed behavior config).
        if (!IsHidden)
        {
            var behaviorConfig = new NpcBehaviorConfig(
                monsterInfo.Behavior,
                monsterInfo.BehaviorInstanceId,
                monsterInfo.BehaviorOffensive,
                monsterInfo.BehaviorOffensiveInstanceId,
                monsterInfo.BehaviorDefensive,
                monsterInfo.BehaviorDefensiveInstanceId);
            var speeds = NpcSpeedCalculator.Calculate(this);
            var strategy = new NavMeshPathStrategy(Shard.NavMesh);
            NpcMovement = new NpcMovementComponent(this, strategy, behaviorConfig, speeds);
        }
    }

    /// <summary>
    ///     Marks the character as server-only. Must be set before the entity is added to the shard.
    /// </summary>
    /// <param name="hidden">True to make the character server-only (never scoped or updated to clients).</param>
    public void SetHidden(bool hidden)
    {
        IsHidden = hidden;
    }

    public void ActivateDefaultAbilities(bool includeMonster = true, bool includeBattleframe = true, bool includeBackpack = true, bool includeCoreAbilityGroup = true)
    {
        if (!IsAlive)
        {
            return;
        }

        var sources = new List<(string Source, uint AbilityId)>();

        if (includeMonster && _monsterTypeId != 0)
        {
            sources.AddRange(SDBInterface.GetDefaultMonsterAbilities(_monsterTypeId)
                .Select(abilityId => ($"monster type {_monsterTypeId}", abilityId)));
        }

        uint chassisId = CurrentLoadout?.ChassisID ?? 0u;

        if (includeBattleframe)
        {
            sources.AddRange(SDBInterface.GetDefaultBattleframeAbilities(chassisId)
                .Select(abilityId => ($"chassis {chassisId}", abilityId)));
        }

        uint backpackId = CurrentLoadout?.BackpackID ?? 0u;

        if (includeBackpack)
        {
            sources.AddRange(SDBInterface.GetDefaultBackpackAbilities(backpackId)
                .Select(abilityId => ($"backpack {backpackId}", abilityId)));
        }

        uint coreAbilityGroupId = includeCoreAbilityGroup ? SDBInterface.GetBattleframe(chassisId)?.CoreAbilityGroupId ?? 0u : 0u;

        if (coreAbilityGroupId != 0)
        {
            sources.AddRange(SDBInterface.GetDefaultCoreAbilityGroupAbilities(coreAbilityGroupId)
                .Select(abilityId => ($"core ability group {coreAbilityGroupId}", abilityId)));
        }

        var skipped = new List<uint>();
        var activated = 0;

        foreach ((string source, uint abilityId) in sources)
        {
            if (abilityId == 0)
            {
                continue;
            }

            if (!_activatedDefaultAbilities.Add(abilityId))
            {
                skipped.Add(abilityId);
                continue;
            }

            activated++;

            if ((SDBInterface.GetAbilityData(abilityId)?.Chain ?? 0u) == 0u)
            {
                continue;
            }

            Logger.Debug("{Entity} activating {Source} default ability {AbilityId}", this, source, abilityId);

            Shard.Abilities.HandleActivateAbility(Shard, this, abilityId, Shard.CurrentTime, new AptitudeTargets(this));
        }

        Logger.Debug(
            "{Entity} activated {Activated} default abilities for chassis {ChassisId} (core ability group {CoreAbilityGroupId}), skipped {SkippedCount} already activated on an earlier frame: {Skipped}",
            this,
            activated,
            chassisId,
            coreAbilityGroupId,
            skipped.Count,
            string.Join(", ", skipped));
    }

    public void ResetCombatState(string reason)
    {
        var carriedEffects = ActiveEffects.Count(effect => effect != null);

        Shard.Abilities.RemoveAllActiveEffects(this);
        _abilityInputs.Clear();
        Shard.WeaponSim.ResetAmmoState(this);

        _activatedDefaultAbilities.Clear();
        ActivateDefaultAbilities(includeMonster: false, includeBattleframe: true);

        Logger.Debug(
            "{Entity} reset combat state ({Reason}): {EffectCount} active effects cleared, ammo refilled, default abilities re-activated",
            this,
            reason,
            carriedEffects);
        LogSpeedSnapshot($"combat state reset ({reason})");
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

        var previousGender = StaticInfo.Gender;
        var previousRace = StaticInfo.Race;

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
                    visuals.EyeColor,
                    visuals.LipColor,
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

        if (CurrentLoadout != null && (previousGender != StaticInfo.Gender || previousRace != StaticInfo.Race))
        {
            Log.Debug(
                "Character {EntityId} changed gender or race, re-applying loadout {LoadoutId} to rebuild the frame model",
                EntityId,
                CurrentLoadout.LoadoutID);

            ApplyLoadout(CurrentLoadout);
        }
    }

    public void ApplyProgressionLevels(uint level, uint eliteLevel)
    {
        var newLevel = (byte)Math.Min(level, byte.MaxValue);

        if (Level == newLevel && EffectiveLevel == newLevel && EliteLevel == eliteLevel)
        {
            return;
        }

        Level = newLevel;
        EffectiveLevel = newLevel;
        EliteLevel = eliteLevel;

        UpdateLevelProps();
    }

    public void ApplyProgressionXp(ulong currentXp)
    {
        var xp = (uint)currentXp;

        if (Character_BaseController != null && Character_BaseController.ProgressionXpProp != xp)
        {
            Character_BaseController.ProgressionXpProp = xp;
        }
    }

    public void ApplyLoadout(CharacterLoadout loadout)
    {
        uint previousChassisId = CurrentLoadout?.ChassisID ?? 0u;

        Shard.Admin.ApplyEquipmentOverrides(Player, loadout);
        CurrentLoadout = loadout;
        RebuildWeaponDetailsCache(loadout);
        RecalculateHealthAndWeaponDamage();

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

        if (Character_BaseController != null)
        {
            Character_BaseController.SelectedLoadoutProp = SelectedLoadout;
        }

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
            VisualRecord battleframeVisualRecord;

            try
            {
                charInfo = SDBInterface.GetCharInfo(charInfoId);
                battleframeRecord = SDBInterface.GetBattleframe(chassis.SdbId);
                poseTypeRecord = SDBInterface.GetPoseType(battleframeRecord.PosetypeId);
                battleframeVisualGroupRecords = SDBInterface.GetBattleframeVisuals(battleframeRecord.VisualGroup);

                // Find the most appropriate visual record
                for (int retry = 3; retry >= 0 && battleframeVisualGroupRecord == null; retry--)
                {
                    foreach (var record in battleframeVisualGroupRecords)
                    {
                        bool matchesRace = record.Race == race;
                        bool matchesAnyRace = record.Race == 255;
                        bool matchesGender =
                            (record.Gender == 'F' && gender == 1) ||
                            (record.Gender == 'M' && gender == 0);
                        bool matchesAnyGender = record.Gender == 'X';

                        bool valid = retry switch
                        {
                            3 => matchesRace && matchesGender,
                            2 => matchesAnyRace && matchesAnyGender,
                            1 => matchesRace || matchesGender,
                            0 => true,
                            _ => false
                        };

                        if (valid)
                        {
                            if (retry < 2)
                            {
                                Log.Warning(
                                    "Picking uncertain Battleframe VisualRecord {recordId} of group {visualGroup} for chassi {chassiId}.",
                                    record.VisualrecId,
                                    battleframeRecord.VisualGroup,
                                    chassis.SdbId);
                            }

                            Log.Debug(
                                "Selected Battleframe VisualRecord {recordId} of group {visualGroup} for chassi {chassiId} (Had Gender {genderChar}, Race {raceId} ({raceStr}))",
                                record.VisualrecId,
                                battleframeRecord.VisualGroup,
                                chassis.SdbId,
                                gender == 1 ? "F" : "M",
                                race,
                                (CharacterRace)race);

                            battleframeVisualGroupRecord = record;
                            break;
                        }
                    }
                }

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

            DamageResponse = battleframeRecord.DamageResponse;
        }

        if (IsNpc && previousChassisId != 0 && previousChassisId != loadout.ChassisID)
        {
            ActivateDefaultAbilities(includeMonster: false, includeBattleframe: true);
        }

        if (!IsNpc && previousChassisId != 0 && previousChassisId != loadout.ChassisID)
        {
            Logger.Debug(
                "{Entity} changed chassis {PreviousChassisId} -> {ChassisId}: {EffectCount} active effects carried over and no stat modifier refreshed",
                this,
                previousChassisId,
                loadout.ChassisID,
                ActiveEffects.Count(effect => effect != null));
        }

        LogSpeedSnapshot($"applied loadout {loadout.LoadoutID}, chassis {previousChassisId} -> {loadout.ChassisID}");
    }

    public float GetItemAttribute(ushort id) => CurrentLoadout.ItemAttributes.GetValueOrDefault(id);

    public void AddStatModifier(uint reference, ActiveStatModifier mod)
    {
        _statOverrides[mod.Stat] = mod;
        RefreshStatModifier(mod.Stat);
    }

    public void RemoveStatModifier(uint reference, AptitudeStat stat)
    {
        if (_statOverrides.Remove(stat))
        {
            RefreshStatModifier(stat);
        }
    }

    public void RefreshStatModifier(AptitudeStat stat)
    {
        if (Character_CombatController != null)
        {
            StatMultiplierData value = new()
            {
                Value = GetCurrentStatModifierValue(stat),
                Time = Shard.CurrentTime,
            };

            Logger.Debug(
                "{Entity} StatModifier {Stat} set to {Value} ({Breakdown})",
                this,
                stat,
                value.Value,
                DescribeStatModifier(stat));

            switch (stat)
            {
                case AptitudeStat.Speed:
                    Character_CombatController.RunSpeedMultProp = value;
                    break;
                case AptitudeStat.FireRateModifier:
                    Character_CombatController.FireRateModifierProp = value;
                    break;
                case AptitudeStat.ForwardSpeed:
                    Character_CombatController.FwdRunSpeedMultProp = value;
                    break;
                case AptitudeStat.JumpHeight:
                    Character_CombatController.JumpHeightMultProp = value;
                    break;
                case AptitudeStat.AirControl:
                    Character_CombatController.AirControlMultProp = value;
                    break;
                case AptitudeStat.ThrustStrength:
                    Character_CombatController.ThrustStrengthMultProp = value;
                    break;
                case AptitudeStat.ThrustAirControl:
                    Character_CombatController.ThrustAirControlProp = value;
                    break;
                case AptitudeStat.Friction:
                    Character_CombatController.FrictionProp = value;
                    break;
                case AptitudeStat.AmmoConsumption:
                    Character_CombatController.AmmoConsumptionProp = value;
                    break;
                case AptitudeStat.MaxTurnRate:
                    Character_CombatController.MaxTurnRateProp = value;
                    break;
                case AptitudeStat.TurnSpeed:
                    Character_CombatController.TurnSpeedProp = value;
                    break;
                case AptitudeStat.TimeDilation:
                    Character_CombatController.TimeDilationProp = value;
                    break;
                case AptitudeStat.AccuracyModifier:
                    Character_CombatController.AccuracyModifierProp = value;
                    break;
                case AptitudeStat.Gravity:
                    Character_CombatController.GravityMultProp = value;
                    break;
                case AptitudeStat.AirResistance:
                    Character_CombatController.AirResistanceMultProp = value;
                    break;
                case AptitudeStat.WeaponChargeupMod:
                    Character_CombatController.WeaponChargeupModProp = value;
                    break;
                case AptitudeStat.WeaponDamageDealtMod:
                    Character_CombatController.WeaponDamageDealtModProp = value;
                    break;
            }
        }
    }

    public float GetCurrentStatModifierValue(AptitudeStat stat)
    {
        float value;
        if (!BaseStatModifiers.TryGetValue(stat, out value))
        {
            Logger.Warning("MISSING BaseStatModifier for {Stat}", stat);
            value = 1.0f;
        }

        // From status effect contexts
        foreach (var effect in GetActiveEffects())
        {
            if (effect is { Context: { } ctx } && ctx.StatChangelist.TryGetValue(stat, out var mod))
            {
                value = FoldStatModifier(value, mod);
            }
        }

        // From custom overrides
        if (_statOverrides.TryGetValue(stat, out var overrideMod))
        {
            value = FoldStatModifier(value, overrideMod);
        }

        return value;
    }

    public string DescribeStatModifier(AptitudeStat stat)
    {
        var contributors = new List<string> { $"{BaseStatModifiers.GetValueOrDefault(stat, 1.0f):0.###}" };

        foreach (var effect in GetActiveEffects())
        {
            if (effect?.Context == null || !effect.Context.StatChangelist.TryGetValue(stat, out var mod))
            {
                continue;
            }

            contributors.Add(
                $"effect {effect.Effect.Id} in slot {effect.Index} ({effect.Stacks}/{effect.Effect.MaxStackCount} stacks) multi {mod.Multi:0.###} add {mod.Add:0.###}");
        }

        if (_statOverrides.TryGetValue(stat, out var overrideMod))
        {
            contributors.Add($"override multi {overrideMod.Multi:0.###} add {overrideMod.Add:0.###}");
        }

        return $"{string.Join(" + ", contributors)} = {GetCurrentStatModifierValue(stat):0.###}";
    }

    public void LogSpeedSnapshot(string reason)
    {
        if (!Logger.IsEnabled(LogEventLevel.Debug))
        {
            return;
        }

        var effects = new List<string>();

        foreach (var effect in GetActiveEffects())
        {
            if (effect == null)
            {
                continue;
            }

            effects.Add(
                $"{effect.Effect.Id} in {effect.Index} x{effect.Stacks}{(effect.Effect.DurationChain == null ? " (no duration)" : string.Empty)}");
        }

        var controller = Character_CombatController;

        Logger.Debug(
            "Speed snapshot {Reason} of {Entity}: chassis {ChassisId} level {Level}; item attributes run {RunSpeed}, sprint {SprintSpeed}, jump {JumpHeight}; " +
            "controller run {RunSpeedMult}, forward run {FwdRunSpeedMult}, jump {JumpHeightMult}; speed {SpeedModifier}; {EffectCount} active effects: {Effects}",
            reason,
            this,
            CurrentLoadout?.ChassisID ?? 0u,
            Level,
            CurrentLoadout?.ItemAttributes.GetValueOrDefault((ushort)ItemAttributeId.RunSpeed) ?? 0f,
            CurrentLoadout?.ItemAttributes.GetValueOrDefault((ushort)ItemAttributeId.SprintSpeed) ?? 0f,
            CurrentLoadout?.ItemAttributes.GetValueOrDefault((ushort)ItemAttributeId.JumpHeight) ?? 0f,
            controller?.RunSpeedMultProp.Value ?? 0f,
            controller?.FwdRunSpeedMultProp.Value ?? 0f,
            controller?.JumpHeightMultProp.Value ?? 0f,
            DescribeStatModifier(AptitudeStat.Speed),
            effects.Count,
            string.Join(", ", effects));
    }

    public void SetCharacterStats(CharacterStatsData value)
    {
        CharacterStats = value;
        Character_EquipmentView.CharacterStatsProp = CharacterStats;

        if (Character_BaseController != null)
        {
            Character_BaseController.CharacterStatsProp = CharacterStats;
        }
    }

    public void SetStaticInfo(StaticInfoData value)
    {
        StaticInfo = value;
        Character_ObserverView.StaticInfoProp = StaticInfo;

        if (Character_BaseController != null)
        {
            Character_BaseController.StaticInfoProp = StaticInfo;
        }
    }

    public void SetTimePlayed(int value)
    {
        TimePlayed = value;

        if (Character_BaseController != null)
        {
            Character_BaseController.TimePlayedProp = TimePlayed;
        }
    }

    public void SetArmyGUID(ulong value)
    {
        ArmyGUID = value;
        Character_ObserverView.ArmyGUIDProp = ArmyGUID;

        if (Character_BaseController != null)
        {
            Character_BaseController.ArmyGUIDProp = ArmyGUID;
        }
    }

    public void SetArmyIsOfficer(sbyte value)
    {
        ArmyIsOfficer = value;

        if (Character_BaseController != null)
        {
            Character_BaseController.ArmyIsOfficerProp = ArmyIsOfficer;
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
        Character_ObserverView.EmoteIDProp = Emote;

        if (Character_BaseController != null)
        {
            Character_BaseController.EmoteIDProp = Emote;
        }
    }

    public void SetFireBurst(uint time)
    {
        Logger.Debug("SetFireBurst Time {Time} {Entity} ({Diff})",  time, this, Shard.CurrentTime - time);
        Character_CombatView.WeaponBurstFiredProp = time;
    }

    public void SetFireCancel(uint time)
    {
        Logger.Debug("SetFireCancel Time {Time} {Entity} ({Diff})",  time, this, Shard.CurrentTime - time);
        Character_CombatView.WeaponBurstCancelledProp = time;
    }

    public void SetFireEnd(uint time)
    {
        Logger.Debug("SetFireEnd Time {Time} {Entity} ({Diff})",  time, this, Shard.CurrentTime - time);
        Character_CombatView.WeaponBurstEndedProp = time;
    }

    /// <summary>
    ///     Sets the NPC's current combat target on the CombatView (NPCTargetObjIdProp) so clients know who the monster
    ///     is attacking (the original server writes this for NPCs in combat). Pass null to clear when it leaves combat.
    /// </summary>
    /// <param name="target">The targeted character, or null for none</param>
    public void SetNpcTarget(CharacterEntity target)
    {
        Character_CombatView.NPCTargetObjIdProp = target?.AeroEntityId;
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
        Orientation = poseData.PosRotState.Rot;
        MovementState = poseData.PosRotState.MovementState;
        Velocity = poseData.Velocity;

        if ((poseData.MovementType & MovementDataType.Aim) != 0)
        {
            AimDirection = poseData.Aim;
        }

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
            MovementType = 0,
            WaterLevelAndDesc = 0,
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

    public void SetNpcType(ushort npcType)
    {
        Character_ObserverView.NPCTypeProp = npcType;
    }

    public void SetWeaponIndex(WeaponIndexData value)
    {
        WeaponIndex = value;
        Character_CombatView.WeaponIndexProp = WeaponIndex;

        if (Character_CombatController != null)
        {
            Character_CombatController.WeaponIndexProp = WeaponIndex;
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

    public void SetHoverProfileId(uint profileId)
    {
        if (Character_CombatController != null)
        {
            Character_CombatController.HoverProfileIdProp = profileId;
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

    public void SetVisualOverrides(VisualOverridesField data)
    {
        VisualOverrides = data;

        if (Character_BaseController != null)
        {
            Character_BaseController.VisualOverridesProp = VisualOverrides;
        }

        Character_EquipmentView.VisualOverridesProp = VisualOverrides;
    }

    public override void SetStatusEffect(byte index, ushort time, StatusEffectData data)
    {
        Logger.Debug("{Entity} SetStatusEffect Index {Index}, Time {Time}, Id {Id}", this, index, time, data.Id);

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
        Logger.Debug("{Entity} ClearStatusEffect Index {Index}, Time {Time}, Id {DebugEffectId}", this, index, time, debugEffectId);

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

        if (Character_BaseController != null)
        {
            Character_BaseController.AttachedToProp = AttachedTo;
        }
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
            Logger.Debug("{Entity} ClearStatusEffect Index {Index}, Time {Time}", this, index, time);

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

        if (CharacterLoadout.LoadoutWeaponSlots.Contains(slot))
        {
            Shard.WeaponSim.ResetAmmoState(this);
        }
    }

    public void EquipVisualBySdbId(int loadoutId, LoadoutVisualType visualSlot, LoadoutSlotType slot, uint sdb_id)
    {
        Player.Inventory.EquipVisualBySdbId(loadoutId, visualSlot, slot, sdb_id);
        Player.CharacterEntity.CurrentLoadout.GliderID = sdb_id;
        ApplyLoadout(CurrentLoadout);
    }

    /// <summary>
    ///     Update loadout visuals
    /// </summary>
    /// <param name="loadoutId">The loadout to change</param>
    /// <param name="configId">The loadout config, 0 for PvE and 1 for PvP</param>
    /// <param name="visuals">The visuals to upsert</param>
    /// <param name="applied">The visuals the loadout config holds afterwards</param>
    /// <returns>True when the inventory changed</returns>
    public bool SetLoadoutVisuals(int loadoutId, uint configId, IReadOnlyList<LoadoutConfig_Visual> visuals, out LoadoutConfig_Visual[] applied)
    {
        var changed = Player.Inventory.SetLoadoutVisuals(loadoutId, configId, visuals, out applied);
        if (changed && loadoutId == SelectedLoadout && configId == CharacterInventory.PveConfigId)
        {
            ApplyLoadoutVisuals(applied);
            ApplyLoadout(CurrentLoadout);
        }

        return changed;
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
            Logger.Warning("{Entity} GetActiveWeaponDetails failed because invalid selected weapon index {Index}", this, index);
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

    public ActiveWeaponDetails? GetWeaponDetails(byte slotIndex, byte modeIndex)
    {
        if (_weaponDetailsCache == null)
        {
            return null;
        }

        if (slotIndex > 2 || modeIndex > 1)
        {
            return null;
        }

        return _weaponDetailsCache[slotIndex, modeIndex];
    }
    #nullable disable

    public Vector3 GetProjectileOrigin()
    {
        return GetProjectileOrigin(AimDirection);
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
    ///     Queues an ability projectile to be fired.
    /// </summary>
    /// <param name="projectile">Pending data</param>
    public void EnqueueAbilityProjectile(PendingAbilityProjectile projectile)
    {
        if (_pendingAbilityProjectiles.Count >= _maxAbilityProjectilePending)
        {
            _pendingAbilityProjectiles.Dequeue();
        }

        _pendingAbilityProjectiles.Enqueue(projectile);
    }

    /// <summary>
    ///     Consumes the oldest pending ability projectile that is still valid for the given fire time,
    ///     returning null when none is pending.
    /// </summary>
    /// <param name="fireTime">Gametime</param>
    /// <returns>PendingAbilityProjectile or null if none can be consumed</returns>
    public PendingAbilityProjectile? TryConsumeAbilityProjectile(uint fireTime)
    {
        while (_pendingAbilityProjectiles.Count > 0)
        {
            var front = _pendingAbilityProjectiles.Peek();
            int age = (int)(fireTime - front.ActivationTime);
            if (age < -(_abilityProjectileStaleMs / 2) || age > (int)_abilityProjectileStaleMs)
            {
                _pendingAbilityProjectiles.Dequeue();
                continue;
            }

            _pendingAbilityProjectiles.Dequeue();
            return front;
        }

        return null;
    }

    /// <summary>
    ///     Consumes the pending ability projectile queued by the given ability execution,
    ///     returning null when none is pending.
    /// </summary>
    /// <param name="executionId">Execution id of the activating ability</param>
    /// <returns>PendingAbilityProjectile or null if none was queued for this execution</returns>
    public PendingAbilityProjectile? TryConsumeAbilityProjectileByExecId(Guid executionId)
    {
        PendingAbilityProjectile? found = null;
        var remaining = new Queue<PendingAbilityProjectile>();
        foreach (var pending in _pendingAbilityProjectiles)
        {
            if (found == null && pending.ExecutionId == executionId)
            {
                found = pending;
                continue;
            }

            remaining.Enqueue(pending);
        }

        _pendingAbilityProjectiles.Clear();
        foreach (var pending in remaining)
        {
            _pendingAbilityProjectiles.Enqueue(pending);
        }

        return found;
    }

    /// <summary>
    ///     Consumes the next pending ability projectile without the client-fire-time staleness filter.
    ///     Used for server-driven NPCs, which have no client FireWeaponProjectile to correlate against.
    /// </summary>
    /// <returns>PendingAbilityProjectile or null if none was queued</returns>
    public PendingAbilityProjectile? TryConsumeNextAbilityProjectile()
    {
        if (_pendingAbilityProjectiles.Count == 0)
        {
            return null;
        }

        return _pendingAbilityProjectiles.Dequeue();
    }

    /// <summary>
    ///     Drops any pending ability projectiles that are too old relative to the given time.
    /// </summary>
    /// <param name="currentTime">Gametime</param>
    public void ExpireAbilityProjectiles(uint currentTime)
    {
        while (_pendingAbilityProjectiles.Count > 0)
        {
            var front = _pendingAbilityProjectiles.Peek();
            int age = (int)(currentTime - front.ActivationTime);
            if (age < -(_abilityProjectileStaleMs / 2) || age > (int)_abilityProjectileStaleMs)
            {
                _pendingAbilityProjectiles.Dequeue();
                continue;
            }

            break;
        }
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
    ///     Records an aim direction from a client FireWeaponProjectile command.
    ///     Only called on the shard thread. Samples older than the newest recorded one are ignored.
    /// </summary>
    /// <param name="clientTimeMs">Client time in ms of the sample</param>
    /// <param name="aim">The aim direction the client reported</param>
    public void RecordAimSample(uint clientTimeMs, Vector3 aim)
    {
        var shortTime = unchecked((ushort)clientTimeMs);

        if (_aimSampleCount > 0)
        {
            var newest = _aimSamples[_aimSampleNewest];
            if (AimSample.MsSince(newest.ShortTime, shortTime) > 0x8000)
            {
                return; // Out-of-order or stale sample
            }
        }

        var index = (_aimSampleNewest + 1) % _maxAimSamples;
        _aimSamples[index] = new AimSample { ShortTime = shortTime, Aim = aim };
        _aimSampleNewest = index;
        if (_aimSampleCount < _maxAimSamples)
        {
            _aimSampleCount++;
        }
    }

    /// <summary>
    ///     Gets the aim direction the client reported most recently at or before the given time. Unlike the
    ///     movement samples there is nothing to interpolate between, so this is the closest reported sample
    ///     rather than a value evaluated at the time, and it falls back to the newest sample when every
    ///     recorded one is newer than the request.
    /// </summary>
    /// <param name="clientTimeMs">Client time in ms to look the aim up at</param>
    /// <param name="aim">The aim direction of the matching sample</param>
    /// <returns>False when no aim has ever been recorded for this character</returns>
    public bool TryGetAimSample(uint clientTimeMs, out Vector3 aim)
    {
        var target = unchecked((ushort)clientTimeMs);

        for (var age = 0; age < _aimSampleCount; age++)
        {
            var sample = _aimSamples[(_aimSampleNewest - age + _maxAimSamples) % _maxAimSamples];
            if (AimSample.MsSince(sample.ShortTime, target) < 0x8000)
            {
                aim = sample.Aim;
                return true;
            }
        }

        if (_aimSampleCount == 0)
        {
            aim = Vector3.Zero;
            return false;
        }

        aim = _aimSamples[_aimSampleNewest].Aim;
        return true;
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

        if (Character_ObserverView != null)
        {
            Character_ObserverView.HostilityInfoProp = HostilityInfo;
        }

        if (Character_BaseController != null)
        {
            Character_BaseController.HostilityInfoProp = HostilityInfo;
        }
    }

    /// <summary>
    ///     Sets the character's own stance towards each faction on top of its faction, and pushes it out to the
    ///     controller. This is where reputation, contracts and temporary PvP overrides land.
    /// </summary>
    /// <param name="newValue">The character's per-faction stance overlay, or null to clear it.</param>
    public void SetPersonalFactionStance(PersonalFactionStance? newValue)
    {
        PersonalFactionStance = newValue;

        if (Character_BaseController != null)
        {
            Character_BaseController.PersonalFactionStanceProp = newValue?.Data;
        }
    }

    public void SetMaxHealth(int newValue, bool resetCurrent)
    {
        MaxHealth = new()
        {
            Value = newValue,
            Time = Shard.CurrentTime,
        };

        if (Character_ObserverView != null)
        {
            Character_ObserverView.MaxHealthProp = MaxHealth;
        }

        if (Character_BaseController != null)
        {
            Character_BaseController.MaxHealthProp = MaxHealth;
        }

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

        if (Character_BaseController != null)
        {
            Character_BaseController.MaxShieldsProp = MaxShields;
        }

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

        if (Character_ObserverView != null)
        {
            Character_ObserverView.CurrentHealthPctProp = pct;
        }

        if (Character_BaseController != null)
        {
            Character_BaseController.CurrentHealthProp = CurrentHealth;
        }
    }

    public void SetCurrentShields(int newValue)
    {
        CurrentShields = Math.Min(Math.Max(0, newValue), MaxShields.Value);

        if (Character_BaseController != null)
        {
            Character_BaseController.CurrentShieldsProp = CurrentShields;
        }
    }

    public void SetRespawnTimes(RespawnTimesData newValue)
    {
        RespawnTimes = newValue;

        if (Character_ObserverView != null)
        {
            Character_ObserverView.RespawnTimesProp = RespawnTimes;
        }

        if (Character_BaseController != null)
        {
            Character_BaseController.RespawnTimesProp = RespawnTimes;
        }
    }

    public void SetGibVisualsInfo(uint gibVisualsId, uint time)
    {
        GibVisualsInfo = new GibVisuals { Id = gibVisualsId, Time = time };
        Character_ObserverView.GibVisualsIDProp = GibVisualsInfo;

        if (Character_BaseController != null)
        {
            Character_BaseController.GibVisualsIdProp = GibVisualsInfo;
        }
    }

    public void SetLastDamage(DamageInfo damageInfo)
    {
        LastDamage = damageInfo;
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
        if (battleframe == null)
        {
            return false;
        }

        return GibVisualsSelector.TrySelectGibVisualsId(battleframe.GibsetId, LastDamage, MaxHealth.Value, CurrentHealth, out gibVisualsId);
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

    public void AddTinyObject(TinyObjectData data)
    {
        uint index = GetFreeTinyIndex();
        if (index == InvalidIndex)
        {
            index = 0;
        }

        SetTinyObject(index, data);
    }

    public uint GetFreeTinyIndex()
    {
        InitTinyObjectView();
        return GetFreeIndexOfArrayProperty(Character_TinyObjectView, "TinyObjects", MaxTinyCount);
    }

    public void SetTinyObject(uint index, TinyObjectData data)
    {
        SetIndexOfArrayProperty(Character_TinyObjectView, "TinyObjects", index, data);
    }

    public void SetInteractionTarget(IEntity target)
    {
        InteractionTarget = target;

        if (target == null)
        {
            InteractionExecutionId = null;
        }
    }

    public void SetAbilityInput(uint abilityId, Guid executionId, uint time)
    {
        _abilityInputs[abilityId] = new AbilityInput
        {
            AbilityId = abilityId,
            ExecutionId = executionId,
            ActivateTime = time,
        };
    }

    public bool TryGetAbilityInput(uint abilityId, out AbilityInput input)
    {
        return _abilityInputs.TryGetValue(abilityId, out input);
    }

    public AbilityInput ClearAbilityInput(uint abilityId)
    {
        if (_abilityInputs.Remove(abilityId, out var input))
        {
            return input;
        }

        return null;
    }

    private static Dictionary<MonsterVisualOptionType, List<uint>> GroupOptionsByType(IEnumerable<MonsterVisualOption> options)
        => options.GroupBy(o => (MonsterVisualOptionType)o.Type)
                    .ToDictionary(g => g.Key, g => g.Select(o => o.Value).ToList());

    private static float FoldStatModifier(float value, ActiveStatModifier mod)
    {
        if (mod.HasCap)
        {
            if (mod.Cap > value)
            {
                value = (value * mod.Multi) + mod.Add;
                if (value >= mod.Cap)
                {
                    value = mod.Cap;
                }
            }
        }
        else
        {
            value = (value * mod.Multi) + mod.Add;
        }

        return value;
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
    /// <param name="sample">The movement sample whose input axes and modifiers drive the estimate.</param>
    /// <returns>The estimated initial movement velocity, or <see cref="Vector3.Zero" /> for negligible input.</returns>
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

        var main = new ActiveWeaponDetails()
        {
            WeaponId = weaponId,
            Weapon = weaponDetails.Main,
            Attributes = attributes,
            WeaponProfile = WeaponProfile.Build(weaponId, weaponDetails.Main, attributes, weaponDetails.Main.MaxSpread),
        };

        cache[index, 0] = main;
        cache[index, 1] = weaponDetails.Alt != null
            ? new ActiveWeaponDetails()
            {
                WeaponId = weaponId,
                Weapon = weaponDetails.Alt,
                Attributes = attributes,
                WeaponProfile = WeaponProfile.Build(weaponId, weaponDetails.Alt, attributes, weaponDetails.Main.MaxSpread),
            }
            : main;
    }

    private static void MergeAttributeRange(Dictionary<ushort, float> result, uint itemId)
    {
        if (itemId == 0)
        {
            return;
        }

        foreach (var range in SDBInterface.GetItemAttributeRange(itemId).Values)
        {
            result[range.AttributeId] = result.GetValueOrDefault(range.AttributeId) + range.Base;
        }
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

    private void RecalculateHealthAndWeaponDamage()
    {
        if (IsNpc)
        {
            RecalculateMonsterStats();
        }
        else
        {
            RecalculatePlayerHealth();
        }
    }

    private void RecalculatePlayerHealth()
    {
        var levelBonus = SDBInterface.GetLevelItemAttribute((uint)ItemAttributeId.Health, Level)?.Value ?? 0f;
        var total = CurrentLoadout.GearHealthAttribute + levelBonus;
        CurrentLoadout.ItemAttributes[(ushort)ItemAttributeId.Health] = total;
        SetMaxHealth((int)Math.Round(total), true);
    }

    private void RecalculateMonsterStats()
    {
        var scaling = SDBInterface.GetMonsterScaling(Level);
        if (scaling == null)
        {
            Logger.Warning("No MonsterScaling row for level {Level} (monster {MonsterId}); leaving health unchanged", Level, _monsterTypeId);
            return;
        }

        // Creature modifiers come from the chassis AttributeRange plus any per-monster MonsterAttributeRange overrides.
        var mods = new Dictionary<ushort, float>();
        MergeAttributeRange(mods, CurrentLoadout.ChassisID);
        foreach (var (attributeId, range) in SDBInterface.GetMonsterAttributeRange(_monsterTypeId))
        {
            mods[attributeId] = mods.GetValueOrDefault(attributeId) + range.Base;
        }

        var hpModifier = mods.GetValueOrDefault((ushort)ItemAttributeId.CreatureHPModifier, 1f);
        var health = scaling.Health * hpModifier;
        CurrentLoadout.ItemAttributes[(ushort)ItemAttributeId.Health] = health;
        SetMaxHealth((int)Math.Round(health), true);

        // Weapon damage scales per weapon by that weapon's own Creature Weapon Damage Modifier (1145).
        ApplyMonsterWeaponDamage(scaling.Damage, LoadoutSlotType.Primary);
        ApplyMonsterWeaponDamage(scaling.Damage, LoadoutSlotType.Secondary);
    }

    private void ApplyMonsterWeaponDamage(uint baseDamage, LoadoutSlotType slot)
    {
        var weaponId = CurrentLoadout.SlottedItems.GetValueOrDefault(slot);
        if (weaponId == 0 || _weaponDetailsCache == null)
        {
            return;
        }

        var weaponMods = new Dictionary<ushort, float>();
        MergeAttributeRange(weaponMods, weaponId);
        var damageModifier = weaponMods.GetValueOrDefault((ushort)ItemAttributeId.CreatureWeaponDamageModifier, 1f);
        var damage = baseDamage * damageModifier;

        // Inject as the WeaponDamage attribute (954) so WeaponSim.ResolveWeaponDamage uses the scaled value.
        for (byte mode = 0; mode <= 1; mode++)
        {
            var details = _weaponDetailsCache[(int)slot, mode];
            if (details != null && !details.IsEmpty)
            {
                details.Attributes[(ushort)ItemAttributeId.WeaponDamage] = damage;
            }
        }
    }

    private void InitTinyObjectView()
    {
        if (Character_TinyObjectView != null)
        {
            return;
        }

        Character_TinyObjectView = new TinyObjectView();
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

    private ulong GetCurrentFrameXp() => Player?.Battleframes.Get(Player.Battleframes.CurrentChassisId)?.CurrentXp ?? 0;

    private void UpdateLevelProps()
    {
        if (Character_BaseController != null)
        {
            Character_BaseController.LevelProp = Level;
            Character_BaseController.EffectiveLevelProp = EffectiveLevel;
            Character_BaseController.EliteLevelProp = EliteLevel;
        }

        if (Character_EquipmentView != null)
        {
            Character_EquipmentView.LevelProp = Level;
            Character_EquipmentView.EffectiveLevelProp = EffectiveLevel;
            Character_EquipmentView.ScalingLevelProp = Level;
            Character_EquipmentView.EliteLevelProp = EliteLevel;
        }
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

        Level = DefaultLevel;
        EffectiveLevel = DefaultLevel;
        EliteLevel = DefaultEliteLevel;

        StaticInfo = new StaticInfoData();
        CharacterState = new CharacterStateData { State = CharacterStateData.CharacterStatus.Living, Time = Shard.CurrentTime };
        HostilityInfo = new HostilityInfoData { Flags = 0 | HostilityInfoData.HostilityFlags.Faction, FactionId = 1 };
        SetMaxShields(0, true);
        SetMaxHealth(19192, true);
        GibVisualsInfo = new GibVisuals { Id = 0, Time = Shard.CurrentTime };
        ProcessDelay = new ProcessDelayData { ChangeTime = 30721, DelayMs = 236 };
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
        ScopeBubble = new ScopeBubbleInfoData { Layer = 0, VisibilityMask = 0 };
        SpawnPose = new CharacterSpawnPose
        {
            Time = Shard.CurrentTime,
            Position = Position,
            Rotation = Orientation,
            AimDirection = AimDirection,
            Velocity = Velocity,
            MovementState = 0x1000,
            MovementType = 0,
            WaterLevelAndDesc = 0,
            JetpackEnergy = 0x639c,
            AirGroundTimer = 0,
            JumpTimer = 0,
            HaveDebugData = 0
        };

        EffectsFlags = 0;
        FireMode_0 = new FireModeData { Mode = 0, Time = Shard.CurrentTime };
        FireMode_1 = new FireModeData { Mode = 0, Time = Shard.CurrentTime };
        WeaponIndex = new WeaponIndexData { Index = 0, PreviousIndex = 1, PlaySwapAnimation = 0, Time = Shard.CurrentTime };

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
            ProgressionXpProp = (uint)GetCurrentFrameXp(),
            PermanentStatusEffectsProp = new PermanentStatusEffectsData { Effects = [] },
            XpBoostModifierProp = new StatModifierData { ModifierId = 0, StatValue = 1.0f },
            XpPermanentModifierProp = new StatModifierData { ModifierId = 0, StatValue = 1.0f },
            XpZoneModifierProp = new StatModifierData { ModifierId = 0, StatValue = 1.0f },
            XpVipModifierProp = new StatModifierData { ModifierId = 0, StatValue = 1.0f },
            XpEventModifierProp = new StatModifierData { ModifierId = 0, StatValue = 1.0f },
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
            LevelProp = Level,
            EffectiveLevelProp = EffectiveLevel,
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
            EliteLevelProp = EliteLevel
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
            PersonalFactionStanceProp = PersonalFactionStance?.Data,
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
            LevelProp = Level,
            EffectiveLevelProp = EffectiveLevel,
            CurrentDurabilityPctProp = 100,
            CharacterStatsProp = CharacterStats,
            ScalingLevelProp = Level, // PIN scales item stats off the character level
            PvPRankProp = 0,
            EliteLevelProp = EliteLevel
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

    private SelectedMonsterVisuals LoadMonsterVisuals(uint typeId)
    {
        var monsterInfo = SDBInterface.GetMonster(typeId);
        var chassisWarpaint = SDBUtils.GetChassisWarpaint(monsterInfo.ChassisId, monsterInfo.FullbodyWarpaintPaletteId, monsterInfo.ArmorWarpaintPaletteId, monsterInfo.BodysuitWarpaintPaletteId, monsterInfo.GlowWarpaintPaletteId);

        var warpaintPatternData = WarpaintPatternData.Parse([.. monsterInfo.WarpaintPatternData]);
        var warpaintTatooData = WarpaintTatooData.Parse([.. monsterInfo.WarpaintTattooData]);

        SelectedMonsterVisuals result = new()
        {
            Sex = monsterInfo.Gender,
            VisualsGroupId = monsterInfo.VisualsGroupId,
            BackpackId = monsterInfo.BackpackId,
            ChassiWarpaint = chassisWarpaint,
            ChassisWarpaintPatterns = warpaintPatternData,
            ChassisWarpaintTatoos = warpaintTatooData,

            HairColor = monsterInfo.HairColor,
            FacialHairColor = monsterInfo.FacialHairColor,
            LipColor = monsterInfo.LipColor,
            NameLocalizationId = monsterInfo.LocalizedNameId,
            EyeColor = monsterInfo.EyeColor,
            HeadId = monsterInfo.HeadId,
            HeadAcc1Id = monsterInfo.HeadAcc1Id,
            HeadAcc2Id = monsterInfo.HeadAcc2Id,
            VoiceSet = monsterInfo.VoiceSet,
            ChassisId = monsterInfo.ChassisId,
            SkinColor = monsterInfo.SkinColor,
            OrnamentsMapGroupId1 = monsterInfo.OrnamentsMapGroupId_1,
            OrnamentsMapGroupId2 = monsterInfo.OrnamentsMapGroupId_2,
            OrnamentsMapGroupId3 = monsterInfo.OrnamentsMapGroupId_3,
            OrnamentsMapGroupId4 = monsterInfo.OrnamentsMapGroupId_4,
        };

        if (monsterInfo.VisualOptionsId == 0)
        {
            return result;
        }

        var record = SDBInterface.GetMonsterVisualOptions(monsterInfo.VisualOptionsId);
        var entries = SDBInterface.GetMonsterVisualOption(monsterInfo.VisualOptionsId);
        var options = GroupOptionsByType(entries);
        var rng = new Random(BitConverter.ToInt32(BitConverter.GetBytes(EntityId), 0));

        char sex = monsterInfo.Gender == 'F' ? 'F' : 'M';

        if (record.Female == 1 && sex != 'F' && rng.Next(2) == 1)
        {
            sex = 'F';
        }
        else if (record.Male == 1 && sex != 'M' && rng.Next(2) == 1)
        {
            sex = 'M';
        }

        result.Sex = sex;

        if (options.TryGetValue(MonsterVisualOptionType.HairColor, out var hairColorOptions))
        {
            var index = rng.Next(hairColorOptions.Count);
            result.HairColor = hairColorOptions[index];
        }

        if (options.TryGetValue(MonsterVisualOptionType.FacialHairColor, out var facialHairColorOptions))
        {
            var index = rng.Next(facialHairColorOptions.Count);
            result.FacialHairColor = facialHairColorOptions[index];
        }

        if (options.TryGetValue(MonsterVisualOptionType.LipColor, out var lipColorOptions))
        {
            var index = rng.Next(lipColorOptions.Count);
            result.LipColor = lipColorOptions[index];
        }

        if (options.TryGetValue(MonsterVisualOptionType.EyeColor, out var eyeColorOptions))
        {
            var index = rng.Next(eyeColorOptions.Count);
            result.EyeColor = eyeColorOptions[index];
        }

        if (options.TryGetValue(MonsterVisualOptionType.SkinColor, out var skinColorOptions))
        {
            var index = rng.Next(skinColorOptions.Count);
            result.SkinColor = skinColorOptions[index];
        }

        if (options.TryGetValue(MonsterVisualOptionType.NameLocalizationId, out var nameLocalizationIdOptions))
        {
            var index = rng.Next(nameLocalizationIdOptions.Count);
            result.NameLocalizationId = nameLocalizationIdOptions[index];
        }

        if (options.TryGetValue(MonsterVisualOptionType.ChassisId, out var chassisIdOptions))
        {
            var index = rng.Next(chassisIdOptions.Count);
            result.ChassisId = chassisIdOptions[index];
            result.ChassiWarpaint = SDBUtils.GetChassisWarpaint(result.ChassisId, monsterInfo.FullbodyWarpaintPaletteId, monsterInfo.ArmorWarpaintPaletteId, monsterInfo.BodysuitWarpaintPaletteId, monsterInfo.GlowWarpaintPaletteId);
        }

        if (options.TryGetValue(MonsterVisualOptionType.ChassiWarpaintPaletteId, out var chassisWarpaintPalleteIdOptions))
        {
            var index = rng.Next(chassisWarpaintPalleteIdOptions.Count);
            var customWarpaint = chassisWarpaintPalleteIdOptions[index];
            result.ChassiWarpaint = SDBUtils.GetChassisWarpaint(result.ChassisId, customWarpaint, monsterInfo.ArmorWarpaintPaletteId, monsterInfo.BodysuitWarpaintPaletteId, monsterInfo.GlowWarpaintPaletteId);
        }

        if (options.TryGetValue(MonsterVisualOptionType.HeadId, out var headOptions))
        {
            var heads = headOptions.Select(id => SDBInterface.GetHead(id)).Where((head) => head.Sex == sex).ToList();
            if (heads.Count > 0)
            {
                var index = rng.Next(heads.Count);
                result.HeadId = heads[index].HeadId;

                var validAccessories = SDBInterface.GetHeadHeadAccessories(result.HeadId);
                if (result.HeadAcc1Id != 0 && !validAccessories.Contains(result.HeadAcc1Id))
                {
                    result.HeadAcc1Id = 0;
                }

                if (result.HeadAcc2Id != 0 && !validAccessories.Contains(result.HeadAcc2Id))
                {
                    result.HeadAcc2Id = 0;
                }
            }
        }

        if (options.TryGetValue(MonsterVisualOptionType.HeadAcc1Id, out var headAcc1Options))
        {
            var validForHead = SDBInterface.GetHeadHeadAccessories(result.HeadId);
            var optionsRows = headAcc1Options;
            var zeros = optionsRows.Where(id => id == 0);
            var filtered = optionsRows.Where(id => id != 0).Select(id => SDBInterface.GetHeadAccessory(id)).Where((ha) => ha.Sex == sex && validForHead.Contains(ha.HaId)).Select((ha) => ha.HaId);

            var valid = zeros.Concat(filtered).ToList();
            if (valid.Count > 0)
            {
                var index = rng.Next(valid.Count);
                result.HeadAcc1Id = valid[index];
            }
        }

        if (options.TryGetValue(MonsterVisualOptionType.HeadAcc2Id, out var headAcc2Options))
        {
            var validForHead = SDBInterface.GetHeadHeadAccessories(result.HeadId);
            var optionsRows = headAcc2Options;
            var zeros = optionsRows.Where(id => id == 0);
            var filtered = optionsRows.Where(id => id != 0).Select(id => SDBInterface.GetHeadAccessory(id)).Where((ha) => ha.Sex == sex && validForHead.Contains(ha.HaId)).Select((ha) => ha.HaId);

            var valid = zeros.Concat(filtered).ToList();
            if (valid.Count > 0)
            {
                var index = rng.Next(valid.Count);
                result.HeadAcc2Id = valid[index];
            }
        }

        if (options.TryGetValue(MonsterVisualOptionType.OrnamentsMapGroupId_1, out var ornamentsGroup1Options))
        {
            var index = rng.Next(ornamentsGroup1Options.Count);
            result.OrnamentsMapGroupId1 = ornamentsGroup1Options[index];
        }

        if (options.TryGetValue(MonsterVisualOptionType.OrnamentsMapGroupId_2, out var ornamentsGroup2Options))
        {
            var index = rng.Next(ornamentsGroup2Options.Count);
            result.OrnamentsMapGroupId2 = ornamentsGroup2Options[index];
        }

        if (options.TryGetValue(MonsterVisualOptionType.OrnamentsMapGroupId_3, out var ornamentsGroup3Options))
        {
            var index = rng.Next(ornamentsGroup3Options.Count);
            result.OrnamentsMapGroupId3 = ornamentsGroup3Options[index];
        }

        if (options.TryGetValue(MonsterVisualOptionType.OrnamentsMapGroupId_4, out var ornamentsGroup4Options))
        {
            var index = rng.Next(ornamentsGroup4Options.Count);
            result.OrnamentsMapGroupId4 = ornamentsGroup4Options[index];
        }

        Logger.Debug("SelectedMonsterVisuals {result}", result);

        return result;
    }

    private void ApplyLoadoutVisuals(IReadOnlyList<LoadoutConfig_Visual> visuals)
    {
        CurrentLoadout.ApplyVisuals(visuals);

        foreach (var visual in visuals)
        {
            switch (visual.VisualType)
            {
                case LoadoutVisualType.Glider:
                    CurrentLoadout.GliderID = visual.ItemSdbId;
                    SetStaticInfo(StaticInfo with { LoadoutGlider = visual.ItemSdbId });
                    break;

                case LoadoutVisualType.Vehicle:
                    CurrentLoadout.VehicleID = visual.ItemSdbId;
                    SetStaticInfo(StaticInfo with { LoadoutVehicle = visual.ItemSdbId });
                    break;
            }
        }
    }

    public record struct SelectedMonsterVisuals
    {
        public char Sex;
        public uint VisualsGroupId;
        public uint BackpackId;
        public ChassisWarpaintResult ChassiWarpaint;
        public WarpaintPatternData ChassisWarpaintPatterns;
        public WarpaintTatooData ChassisWarpaintTatoos;

        public uint HairColor;
        public uint FacialHairColor;
        public uint LipColor;
        public uint NameLocalizationId;
        public uint EyeColor;
        public uint HeadId;
        public uint HeadAcc1Id;
        public uint HeadAcc2Id;
        public uint VoiceSet;
        public uint ChassisId;
        public uint SkinColor;
        public uint OrnamentsMapGroupId1;
        public uint OrnamentsMapGroupId2;
        public uint OrnamentsMapGroupId3;
        public uint OrnamentsMapGroupId4;
    }

    public class ActiveWeaponDetails
    {
        public static readonly ActiveWeaponDetails Empty = new()
        {
            WeaponId = 0,
            Weapon = null,
            Attributes = [],
            WeaponProfile = default,
        };

        public uint WeaponId;
        public WeaponTemplateResult Weapon;
        public Dictionary<ushort, float> Attributes = [];
        public WeaponProfile WeaponProfile;

        public bool IsEmpty => Weapon == null;
        public string DisplayName => Weapon?.DebugName ?? "(not a weapon)";
        public float Spread => WeaponProfile.OtherSpreadPct;
    }
}