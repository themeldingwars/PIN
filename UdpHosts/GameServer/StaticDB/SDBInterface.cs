namespace GameServer.StaticDB;

using System.Collections.Generic;
using System.Linq;
using FauFau.Formats;
using Records.apt;
using Records.aptfs;
using Records.dbcharacter;
using Records.dbencounterdata;
using Records.dbitems;
using Records.dbphysicsmaterials;
using Records.dbvisualrecords;
using Records.dbzonemetadata;
using Records.vcs;

public class SDBInterface
{
    // dbcharacter
    private static Dictionary<uint, CharCreateLoadout> _charCreateLoadout;
    private static Dictionary<uint, Dictionary<byte, CharCreateLoadoutSlots>> _charCreateLoadoutSlots;
    private static Dictionary<uint, Deployable> _deployable;
    private static Dictionary<uint, DeployableFunction> _deployableFunction;
    private static Dictionary<uint, DeployableCategory> _deployableCategory;
    private static Dictionary<uint, Faction> _faction;
    private static List<FactionRelations> _factionRelations;
    private static Dictionary<uint, List<FactionReputations>> _factionReputations;
    private static Dictionary<uint, Monster> _monster;
    private static Dictionary<uint, Turret> _turret;
    private static Dictionary<uint, PoseType> _poseType;
    private static Dictionary<uint, CharInfo> _charInfo;
    private static Dictionary<byte, DamageType> _damageType;
    private static Dictionary<byte, DamageResponse> _damageResponse;
    private static Dictionary<uint, DamageResponseDamageType> _damageResponseDamageType;
    private static Dictionary<uint, TinyObject> _tinyObject;

    // dbencounterdata
    private static Dictionary<uint, MapMarkerInfo> _mapMarkerInfo;
    private static Dictionary<uint, SinCardTemplate> _sinCardTemplate;

    // dbphysicsmaterials
    private static Dictionary<uint, PhysicsMaterial> _physicsMaterial;

    // dbvisualrecords
    private static Dictionary<uint, WarpaintPalette> _warpaintPalettes;
    private static Dictionary<uint, VisualRecord> _visualRecord;

    // dbitems
    private static Dictionary<uint, AttributeCategory> _attributeCategory;
    private static Dictionary<uint, AttributeDefinition> _attributeDefinition;
    private static Dictionary<KeyValuePair<uint, ushort>, AttributeRange> _attributeRange;
    private static Dictionary<KeyValuePair<uint, ushort>, ItemModuleScalars> _itemModuleScalars;
    private static Dictionary<KeyValuePair<uint, ushort>, ItemCharacterScalars> _itemCharacterScalars;
    private static Dictionary<uint, RootItem> _rootItem;
    private static Dictionary<uint, AbilityModule> _abilityModule;
    private static Dictionary<uint, Battleframe> _battleframe;
    private static Dictionary<uint, CarryableObject> _carryableObject;
    private static Dictionary<uint, Weapons> _weapons;
    private static Dictionary<uint, WeaponTemplates> _weaponTemplates;
    private static Dictionary<uint, WeaponTemplateModifiers> _weaponTemplateModifiers;
    private static Dictionary<uint, WeaponScope> _weaponScope;
    private static Dictionary<uint, WeaponUnderbarrel> _weaponUnderbarrel;
    private static Dictionary<uint, Ammo> _ammo;
    private static Dictionary<uint, LevelBand> _levelBand;
    private static Dictionary<uint, ResourceNodeBeacon> _resourceNodeBeacon;
    private static Dictionary<KeyValuePair<uint, uint>, LevelCategoryScalars> _levelCategoryScalars;
    private static Dictionary<uint, FrameProgressionLevel> _frameProgressionLevel;
    private static Dictionary<uint, Blueprints> _blueprints;
    private static Dictionary<uint, List<Blueprint_Items>> _blueprintItems;
    private static Dictionary<uint, List<BattleframeVisuals>> _battleframeVisuals;

    // dbzonemetadata
    private static Dictionary<uint, ZoneRecord> _zoneRecord;

    // apt
    private static Dictionary<uint, BaseCommandDef> _baseCommandDef;
    private static Dictionary<uint, CommandType> _commandType;
    private static Dictionary<uint, StatusEffectData> _statusEffectData;
    private static Dictionary<uint, HashSet<uint>> _statusEffectTag;
    private static Dictionary<uint, AbilityData> _abilityData;
    private static Dictionary<uint, ActiveInitiationCommandDef> _activeInitiationCommandDef;
    private static Dictionary<uint, ImpactApplyEffectCommandDef> _impactApplyEffectCommandDef;
    private static Dictionary<uint, ImpactToggleEffectCommandDef> _impactToggleEffectCommandDef;
    private static Dictionary<uint, ConditionalBranchCommandDef> _conditionalBranchCommandDef;
    private static Dictionary<uint, WhileLoopCommandDef> _whileLoopCommandDef;
    private static Dictionary<uint, LogicNegateCommandDef> _logicNegateCommandDef;
    private static Dictionary<uint, LogicOrCommandDef> _logicOrCommandDef;
    private static Dictionary<uint, LogicOrChainCommandDef> _logicOrChainCommandDef;
    private static Dictionary<uint, LogicAndChainCommandDef> _logicAndChainCommandDef;
    private static Dictionary<uint, CallCommandDef> _callCommandDef;
    private static Dictionary<uint, InstantActivationCommandDef> _instantActivationCommandDef;
    private static Dictionary<uint, StagedActivationCommandDef> _stagedActivationCommandDef;
    private static Dictionary<uint, TargetPBAECommandDef> _targetPBAECommandDef;
    private static Dictionary<uint, TargetConeAECommandDef> _targetConeAECommandDef;
    private static Dictionary<uint, TargetClearCommandDef> _targetClearCommandDef;
    private static Dictionary<uint, TargetSelfCommandDef> _targetSelfCommandDef;
    private static Dictionary<uint, TargetSwapCommandDef> _targetSwapCommandDef;
    private static Dictionary<uint, TargetStackEmptyCommandDef> _targetStackEmptyCommandDef;
    private static Dictionary<uint, TargetInitiatorCommandDef> _targetInitiatorCommandDef;
    private static Dictionary<uint, PeekTargetsCommandDef> _peekTargetsCommandDef;
    private static Dictionary<uint, PopTargetsCommandDef> _popTargetsCommandDef;
    private static Dictionary<uint, PushTargetsCommandDef> _pushTargetsCommandDef;
    private static Dictionary<uint, TimeDurationCommandDef> _timeDurationCommandDef;
    private static Dictionary<uint, AirborneDurationCommandDef> _airborneDurationCommandDef;
    private static Dictionary<uint, ActivationDurationCommandDef> _activationDurationCommandDef;
    private static Dictionary<uint, ReturnCommandDef> _returnCommandDef;
    private static Dictionary<uint, LoadRegisterFromItemStatCommandDef> _loadRegisterFromItemStatCommandDef;
    private static Dictionary<uint, LoadRegisterFromBonusCommandDef> _loadRegisterFromBonusCommandDef;
    private static Dictionary<uint, LoadRegisterFromDamageCommandDef> _loadRegisterFromDamageCommandDef;
    private static Dictionary<uint, LoadRegisterFromLevelCommandDef> _loadRegisterFromLevelCommandDef;
    private static Dictionary<uint, LoadRegisterFromModulePowerCommandDef> _loadRegisterFromModulePowerCommandDef;
    private static Dictionary<uint, LoadRegisterFromNamedVarCommandDef> _loadRegisterFromNamedVarCommandDef;
    private static Dictionary<uint, LoadRegisterFromResourceCommandDef> _loadRegisterFromResourceCommandDef;
    private static Dictionary<uint, LoadRegisterFromStatCommandDef> _loadRegisterFromStatCommandDef;
    private static Dictionary<uint, RegisterComparisonCommandDef> _registerComparisonCommandDef;
    private static Dictionary<uint, RegisterRandomCommandDef> _registerRandomCommandDef;
    private static Dictionary<uint, SetRegisterCommandDef> _setRegisterCommandDef;
    private static Dictionary<uint, NamedVariableAssignCommandDef> _namedVariableAssignCommandDef;
    private static Dictionary<uint, InflictCooldownCommandDef> _inflictCooldownCommandDef;
    private static Dictionary<uint, RequireDamageTypeCommandDef> _requireDamageTypeCommandDef;
    private static Dictionary<uint, TargetSingleCommandDef> _targetSingleCommandDef;
    private static Dictionary<uint, TimeCooldownCommandDef> _timeCooldownCommandDef;
    private static Dictionary<uint, TimedActivationCommandDef> _timedActivationCommandDef;
    private static Dictionary<uint, PassiveInitiationCommandDef> _passiveInitiationCommandDef;
    private static Dictionary<uint, TargetPreviousCommandDef> _targetPreviousCommandDef;
    private static Dictionary<uint, UpdateYieldCommandDef> _updateYieldCommandDef;
    private static Dictionary<uint, TargetDifferenceCommandDef> _targetDifferenceCommandDef;
    private static Dictionary<uint, AimRangeDurationCommandDef> _aimRangeDurationCommandDef;
    private static Dictionary<uint, TargetTrimCommandDef> _targetTrimCommandDef;
    private static Dictionary<uint, BonusGreaterThanCommandDef> _bonusGreaterThanCommandDef;
    private static Dictionary<uint, UpdateWaitCommandDef> _updateWaitCommandDef;
    private static Dictionary<uint, PushRegisterCommandDef> _pushRegisterCommandDef;
    private static Dictionary<uint, PopRegisterCommandDef> _popRegisterCommandDef;
    private static Dictionary<uint, PeekRegisterCommandDef> _peekRegisterCommandDef;
    private static Dictionary<uint, UpdateWaitAndFireOnceCommandDef> _updateWaitAndFireOnceCommandDef;
    private static Dictionary<uint, RegisterLoadScaleCommandDef> _registerLoadScaleCommandDef;

    // aptfs
    private static Dictionary<uint, TargetFriendliesCommandDef> _targetFriendliesCommandDef;
    private static Dictionary<uint, TargetByEffectCommandDef> _targetByEffectCommandDef;
    private static Dictionary<uint, TargetByEffectTagCommandDef> _targetByEffectTagCommandDef;
    private static Dictionary<uint, TargetOwnerCommandDef> _targetOwnerCommandDef;
    private static Dictionary<uint, TargetByObjectTypeCommandDef> _targetByObjectTypeCommandDef;
    private static Dictionary<uint, TargetHostilesCommandDef> _targetHostilesCommandDef;
    private static Dictionary<uint, TargetByCharacterStateCommandDef> _targetByCharacterStateCommandDef;
    private static Dictionary<uint, InflictDamageCommandDef> _inflictDamageCommandDef;
    private static Dictionary<uint, ForcePushCommandDef> _forcePushCommandDef;
    private static Dictionary<uint, RequestBattleFrameListCommandDef> _requestBattleFrameListCommandDef;
    private static Dictionary<uint, ApplyImpulseCommandDef> _applyImpulseCommandDef;
    private static Dictionary<uint, DeployableCalldownCommandDef> _deployableCalldownCommandDef;
    private static Dictionary<uint, VehicleCalldownCommandDef> _vehicleCalldownCommandDef;
    private static Dictionary<uint, FireProjectileCommandDef> _fireProjectileCommandDef;
    private static Dictionary<uint, ResourceNodeBeaconCalldownCommandDef> _resourceNodeBeaconCalldownCommandDef;
    private static Dictionary<uint, AttemptToCalldownVehicleCommandDef> _attemptToCalldownVehicleCommandDef;
    private static Dictionary<uint, RegisterClientProximityCommandDef> _rregisterClientProximityCommandDef;
    private static Dictionary<uint, CombatFlagsCommandDef> _combatFlagsCommandDef;
    private static Dictionary<uint, ApplyFreezeCommandDef> _applyFreezeCommandDef;
    private static Dictionary<uint, OrientationLockCommandDef> _orientationLockCommandDef;
    private static Dictionary<uint, StatModifierCommandDef> _statModifierCommandDef;
    private static Dictionary<uint, RequireAimModeCommandDef> _requireAimModeCommandDef;
    private static Dictionary<uint, RequireArmyCommandDef> _requireArmyCommandDef;
    private static Dictionary<uint, RequireBackstabCommandDef> _requireBackstabCommandDef;
    private static Dictionary<uint, RequireBulletHitCommandDef> _rrequireBulletHitCommandDef;
    private static Dictionary<uint, RequireCAISStateCommandDef> _requireCAISStateCommandDef;
    private static Dictionary<uint, RequireCStateCommandDef> _requireCStateCommandDef;
    private static Dictionary<uint, RequireDamageResponseCommandDef> _requireDamageResponseCommandDef;
    private static Dictionary<uint, RequireEliteLevelCommandDef> _requireEliteLevelCommandDef;
    private static Dictionary<uint, RequireEnergyByRangeCommandDef> _requireEnergyByRangeCommandDef;
    private static Dictionary<uint, RequireEnergyCommandDef> _requireEnergyCommandDef;
    private static Dictionary<uint, RequireEnergyFromTargetCommandDef> _requireEnergyFromTargetCommandDef;
    private static Dictionary<uint, RequireEquippedItemCommandDef> _requireEquippedItemCommandDef;
    private static Dictionary<uint, RequireFriendsCommandDef> _requireFriendsCommandDef;
    private static Dictionary<uint, RequireHasCertificateCommandDef> _requireHasCertificateCommandDef;
    private static Dictionary<uint, RequireHasEffectCommandDef> _requireHasEffectCommandDef;
    private static Dictionary<uint, RequireHasEffectTagCommandDef> _requireHasEffectTagCommandDef;
    private static Dictionary<uint, RequireHasItemCommandDef> _requireHasItemCommandDef;
    private static Dictionary<uint, RequireHasUnlockCommandDef> _requireHasUnlockCommandDef;
    private static Dictionary<uint, RequireHeadshotCommandDef> _requireHeadshotCommandDef;
    private static Dictionary<uint, RequireInCombatCommandDef> _requireInCombatCommandDef;
    private static Dictionary<uint, RequireInRangeCommandDef> _requireInRangeCommandDef;
    private static Dictionary<uint, RequireInVehicleCommandDef> _requireInVehicleCommandDef;
    private static Dictionary<uint, RequireIsNPCCommandDef> _requireIsNPCCommandDef;
    private static Dictionary<uint, RequireItemAttributeCommandDef> _requireItemAttributeCommandDef;
    private static Dictionary<uint, RequireItemDurabilityCommandDef> _requireItemDurabilityCommandDef;
    private static Dictionary<uint, RequireJumpedCommandDef> _requireJumpedCommandDef;
    private static Dictionary<uint, RequireLevelCommandDef> _requireLevelCommandDef;
    private static Dictionary<uint, RequireLineOfSightCommandDef> _rrequireLineOfSightCommandDef;
    private static Dictionary<uint, RequirementServerCommandDef> _requirementServerCommandDef;
    private static Dictionary<uint, RequireMovementFlagsCommandDef> _requireMovementFlagsCommandDef;
    private static Dictionary<uint, RequireMovestateCommandDef> _requireMovestateCommandDef;
    private static Dictionary<uint, RequireMovingCommandDef> _requireMovingCommandDef;
    private static Dictionary<uint, RequireNeedsAmmoCommandDef> _rrequireNeedsAmmoCommandDef;
    private static Dictionary<uint, RequireNotRespawnedCommandDef> _requireNotRespawnedCommandDef;
    private static Dictionary<uint, RequirePermissionCommandDef> _requirePermissionCommandDef;
    private static Dictionary<uint, RequireProjectileSlopeCommandDef> _requireProjectileSlopeCommandDef;
    private static Dictionary<uint, RequireReloadCommandDef> _requireReloadCommandDef;
    private static Dictionary<uint, RequireResourceCommandDef> _requireResourceCommandDef;
    private static Dictionary<uint, RequireResourceFromTargetCommandDef> _requireResourceFromTargetCommandDef;
    private static Dictionary<uint, RequireSinAcquiredCommandDef> _requireSinAcquiredCommandDef;
    private static Dictionary<uint, RequireSprintModifierCommandDef> _requireSprintModifierCommandDef;
    private static Dictionary<uint, RequireSquadLeaderCommandDef> _requireSquadLeaderCommandDef;
    private static Dictionary<uint, RequireSuperChargeCommandDef> _requireSuperChargeCommandDef;
    private static Dictionary<uint, RequireTookDamageCommandDef> _requireTookDamageCommandDef;
    private static Dictionary<uint, RequireWeaponArmedCommandDef> _requireWeaponArmedCommandDef;
    private static Dictionary<uint, RequireWeaponTemplateCommandDef> _requireWeaponTemplateCommandDef;
    private static Dictionary<uint, RequireZoneTypeCommandDef> _requireZoneTypeCommandDef;
    private static Dictionary<uint, InteractionTypeCommandDef> _interactionTypeCommandDef;
    private static Dictionary<uint, TargetInteractivesCommandDef> _targetInteractivesCommandDef;
    private static Dictionary<uint, ImpactMarkInteractivesCommandDef> _impactMarkInteractivesCommandDef;
    private static Dictionary<uint, HasTargetsDurationCommandDef> _hhasTargetsDurationCommandDef;
    private static Dictionary<uint, RopePullCommandDef> _ropePullCommandDef;
    private static Dictionary<uint, SetTargetOffsetCommandDef> _setTargetOffsetCommandDef;
    private static Dictionary<uint, HealDamageCommandDef> _hhealDamageCommandDef;
    private static Dictionary<uint, BullrushCommandDef> _bullrushCommandDef;
    private static Dictionary<uint, EnergyToDamageCommandDef> _energyToDamageCommandDef;
    private static Dictionary<uint, BattleFrameDurationCommandDef> _battleFrameDurationCommandDef;
    private static Dictionary<uint, ShootingDurationCommandDef> _shootingDurationCommandDef;
    private static Dictionary<uint, SwitchWeaponCommandDef> _switchWeaponCommandDef;
    private static Dictionary<uint, StatRequirementCommandDef> _statRequirementCommandDef;
    private static Dictionary<uint, ConsumeEnergyCommandDef> _cconsumeEnergyCommandDef;
    private static Dictionary<uint, TargetClassTypeCommandDef> _targetClassTypeCommandDef;
    private static Dictionary<uint, ClimbLedgeCommandDef> _climbLedgeCommandDef;
    private static Dictionary<uint, CopyInitiationPositionCommandDef> _copyInitiationPositionCommandDef;
    private static Dictionary<uint, SlotAmmoCommandDef> _slotAmmoCommandDef;
    private static Dictionary<uint, AddPhysicsCommandDef> _addPhysicsCommandDef;
    private static Dictionary<uint, TargetCurrentVehicleCommandDef> _targetCurrentVehicleCommandDef;
    private static Dictionary<uint, TargetPassengersCommandDef> _targetPassengersCommandDef;
    private static Dictionary<uint, TargetSquadmatesCommandDef> _targetSquadmatesCommandDef;
    private static Dictionary<uint, SetWeaponDamageCommandDef> _setWeaponDamageCommandDef;
    private static Dictionary<uint, ConsumeEnergyOverTimeCommandDef> _consumeEnergyOverTimeCommandDef;
    private static Dictionary<uint, RequestAbilitySelectionCommandDef> _requestAbilitySelectionCommandDef;
    private static Dictionary<uint, BombardmentCommandDef> _bombardmentCommandDef;
    private static Dictionary<uint, SetProjectileTargetCommandDef> _setProjectileTargetCommandDef;
    private static Dictionary<uint, MovementSlideCommandDef> _movementSlideCommandDef;
    private static Dictionary<uint, TargetFromStatusEffectCommandDef> _targetFromStatusEffectCommandDef;
    private static Dictionary<uint, TargetByDamageResponseCommandDef> _targetByDamageResponseCommandDef;
    private static Dictionary<uint, ForcedMovementDurationCommandDef> _fforcedMovementDurationCommandDef;
    private static Dictionary<uint, FireUiEventCommandDef> _ffireUiEventCommandDef;
    private static Dictionary<uint, UiNamedVariableCommandDef> _uiNamedVariableCommandDef;
    private static Dictionary<uint, DetonateProjectilesCommandDef> _ddetonateProjectilesCommandDef;
    private static Dictionary<uint, SetWeaponDamageTypeCommandDef> _setWeaponDamageTypeCommandDef;
    private static Dictionary<uint, TargetFilterMovestateCommandDef> _targetFilterMovestateCommandDef;
    private static Dictionary<uint, TargetByHostilityCommandDef> _ttargetByHostilityCommandDef;
    private static Dictionary<uint, ConsumeSuperChargeCommandDef> _consumeSuperChargeCommandDef;
    private static Dictionary<uint, TargetByHealthCommandDef> _targetByHealthCommandDef;
    private static Dictionary<uint, RegisterMovementEffectCommandDef> _registerMovementEffectCommandDef;
    private static Dictionary<uint, ApplyAmmoRiderCommandDef> _applyAmmoRiderCommandDef;
    private static Dictionary<uint, TargetFilterByRangeCommandDef> _targetFilterByRangeCommandDef;
    private static Dictionary<uint, OverrideCollisionCommandDef> _ooverrideCollisionCommandDef;
    private static Dictionary<uint, MovementFacingCommandDef> _movementFacingCommandDef;
    private static Dictionary<uint, TargetFilterBySinAcquiredCommandDef> _targetFilterBySinAcquiredCommandDef;
    private static Dictionary<uint, MovementTetherCommandDef> _movementTetherCommandDef;
    private static Dictionary<uint, RegisterLoadFromWeaponCommandDef> _registerLoadFromWeaponCommandDef;
    private static Dictionary<uint, ApplyClientStatusEffectCommandDef> _applyClientStatusEffectCommandDef;
    private static Dictionary<uint, RemoveClientStatusEffectCommandDef> _removeClientStatusEffectCommandDef;
    private static Dictionary<uint, DisableChatBubbleCommandDef> _disableChatBubbleCommandDef;
    private static Dictionary<uint, DisableHealthAndIconCommandDef> _disableHealthAndIconCommandDef;

    // vcs
    private static Dictionary<byte, VehicleClass> _vehicleClass;
    private static Dictionary<ushort, VehicleInfo> _vehicleInfo;
    private static Dictionary<ushort, Dictionary<uint, BaseComponentDef>> _baseComponentDef;
    private static Dictionary<uint, ScopingComponentDef> _scopingComponentDef;
    private static Dictionary<uint, DriverComponentDef> _driverComponentDef;
    private static Dictionary<uint, PassengerComponentDef> _passengerComponentDef;
    private static Dictionary<uint, AbilityComponentDef> _abilityComponentDef;
    private static Dictionary<uint, DamageComponentDef> _damageComponentDef;
    private static Dictionary<uint, StatusEffectComponentDef> _statusEffectComponentDef;
    private static Dictionary<uint, TurretComponentDef> _turretComponentDef;
    private static Dictionary<uint, DeployableComponentDef> _deployableComponentDef;
    private static Dictionary<uint, SpawnPointComponentDef> _spawnPointComponentDef;
    private static Dictionary<uint, HullSegmentDef> _hullSegmentDef;

    public static void Init(StaticDB instance)
    {
        var loader = new StaticDBLoader(instance);

        // dbcharacter
        _charCreateLoadout = loader.LoadCharCreateLoadout();
        _charCreateLoadoutSlots = loader.LoadCharCreateLoadoutSlots();
        _deployable = loader.LoadDeployable();
        _deployableFunction = loader.LoadDeployableFunction();
        _deployableCategory = loader.LoadDeployableCategory();
        _faction = loader.LoadFaction();
        _factionRelations = loader.LoadFactionRelations();
        _factionReputations = loader.LoadFactionReputations();
        _monster = loader.LoadMonster();
        _turret = loader.LoadTurret();
        _poseType = loader.LoadPoseType();
        _charInfo = loader.LoadCharInfo();
        _damageType = loader.LoadDamageType();
        _damageResponse = loader.LoadDamageResponse();
        _damageResponseDamageType = loader.LoadDamageResponseDamageType();
        _tinyObject = loader.LoadTinyObject();

        // dbencounterdata
        _mapMarkerInfo = loader.LoadMapMarkerInfo();
        _sinCardTemplate = loader.LoadSinCardTemplate();

        // dbphysicsmaterials
        _physicsMaterial = loader.LoadPhysicsMaterial();

        // dbvisualrecords
        _warpaintPalettes = loader.LoadWarpaintPalettes();
        _visualRecord = loader.LoadVisualRecord();

        // dbitems
        _attributeCategory = loader.LoadAttributeCategory();
        _attributeDefinition = loader.LoadAttributeDefinition();
        _attributeRange = loader.LoadAttributeRange();
        _itemModuleScalars = loader.LoadItemModuleScalars();
        _itemCharacterScalars = loader.LoadItemCharacterScalars();
        _rootItem = loader.LoadRootItem();
        _abilityModule = loader.LoadAbilityModule();
        _battleframe = loader.LoadBattleframe();
        _carryableObject = loader.LoadCarryableObject();
        _weapons = loader.LoadWeapons();
        _weaponTemplates = loader.LoadWeaponTemplates();
        _weaponTemplateModifiers = loader.LoadWeaponTemplateModifiers();
        _weaponScope = loader.LoadWeaponScope();
        _weaponUnderbarrel = loader.LoadWeaponUnderbarrel();
        _ammo = loader.LoadAmmo();
        _levelBand = loader.LoadLevelBand();
        _resourceNodeBeacon = loader.LoadResourceNodeBeacon();
        _levelCategoryScalars = loader.LoadLevelCategoryScalars();
        _frameProgressionLevel = loader.LoadFrameProgressionLevel();
        _blueprints = loader.LoadBlueprints();
        _blueprintItems = loader.LoadBlueprintItems();
        _battleframeVisuals = loader.LoadBattleframeVisuals();

        // dbzonemetadata
        _zoneRecord = loader.LoadZoneRecord();

        // apt
        _statusEffectData = loader.LoadStatusEffectData();
        _statusEffectTag = loader.LoadStatusEffectTags();
        _baseCommandDef = loader.LoadBaseCommandDef();
        _commandType = loader.LoadCommandType();
        _abilityData = loader.LoadAbilityData();
        _activeInitiationCommandDef = loader.LoadActiveInitiationCommandDef();
        _impactApplyEffectCommandDef = loader.LoadImpactApplyEffectCommandDef();
        _impactToggleEffectCommandDef = loader.LoadImpactToggleEffectCommandDef();
        _whileLoopCommandDef = loader.LoadWhileLoopCommandDef();
        _logicNegateCommandDef = loader.LoadLogicNegateCommandDef();
        _logicOrCommandDef = loader.LoadLogicOrCommandDef();
        _logicOrChainCommandDef = loader.LoadLogicOrChainCommandDef();
        _logicAndChainCommandDef = loader.LoadLogicAndChainCommandDef();
        _callCommandDef = loader.LoadCallCommandDef();
        _instantActivationCommandDef = loader.LoadInstantActivationCommandDef();
        _stagedActivationCommandDef = loader.LoadStagedActivationCommandDef();
        _conditionalBranchCommandDef = loader.LoadConditionalBranchCommandDef();
        _targetPBAECommandDef = loader.LoadTargetPBAECommandDef();
        _targetConeAECommandDef = loader.LoadTargetConeAECommandDef();
        _targetClearCommandDef = loader.LoadTargetClearCommandDef();
        _targetSelfCommandDef = loader.LoadTargetSelfCommandDef();
        _targetSwapCommandDef = loader.LoadTargetSwapCommandDef();
        _targetStackEmptyCommandDef = loader.LoadTargetStackEmptyCommandDef();
        _targetInitiatorCommandDef = loader.LoadTargetInitiatorCommandDef();
        _peekTargetsCommandDef = loader.LoadPeekTargetsCommandDef();
        _popTargetsCommandDef = loader.LoadPopTargetsCommandDef();
        _pushTargetsCommandDef = loader.LoadPushTargetsCommandDef();
        _timeDurationCommandDef = loader.LoadTimeDurationCommandDef();
        _returnCommandDef = loader.LoadReturnCommandDef();
        _loadRegisterFromItemStatCommandDef = loader.LoadLoadRegisterFromItemStatCommandDef();
        _loadRegisterFromBonusCommandDef = loader.LoadLoadRegisterFromBonusCommandDef();
        _loadRegisterFromDamageCommandDef = loader.LoadLoadRegisterFromDamageCommandDef();
        _loadRegisterFromLevelCommandDef = loader.LoadLoadRegisterFromLevelCommandDef();
        _loadRegisterFromModulePowerCommandDef = loader.LoadLoadRegisterFromModulePowerCommandDef();
        _loadRegisterFromNamedVarCommandDef = loader.LoadLoadRegisterFromNamedVarCommandDef();
        _loadRegisterFromResourceCommandDef = loader.LoadLoadRegisterFromResourceCommandDef();
        _loadRegisterFromStatCommandDef = loader.LoadLoadRegisterFromStatCommandDef();
        _registerComparisonCommandDef = loader.LoadRegisterComparisonCommandDef();
        _registerRandomCommandDef = loader.LoadRegisterRandomCommandDef();
        _setRegisterCommandDef = loader.LoadSetRegisterCommandDef();
        _namedVariableAssignCommandDef = loader.LoadNamedVariableAssignCommandDef();
        _inflictCooldownCommandDef = loader.LoadInflictCooldownCommandDef();
        _requireDamageTypeCommandDef = loader.LoadRequireDamageTypeCommandDef();
        _targetSingleCommandDef = loader.LoadTargetSingleCommandDef();
        _timeCooldownCommandDef = loader.LoadTimeCooldownCommandDef();
        _timedActivationCommandDef = loader.LoadTimedActivationCommandDef();
        _passiveInitiationCommandDef = loader.LoadPassiveInitiationCommandDef();
        _targetPreviousCommandDef = loader.LoadTargetPreviousCommandDef();
        _updateYieldCommandDef = loader.LoadUpdateYieldCommandDef();
        _targetDifferenceCommandDef = loader.LoadTargetDifferenceCommandDef();
        _aimRangeDurationCommandDef = loader.LoadAimRangeDurationCommandDef();
        _targetTrimCommandDef = loader.LoadTargetTrimCommandDef();
        _bonusGreaterThanCommandDef = loader.LoadBonusGreaterThanCommandDef();
        _updateWaitCommandDef = loader.LoadUpdateWaitCommandDef();
        _pushRegisterCommandDef = loader.LoadPushRegisterCommandDef();
        _popRegisterCommandDef = loader.LoadPopRegisterCommandDef();
        _peekRegisterCommandDef = loader.LoadPeekRegisterCommandDef();
        _updateWaitAndFireOnceCommandDef = loader.LoadUpdateWaitAndFireOnceCommandDef();
        _registerLoadScaleCommandDef = loader.LoadRegisterLoadScaleCommandDef();

        // aptfs
        _targetFriendliesCommandDef = loader.LoadTargetFriendliesCommandDef();
        _targetByEffectCommandDef = loader.LoadTargetByEffectCommandDef();
        _targetByEffectTagCommandDef = loader.LoadTargetByEffectTagCommandDef();
        _targetOwnerCommandDef = loader.LoadTargetOwnerCommandDef();
        _targetByObjectTypeCommandDef = loader.LoadTargetByObjectTypeCommandDef();
        _targetHostilesCommandDef = loader.LoadTargetHostilesCommandDef();
        _targetByCharacterStateCommandDef = loader.LoadTargetByCharacterStateCommandDef();
        _inflictDamageCommandDef = loader.LoadInflictDamageCommandDef();
        _forcePushCommandDef = loader.LoadForcePushCommandDef();
        _requestBattleFrameListCommandDef = loader.LoadRequestBattleFrameListCommandDef();
        _applyImpulseCommandDef = loader.LoadApplyImpulseCommandDef();
        _deployableCalldownCommandDef = loader.LoadDeployableCalldownCommandDef();
        _fireProjectileCommandDef = loader.LoadFireProjectileCommandDef();
        _resourceNodeBeaconCalldownCommandDef = loader.LoadResourceNodeBeaconCalldownCommandDef();
        _attemptToCalldownVehicleCommandDef = loader.LoadAttemptToCalldownVehicleCommandDef();
        _vehicleCalldownCommandDef = loader.LoadVehicleCalldownCommandDef();
        _rregisterClientProximityCommandDef = loader.LoadRegisterClientProximityCommandDef();
        _combatFlagsCommandDef = loader.LoadCombatFlagsCommandDef();
        _applyFreezeCommandDef = loader.LoadApplyFreezeCommandDef();
        _orientationLockCommandDef = loader.LoadOrientationLockCommandDef();
        _statModifierCommandDef = loader.LoadStatModifierCommandDef();
        _requireAimModeCommandDef = loader.LoadRequireAimModeCommandDef();
        _requireArmyCommandDef = loader.LoadRequireArmyCommandDef();
        _requireBackstabCommandDef = loader.LoadRequireBackstabCommandDef();
        _rrequireBulletHitCommandDef = loader.LoadRequireBulletHitCommandDef();
        _requireCAISStateCommandDef = loader.LoadRequireCAISStateCommandDef();
        _requireCStateCommandDef = loader.LoadRequireCStateCommandDef();
        _requireDamageResponseCommandDef = loader.LoadRequireDamageResponseCommandDef();
        _requireEliteLevelCommandDef = loader.LoadRequireEliteLevelCommandDef();
        _requireEnergyByRangeCommandDef = loader.LoadRequireEnergyByRangeCommandDef();
        _requireEnergyCommandDef = loader.LoadRequireEnergyCommandDef();
        _requireEnergyFromTargetCommandDef = loader.LoadRequireEnergyFromTargetCommandDef();
        _requireEquippedItemCommandDef = loader.LoadRequireEquippedItemCommandDef();
        _requireFriendsCommandDef = loader.LoadRequireFriendsCommandDef();
        _requireHasCertificateCommandDef = loader.LoadRequireHasCertificateCommandDef();
        _requireHasEffectCommandDef = loader.LoadRequireHasEffectCommandDef();
        _requireHasEffectTagCommandDef = loader.LoadRequireHasEffectTagCommandDef();
        _requireHasItemCommandDef = loader.LoadRequireHasItemCommandDef();
        _requireHasUnlockCommandDef = loader.LoadRequireHasUnlockCommandDef();
        _requireHeadshotCommandDef = loader.LoadRequireHeadshotCommandDef();
        _requireInCombatCommandDef = loader.LoadRequireInCombatCommandDef();
        _requireInRangeCommandDef = loader.LoadRequireInRangeCommandDef();
        _requireInVehicleCommandDef = loader.LoadRequireInVehicleCommandDef();
        _requireIsNPCCommandDef = loader.LoadRequireIsNPCCommandDef();
        _requireItemAttributeCommandDef = loader.LoadRequireItemAttributeCommandDef();
        _requireItemDurabilityCommandDef = loader.LoadRequireItemDurabilityCommandDef();
        _requireJumpedCommandDef = loader.LoadRequireJumpedCommandDef();
        _requireLevelCommandDef = loader.LoadRequireLevelCommandDef();
        _rrequireLineOfSightCommandDef = loader.LoadRequireLineOfSightCommandDef();
        _requirementServerCommandDef = loader.LoadRequirementServerCommandDef();
        _requireMovementFlagsCommandDef = loader.LoadRequireMovementFlagsCommandDef();
        _requireMovestateCommandDef = loader.LoadRequireMovestateCommandDef();
        _requireMovingCommandDef = loader.LoadRequireMovingCommandDef();
        _rrequireNeedsAmmoCommandDef = loader.LoadRequireNeedsAmmoCommandDef();
        _requireNotRespawnedCommandDef = loader.LoadRequireNotRespawnedCommandDef();
        _requirePermissionCommandDef = loader.LoadRequirePermissionCommandDef();
        _requireProjectileSlopeCommandDef = loader.LoadRequireProjectileSlopeCommandDef();
        _requireReloadCommandDef = loader.LoadRequireReloadCommandDef();
        _requireResourceCommandDef = loader.LoadRequireResourceCommandDef();
        _requireResourceFromTargetCommandDef = loader.LoadRequireResourceFromTargetCommandDef();
        _requireSinAcquiredCommandDef = loader.LoadRequireSinAcquiredCommandDef();
        _requireSprintModifierCommandDef = loader.LoadRequireSprintModifierCommandDef();
        _requireSquadLeaderCommandDef = loader.LoadRequireSquadLeaderCommandDef();
        _requireSuperChargeCommandDef = loader.LoadRequireSuperChargeCommandDef();
        _requireTookDamageCommandDef = loader.LoadRequireTookDamageCommandDef();
        _requireWeaponArmedCommandDef = loader.LoadRequireWeaponArmedCommandDef();
        _requireWeaponTemplateCommandDef = loader.LoadRequireWeaponTemplateCommandDef();
        _requireZoneTypeCommandDef = loader.LoadRequireZoneTypeCommandDef();
        _airborneDurationCommandDef = loader.LoadAirborneDurationCommandDef();
        _activationDurationCommandDef = loader.LoadActivationDurationCommandDef();
        _interactionTypeCommandDef = loader.LoadInteractionTypeCommandDef();
        _targetInteractivesCommandDef = loader.LoadTargetInteractivesCommandDef();
        _impactMarkInteractivesCommandDef = loader.LoadImpactMarkInteractivesCommandDef();
        _hhasTargetsDurationCommandDef = loader.LoadHasTargetsDurationCommandDef();
        _ropePullCommandDef = loader.LoadRopePullCommandDef();
        _setTargetOffsetCommandDef = loader.LoadSetTargetOffsetCommandDef();
        _hhealDamageCommandDef = loader.LoadHealDamageCommandDef();
        _bullrushCommandDef = loader.LoadBullrushCommandDef();
        _energyToDamageCommandDef = loader.LoadEnergyToDamageCommandDef();
        _battleFrameDurationCommandDef = loader.LoadBattleFrameDurationCommandDef();
        _shootingDurationCommandDef = loader.LoadShootingDurationCommandDef();
        _switchWeaponCommandDef = loader.LoadSwitchWeaponCommandDef();
        _statRequirementCommandDef = loader.LoadStatRequirementCommandDef();
        _cconsumeEnergyCommandDef = loader.LoadConsumeEnergyCommandDef();
        _targetClassTypeCommandDef = loader.LoadTargetClassTypeCommandDef();
        _climbLedgeCommandDef = loader.LoadClimbLedgeCommandDef();
        _copyInitiationPositionCommandDef = loader.LoadCopyInitiationPositionCommandDef();
        _slotAmmoCommandDef = loader.LoadSlotAmmoCommandDef();
        _addPhysicsCommandDef = loader.LoadAddPhysicsCommandDef();
        _targetCurrentVehicleCommandDef = loader.LoadTargetCurrentVehicleCommandDef();
        _targetPassengersCommandDef = loader.LoadTargetPassengersCommandDef();
        _targetSquadmatesCommandDef = loader.LoadTargetSquadmatesCommandDef();
        _setWeaponDamageCommandDef = loader.LoadSetWeaponDamageCommandDef();
        _consumeEnergyOverTimeCommandDef = loader.LoadConsumeEnergyOverTimeCommandDef();
        _requestAbilitySelectionCommandDef = loader.LoadRequestAbilitySelectionCommandDef();
        _bombardmentCommandDef = loader.LoadBombardmentCommandDef();
        _setProjectileTargetCommandDef = loader.LoadSetProjectileTargetCommandDef();
        _movementSlideCommandDef = loader.LoadMovementSlideCommandDef();
        _targetFromStatusEffectCommandDef = loader.LoadTargetFromStatusEffectCommandDef();
        _targetByDamageResponseCommandDef = loader.LoadTargetByDamageResponseCommandDef();
        _fforcedMovementDurationCommandDef = loader.LoadForcedMovementDurationCommandDef();
        _ffireUiEventCommandDef = loader.LoadFireUiEventCommandDef();
        _uiNamedVariableCommandDef = loader.LoadUiNamedVariableCommandDef();
        _ddetonateProjectilesCommandDef = loader.LoadDetonateProjectilesCommandDef();
        _setWeaponDamageTypeCommandDef = loader.LoadSetWeaponDamageTypeCommandDef();
        _targetFilterMovestateCommandDef = loader.LoadTargetFilterMovestateCommandDef();
        _ttargetByHostilityCommandDef = loader.LoadTargetByHostilityCommandDef();
        _consumeSuperChargeCommandDef = loader.LoadConsumeSuperChargeCommandDef();
        _targetByHealthCommandDef = loader.LoadTargetByHealthCommandDef();
        _registerMovementEffectCommandDef = loader.LoadRegisterMovementEffectCommandDef();
        _applyAmmoRiderCommandDef = loader.LoadApplyAmmoRiderCommandDef();
        _targetFilterByRangeCommandDef = loader.LoadTargetFilterByRangeCommandDef();
        _ooverrideCollisionCommandDef = loader.LoadOverrideCollisionCommandDef();
        _movementFacingCommandDef = loader.LoadMovementFacingCommandDef();
        _targetFilterBySinAcquiredCommandDef = loader.LoadTargetFilterBySinAcquiredCommandDef();
        _movementTetherCommandDef = loader.LoadMovementTetherCommandDef();
        _registerLoadFromWeaponCommandDef = loader.LoadRegisterLoadFromWeaponCommandDef();
        _applyClientStatusEffectCommandDef = loader.LoadApplyClientStatusEffectCommandDef();
        _removeClientStatusEffectCommandDef = loader.LoadRemoveClientStatusEffectCommandDef();
        _disableChatBubbleCommandDef = loader.LoadDisableChatBubbleCommandDef();
        _disableHealthAndIconCommandDef = loader.LoadDisableHealthAndIconCommandDef();

        // vcs
        _vehicleClass = loader.LoadVehicleClass();
        _vehicleInfo = loader.LoadVehicleInfo();
        _baseComponentDef = loader.LoadBaseComponentDef();
        _scopingComponentDef = loader.LoadScopingComponentDef();
        _driverComponentDef = loader.LoadDriverComponentDef();
        _passengerComponentDef = loader.LoadPassengerComponentDef();
        _abilityComponentDef = loader.LoadAbilityComponentDef();
        _damageComponentDef = loader.LoadDamageComponentDef();
        _statusEffectComponentDef = loader.LoadStatusEffectComponentDef();
        _turretComponentDef = loader.LoadTurretComponentDef();
        _deployableComponentDef = loader.LoadDeployableComponentDef();
        _spawnPointComponentDef = loader.LoadSpawnPointComponentDef();
        _hullSegmentDef = loader.LoadHullSegmentDef();
    }

    // dbcharacter
    public static CharCreateLoadout GetCharCreateLoadout(uint id) => _charCreateLoadout.GetValueOrDefault(id);
    public static CharCreateLoadout[] GetCharCreateLoadoutsByFrame(uint frameId) => [.. _charCreateLoadout
    .Select(pair => pair.Value)
    .Where(value => value.FrameId == frameId)];

    public static AttributeCategory GetAttributeCategory(uint id) => _attributeCategory.GetValueOrDefault(id);
    public static AttributeDefinition GetAttributeDefinition(uint id) => _attributeDefinition.GetValueOrDefault(id);
    public static Dictionary<ushort, AttributeRange> GetItemAttributeRange(uint itemId)
    {
        return _attributeRange
        .Where(pair => pair.Key.Key.Equals(itemId))
        .Select(pair => new KeyValuePair<ushort, AttributeRange>(pair.Key.Value, pair.Value))
        .ToDictionary();
    }

    public static Dictionary<ushort, (float, float)> GetItemModuleScalars(uint itemId)
    {
        return _itemModuleScalars
        .Where(pair => pair.Key.Key.Equals(itemId))
        .Select(pair => new KeyValuePair<ushort, (float value, float perLevel)>(pair.Value.AttributeCategory, (pair.Value.Value, pair.Value.PerLevel)))
        .ToDictionary();
    }

    public static Dictionary<ushort, (float, float)> GetItemCharacterScalars(uint itemId)
    {
        return _itemCharacterScalars
        .Where(pair => pair.Key.Key.Equals(itemId))
        .Select(pair => new KeyValuePair<ushort, (float value, float perLevel)>(pair.Value.AttributeCategory, (pair.Value.Value, pair.Value.PerLevel)))
        .ToDictionary();
    }

    public static Dictionary<byte, CharCreateLoadoutSlots> GetCharCreateLoadoutSlots(uint id) => _charCreateLoadoutSlots.GetValueOrDefault(id);
    public static Deployable GetDeployable(uint id) => _deployable.GetValueOrDefault(id);
    public static DeployableFunction GetDeployableFunction(uint id) => _deployableFunction.GetValueOrDefault(id);
    public static DeployableCategory GetDeployableCategory(uint id) => _deployableCategory.GetValueOrDefault(id);
    public static DamageType GetDamageType(byte id) => _damageType.GetValueOrDefault(id);
    public static DamageResponse GetDamageResponse(byte id) => _damageResponse.GetValueOrDefault(id);
    public static DamageResponseDamageType GetDamageResponseDamageType(uint id) => _damageResponseDamageType.GetValueOrDefault(id);
    public static TinyObject GetTinyObject(uint id) => _tinyObject.GetValueOrDefault(id);
    public static Faction GetFaction(uint id) => _faction.GetValueOrDefault(id);
    public static List<Faction> GetFactions() => [.. _faction.Select(pair => pair.Value)];
    public static List<FactionRelations> GetFactionRelations() => _factionRelations;
    public static List<FactionReputations> GetFactionReputations(uint id) => _factionReputations.GetValueOrDefault(id);

    public static Monster GetMonster(uint id) => _monster.GetValueOrDefault(id);
    public static Turret GetTurret(uint id) => _turret.GetValueOrDefault(id);
    public static PoseType GetPoseType(uint id) => _poseType.GetValueOrDefault(id);
    public static CharInfo GetCharInfo(uint id) => _charInfo.GetValueOrDefault(id);

    // dbencounterdata
    public static MapMarkerInfo GetMapMarkerInfo(uint id) => _mapMarkerInfo.GetValueOrDefault(id);
    public static SinCardTemplate GetSinCardTemplate(uint id) => _sinCardTemplate.GetValueOrDefault(id);

    // dbphysicsMaterial
    public static PhysicsMaterial GetPhysicsMaterial(uint id) => _physicsMaterial.GetValueOrDefault(id);

    // dbvisualrecords
    public static WarpaintPalette GetWarpaintPalette(uint id) => _warpaintPalettes.GetValueOrDefault(id);
    public static VisualRecord GetVisualRecord(uint id) => _visualRecord.GetValueOrDefault(id);

    // dbitems
    public static RootItem GetRootItem(uint id) => _rootItem.GetValueOrDefault(id);
    public static AbilityModule GetAbilityModule(uint id) => _abilityModule.GetValueOrDefault(id);
    public static Battleframe GetBattleframe(uint id) => _battleframe.GetValueOrDefault(id);
    public static CarryableObject GetCarryableObject(uint id) => _carryableObject.GetValueOrDefault(id);
    public static Weapons GetWeapon(uint id) => _weapons.GetValueOrDefault(id);
    public static WeaponTemplates GetWeaponTemplate(uint id) => _weaponTemplates.GetValueOrDefault(id);
    public static WeaponTemplateModifiers GetWeaponTemplateModifiers(uint id) => _weaponTemplateModifiers.GetValueOrDefault(id);
    public static WeaponScope GetWeaponScope(uint id) => _weaponScope.GetValueOrDefault(id);
    public static WeaponUnderbarrel GetWeaponUnderbarrel(uint id) => _weaponUnderbarrel.GetValueOrDefault(id);
    public static Ammo GetAmmo(uint id) => _ammo.GetValueOrDefault(id);
    public static LevelBand GetLevelBand(uint id) => _levelBand.GetValueOrDefault(id);
    public static ResourceNodeBeacon GetResourceNodeBeacon(uint id) => _resourceNodeBeacon.GetValueOrDefault(id);
    public static LevelCategoryScalars GetLevelCategoryScalar(uint attributeCategory, uint level) => _levelCategoryScalars.GetValueOrDefault(new KeyValuePair<uint, uint>(attributeCategory, level));
    public static FrameProgressionLevel GetFrameProgressionLevel(uint level) => _frameProgressionLevel.GetValueOrDefault(level);
    public static Blueprints GetBlueprint(uint id) => _blueprints.GetValueOrDefault(id);
    public static List<Blueprint_Items> GetBlueprintItems(uint blueprintId) => _blueprintItems.GetValueOrDefault(blueprintId);
    public static List<BattleframeVisuals> GetBattleframeVisuals(uint id) => _battleframeVisuals.GetValueOrDefault(id);

    // dbzonemetadata
    public static ZoneRecord GetZoneRecord(uint id) => _zoneRecord.GetValueOrDefault(id);

    // apt
    public static BaseCommandDef GetBaseCommandDef(uint id) => _baseCommandDef.GetValueOrDefault(id);
    public static CommandType GetCommandType(uint id) => _commandType.GetValueOrDefault(id);
    public static AbilityData GetAbilityData(uint id) => _abilityData.GetValueOrDefault(id);
    public static ActiveInitiationCommandDef GetActiveInitiationCommandDef(uint id) => _activeInitiationCommandDef.GetValueOrDefault(id);
    public static StatusEffectData GetStatusEffectData(uint id) => _statusEffectData.GetValueOrDefault(id);
    public static HashSet<uint> GetStatusEffectTag(uint id) => _statusEffectTag.GetValueOrDefault(id);
    public static ImpactApplyEffectCommandDef GetImpactApplyEffectCommandDef(uint id) => _impactApplyEffectCommandDef.GetValueOrDefault(id);
    public static ImpactToggleEffectCommandDef GetImpactToggleEffectCommandDef(uint id) => _impactToggleEffectCommandDef.GetValueOrDefault(id);
    public static ConditionalBranchCommandDef GetConditionalBranchCommandDef(uint id) => _conditionalBranchCommandDef.GetValueOrDefault(id);
    public static WhileLoopCommandDef GetWhileLoopCommandDef(uint id) => _whileLoopCommandDef.GetValueOrDefault(id);
    public static LogicNegateCommandDef GetLogicNegateCommandDef(uint id) => _logicNegateCommandDef.GetValueOrDefault(id);
    public static LogicOrCommandDef GetLogicOrCommandDef(uint id) => _logicOrCommandDef.GetValueOrDefault(id);
    public static LogicOrChainCommandDef GetLogicOrChainCommandDef(uint id) => _logicOrChainCommandDef.GetValueOrDefault(id);
    public static LogicAndChainCommandDef GetLogicAndChainCommandDef(uint id) => _logicAndChainCommandDef.GetValueOrDefault(id);
    public static CallCommandDef GetCallCommandDef(uint id) => _callCommandDef.GetValueOrDefault(id);
    public static InstantActivationCommandDef GetInstantActivationCommandDef(uint id) => _instantActivationCommandDef.GetValueOrDefault(id);
    public static StagedActivationCommandDef GetStagedActivationCommandDef(uint id) => _stagedActivationCommandDef.GetValueOrDefault(id);
    public static TargetPBAECommandDef GetTargetPBAECommandDef(uint id) => _targetPBAECommandDef.GetValueOrDefault(id);
    public static TargetConeAECommandDef GetTargetConeAECommandDef(uint id) => _targetConeAECommandDef.GetValueOrDefault(id);
    public static TargetClearCommandDef GetTargetClearCommandDef(uint id) => _targetClearCommandDef.GetValueOrDefault(id);
    public static TargetSelfCommandDef GetTargetSelfCommandDef(uint id) => _targetSelfCommandDef.GetValueOrDefault(id);
    public static TargetInitiatorCommandDef GetTargetInitiatorCommandDef(uint id) => _targetInitiatorCommandDef.GetValueOrDefault(id);
    public static TargetSwapCommandDef GetTargetSwapCommandDef(uint id) => _targetSwapCommandDef.GetValueOrDefault(id);
    public static TargetStackEmptyCommandDef GetTargetStackEmptyCommandDef(uint id) => _targetStackEmptyCommandDef.GetValueOrDefault(id);
    public static PeekTargetsCommandDef GetPeekTargetsCommandDef(uint id) => _peekTargetsCommandDef.GetValueOrDefault(id);
    public static PopTargetsCommandDef GetPopTargetsCommandDef(uint id) => _popTargetsCommandDef.GetValueOrDefault(id);
    public static PushTargetsCommandDef GetPushTargetsCommandDef(uint id) => _pushTargetsCommandDef.GetValueOrDefault(id);
    public static TimeDurationCommandDef GetTimeDurationCommandDef(uint id) => _timeDurationCommandDef.GetValueOrDefault(id);
    public static AirborneDurationCommandDef GetAirborneDurationCommandDef(uint id) => _airborneDurationCommandDef.GetValueOrDefault(id);
    public static ActivationDurationCommandDef GetActivationDurationCommandDef(uint id) => _activationDurationCommandDef.GetValueOrDefault(id);
    public static ReturnCommandDef GetReturnCommandDef(uint id) => _returnCommandDef.GetValueOrDefault(id);
    public static LoadRegisterFromItemStatCommandDef GetLoadRegisterFromItemStatCommandDef(uint id) => _loadRegisterFromItemStatCommandDef.GetValueOrDefault(id);
    public static LoadRegisterFromBonusCommandDef GetLoadRegisterFromBonusCommandDef(uint id) => _loadRegisterFromBonusCommandDef.GetValueOrDefault(id);
    public static LoadRegisterFromDamageCommandDef GetLoadRegisterFromDamageCommandDef(uint id) => _loadRegisterFromDamageCommandDef.GetValueOrDefault(id);
    public static LoadRegisterFromLevelCommandDef GetLoadRegisterFromLevelCommandDef(uint id) => _loadRegisterFromLevelCommandDef.GetValueOrDefault(id);
    public static LoadRegisterFromModulePowerCommandDef GetLoadRegisterFromModulePowerCommandDef(uint id) => _loadRegisterFromModulePowerCommandDef.GetValueOrDefault(id);
    public static LoadRegisterFromNamedVarCommandDef GetLoadRegisterFromNamedVarCommandDef(uint id) => _loadRegisterFromNamedVarCommandDef.GetValueOrDefault(id);
    public static LoadRegisterFromResourceCommandDef GetLoadRegisterFromResourceCommandDef(uint id) => _loadRegisterFromResourceCommandDef.GetValueOrDefault(id);
    public static LoadRegisterFromStatCommandDef GetLoadRegisterFromStatCommandDef(uint id) => _loadRegisterFromStatCommandDef.GetValueOrDefault(id);
    public static RegisterComparisonCommandDef GetRegisterComparisonCommandDef(uint id) => _registerComparisonCommandDef.GetValueOrDefault(id);
    public static RegisterRandomCommandDef GetRegisterRandomCommandDef(uint id) => _registerRandomCommandDef.GetValueOrDefault(id);
    public static SetRegisterCommandDef GetSetRegisterCommandDef(uint id) => _setRegisterCommandDef.GetValueOrDefault(id);
    public static NamedVariableAssignCommandDef GetNamedVariableAssignCommandDef(uint id) => _namedVariableAssignCommandDef.GetValueOrDefault(id);
    public static InflictCooldownCommandDef GetInflictCooldownCommandDef(uint id) => _inflictCooldownCommandDef.GetValueOrDefault(id);
    public static RequireDamageTypeCommandDef GetRequireDamageTypeCommandDef(uint id) => _requireDamageTypeCommandDef.GetValueOrDefault(id);
    public static TargetSingleCommandDef GetTargetSingleCommandDef(uint id) => _targetSingleCommandDef.GetValueOrDefault(id);
    public static TimeCooldownCommandDef GetTimeCooldownCommandDef(uint id) => _timeCooldownCommandDef.GetValueOrDefault(id);
    public static TimedActivationCommandDef GetTimedActivationCommandDef(uint id) => _timedActivationCommandDef.GetValueOrDefault(id);
    public static PassiveInitiationCommandDef GetPassiveInitiationCommandDef(uint id) => _passiveInitiationCommandDef.GetValueOrDefault(id);
    public static TargetPreviousCommandDef GetTargetPreviousCommandDef(uint id) => _targetPreviousCommandDef.GetValueOrDefault(id);
    public static UpdateYieldCommandDef GetUpdateYieldCommandDef(uint id) => _updateYieldCommandDef.GetValueOrDefault(id);
    public static TargetDifferenceCommandDef GetTargetDifferenceCommandDef(uint id) => _targetDifferenceCommandDef.GetValueOrDefault(id);
    public static AimRangeDurationCommandDef GetAimRangeDurationCommandDef(uint id) => _aimRangeDurationCommandDef.GetValueOrDefault(id);
    public static TargetTrimCommandDef GetTargetTrimCommandDef(uint id) => _targetTrimCommandDef.GetValueOrDefault(id);
    public static BonusGreaterThanCommandDef GetBonusGreaterThanCommandDef(uint id) => _bonusGreaterThanCommandDef.GetValueOrDefault(id);
    public static UpdateWaitCommandDef GetUpdateWaitCommandDef(uint id) => _updateWaitCommandDef.GetValueOrDefault(id);
    public static PushRegisterCommandDef GetPushRegisterCommandDef(uint id) => _pushRegisterCommandDef.GetValueOrDefault(id);
    public static PopRegisterCommandDef GetPopRegisterCommandDef(uint id) => _popRegisterCommandDef.GetValueOrDefault(id);
    public static PeekRegisterCommandDef GetPeekRegisterCommandDef(uint id) => _peekRegisterCommandDef.GetValueOrDefault(id);
    public static UpdateWaitAndFireOnceCommandDef GetUpdateWaitAndFireOnceCommandDef(uint id) => _updateWaitAndFireOnceCommandDef.GetValueOrDefault(id);
    public static RegisterLoadScaleCommandDef GetRegisterLoadScaleCommandDef(uint id) => _registerLoadScaleCommandDef.GetValueOrDefault(id);

    // aptfs
    public static TargetFriendliesCommandDef GetTargetFriendliesCommandDef(uint id) => _targetFriendliesCommandDef.GetValueOrDefault(id);
    public static TargetByEffectCommandDef GetTargetByEffectCommandDef(uint id) => _targetByEffectCommandDef.GetValueOrDefault(id);
    public static TargetByEffectTagCommandDef GetTargetByEffectTagCommandDef(uint id) => _targetByEffectTagCommandDef.GetValueOrDefault(id);
    public static TargetOwnerCommandDef GetTargetOwnerCommandDef(uint id) => _targetOwnerCommandDef.GetValueOrDefault(id);
    public static TargetByObjectTypeCommandDef GetTargetByObjectTypeCommandDef(uint id) => _targetByObjectTypeCommandDef.GetValueOrDefault(id);
    public static TargetHostilesCommandDef GetTargetHostilesCommandDef(uint id) => _targetHostilesCommandDef.GetValueOrDefault(id);
    public static TargetByCharacterStateCommandDef GetTargetByCharacterStateCommandDef(uint id) => _targetByCharacterStateCommandDef.GetValueOrDefault(id);
    public static InflictDamageCommandDef GetInflictDamageCommandDef(uint id) => _inflictDamageCommandDef.GetValueOrDefault(id);
    public static ForcePushCommandDef GetForcePushCommandDef(uint id) => _forcePushCommandDef.GetValueOrDefault(id);
    public static RequestBattleFrameListCommandDef GetRequestBattleFrameList(uint id) => _requestBattleFrameListCommandDef.GetValueOrDefault(id);
    public static ApplyImpulseCommandDef GetApplyImpulseCommandDef(uint id) => _applyImpulseCommandDef.GetValueOrDefault(id);
    public static DeployableCalldownCommandDef GetDeployableCalldownCommandDef(uint id) => _deployableCalldownCommandDef.GetValueOrDefault(id);
    public static VehicleCalldownCommandDef GetVehicleCalldownCommandDef(uint id) => _vehicleCalldownCommandDef.GetValueOrDefault(id);
    public static FireProjectileCommandDef GetFireProjectileCommandDef(uint id) => _fireProjectileCommandDef.GetValueOrDefault(id);
    public static ResourceNodeBeaconCalldownCommandDef GetResourceNodeBeaconCalldownCommandDef(uint id) => _resourceNodeBeaconCalldownCommandDef.GetValueOrDefault(id);
    public static AttemptToCalldownVehicleCommandDef GetAttemptToCalldownVehicleCommandDef(uint id) => _attemptToCalldownVehicleCommandDef.GetValueOrDefault(id);
    public static RegisterClientProximityCommandDef GetRegisterClientProximityCommandDef(uint id) => _rregisterClientProximityCommandDef.GetValueOrDefault(id);
    public static CombatFlagsCommandDef GetCombatFlagsCommandDef(uint id) => _combatFlagsCommandDef.GetValueOrDefault(id);
    public static ApplyFreezeCommandDef GetApplyFreezeCommandDef(uint id) => _applyFreezeCommandDef.GetValueOrDefault(id);
    public static OrientationLockCommandDef GetOrientationLockCommandDef(uint id) => _orientationLockCommandDef.GetValueOrDefault(id);
    public static StatModifierCommandDef GetStatModifierCommandDef(uint id) => _statModifierCommandDef.GetValueOrDefault(id);
    public static RequireAimModeCommandDef GetRequireAimModeCommandDef(uint id) => _requireAimModeCommandDef.GetValueOrDefault(id);
    public static RequireArmyCommandDef GetRequireArmyCommandDef(uint id) => _requireArmyCommandDef.GetValueOrDefault(id);
    public static RequireBackstabCommandDef GetRequireBackstabCommandDef(uint id) => _requireBackstabCommandDef.GetValueOrDefault(id);
    public static RequireBulletHitCommandDef GetRequireBulletHitCommandDef(uint id) => _rrequireBulletHitCommandDef.GetValueOrDefault(id);
    public static RequireCAISStateCommandDef GetRequireCAISStateCommandDef(uint id) => _requireCAISStateCommandDef.GetValueOrDefault(id);
    public static RequireCStateCommandDef GetRequireCStateCommandDef(uint id) => _requireCStateCommandDef.GetValueOrDefault(id);
    public static RequireDamageResponseCommandDef GetRequireDamageResponseCommandDef(uint id) => _requireDamageResponseCommandDef.GetValueOrDefault(id);
    public static RequireEliteLevelCommandDef GetRequireEliteLevelCommandDef(uint id) => _requireEliteLevelCommandDef.GetValueOrDefault(id);
    public static RequireEnergyByRangeCommandDef GetRequireEnergyByRangeCommandDef(uint id) => _requireEnergyByRangeCommandDef.GetValueOrDefault(id);
    public static RequireEnergyCommandDef GetRequireEnergyCommandDef(uint id) => _requireEnergyCommandDef.GetValueOrDefault(id);
    public static RequireEnergyFromTargetCommandDef GetRequireEnergyFromTargetCommandDef(uint id) => _requireEnergyFromTargetCommandDef.GetValueOrDefault(id);
    public static RequireEquippedItemCommandDef GetRequireEquippedItemCommandDef(uint id) => _requireEquippedItemCommandDef.GetValueOrDefault(id);
    public static RequireFriendsCommandDef GetRequireFriendsCommandDef(uint id) => _requireFriendsCommandDef.GetValueOrDefault(id);
    public static RequireHasCertificateCommandDef GetRequireHasCertificateCommandDef(uint id) => _requireHasCertificateCommandDef.GetValueOrDefault(id);
    public static RequireHasEffectCommandDef GetRequireHasEffectCommandDef(uint id) => _requireHasEffectCommandDef.GetValueOrDefault(id);
    public static RequireHasEffectTagCommandDef GetRequireHasEffectTagCommandDef(uint id) => _requireHasEffectTagCommandDef.GetValueOrDefault(id);
    public static RequireHasItemCommandDef GetRequireHasItemCommandDef(uint id) => _requireHasItemCommandDef.GetValueOrDefault(id);
    public static RequireHasUnlockCommandDef GetRequireHasUnlockCommandDef(uint id) => _requireHasUnlockCommandDef.GetValueOrDefault(id);
    public static RequireHeadshotCommandDef GetRequireHeadshotCommandDef(uint id) => _requireHeadshotCommandDef.GetValueOrDefault(id);
    public static RequireInCombatCommandDef GetRequireInCombatCommandDef(uint id) => _requireInCombatCommandDef.GetValueOrDefault(id);
    public static RequireInRangeCommandDef GetRequireInRangeCommandDef(uint id) => _requireInRangeCommandDef.GetValueOrDefault(id);
    public static RequireInVehicleCommandDef GetRequireInVehicleCommandDef(uint id) => _requireInVehicleCommandDef.GetValueOrDefault(id);
    public static RequireIsNPCCommandDef GetRequireIsNPCCommandDef(uint id) => _requireIsNPCCommandDef.GetValueOrDefault(id);
    public static RequireItemAttributeCommandDef GetRequireItemAttributeCommandDef(uint id) => _requireItemAttributeCommandDef.GetValueOrDefault(id);
    public static RequireItemDurabilityCommandDef GetRequireItemDurabilityCommandDef(uint id) => _requireItemDurabilityCommandDef.GetValueOrDefault(id);
    public static RequireJumpedCommandDef GetRequireJumpedCommandDef(uint id) => _requireJumpedCommandDef.GetValueOrDefault(id);
    public static RequireLevelCommandDef GetRequireLevelCommandDef(uint id) => _requireLevelCommandDef.GetValueOrDefault(id);
    public static RequireLineOfSightCommandDef GetRequireLineOfSightCommandDef(uint id) => _rrequireLineOfSightCommandDef.GetValueOrDefault(id);
    public static RequirementServerCommandDef GetRequirementServerCommandDef(uint id) => _requirementServerCommandDef.GetValueOrDefault(id);
    public static RequireMovementFlagsCommandDef GetRequireMovementFlagsCommandDef(uint id) => _requireMovementFlagsCommandDef.GetValueOrDefault(id);
    public static RequireMovestateCommandDef GetRequireMovestateCommandDef(uint id) => _requireMovestateCommandDef.GetValueOrDefault(id);
    public static RequireMovingCommandDef GetRequireMovingCommandDef(uint id) => _requireMovingCommandDef.GetValueOrDefault(id);
    public static RequireNeedsAmmoCommandDef GetRequireNeedsAmmoCommandDef(uint id) => _rrequireNeedsAmmoCommandDef.GetValueOrDefault(id);
    public static RequireNotRespawnedCommandDef GetRequireNotRespawnedCommandDef(uint id) => _requireNotRespawnedCommandDef.GetValueOrDefault(id);
    public static RequirePermissionCommandDef GetRequirePermissionCommandDef(uint id) => _requirePermissionCommandDef.GetValueOrDefault(id);
    public static RequireProjectileSlopeCommandDef GetRequireProjectileSlopeCommandDef(uint id) => _requireProjectileSlopeCommandDef.GetValueOrDefault(id);
    public static RequireReloadCommandDef GetRequireReloadCommandDef(uint id) => _requireReloadCommandDef.GetValueOrDefault(id);
    public static RequireResourceCommandDef GetRequireResourceCommandDef(uint id) => _requireResourceCommandDef.GetValueOrDefault(id);
    public static RequireResourceFromTargetCommandDef GetRequireResourceFromTargetCommandDef(uint id) => _requireResourceFromTargetCommandDef.GetValueOrDefault(id);
    public static RequireSinAcquiredCommandDef GetRequireSinAcquiredCommandDef(uint id) => _requireSinAcquiredCommandDef.GetValueOrDefault(id);
    public static RequireSprintModifierCommandDef GetRequireSprintModifierCommandDef(uint id) => _requireSprintModifierCommandDef.GetValueOrDefault(id);
    public static RequireSquadLeaderCommandDef GetRequireSquadLeaderCommandDef(uint id) => _requireSquadLeaderCommandDef.GetValueOrDefault(id);
    public static RequireSuperChargeCommandDef GetRequireSuperChargeCommandDef(uint id) => _requireSuperChargeCommandDef.GetValueOrDefault(id);
    public static RequireTookDamageCommandDef GetRequireTookDamageCommandDef(uint id) => _requireTookDamageCommandDef.GetValueOrDefault(id);
    public static RequireWeaponArmedCommandDef GetRequireWeaponArmedCommandDef(uint id) => _requireWeaponArmedCommandDef.GetValueOrDefault(id);
    public static RequireWeaponTemplateCommandDef GetRequireWeaponTemplateCommandDef(uint id) => _requireWeaponTemplateCommandDef.GetValueOrDefault(id);
    public static RequireZoneTypeCommandDef GetRequireZoneTypeCommandDef(uint id) => _requireZoneTypeCommandDef.GetValueOrDefault(id);
    public static InteractionTypeCommandDef GetInteractionTypeCommandDef(uint id) => _interactionTypeCommandDef.GetValueOrDefault(id);
    public static TargetInteractivesCommandDef GetTargetInteractivesCommandDef(uint id) => _targetInteractivesCommandDef.GetValueOrDefault(id);
    public static ImpactMarkInteractivesCommandDef GetImpactMarkInteractivesCommandDef(uint id) => _impactMarkInteractivesCommandDef.GetValueOrDefault(id);
    public static HasTargetsDurationCommandDef GetHasTargetsDurationCommandDef(uint id) => _hhasTargetsDurationCommandDef.GetValueOrDefault(id);
    public static RopePullCommandDef GetRopePullCommandDef(uint id) => _ropePullCommandDef.GetValueOrDefault(id);
    public static SetTargetOffsetCommandDef GetSetTargetOffsetCommandDef(uint id) => _setTargetOffsetCommandDef.GetValueOrDefault(id);
    public static HealDamageCommandDef GetHealDamageCommandDef(uint id) => _hhealDamageCommandDef.GetValueOrDefault(id);
    public static BullrushCommandDef GetBullrushCommandDef(uint id) => _bullrushCommandDef.GetValueOrDefault(id);
    public static EnergyToDamageCommandDef GetEnergyToDamageCommandDef(uint id) => _energyToDamageCommandDef.GetValueOrDefault(id);
    public static BattleFrameDurationCommandDef GetBattleFrameDurationCommandDef(uint id) => _battleFrameDurationCommandDef.GetValueOrDefault(id);
    public static ShootingDurationCommandDef GetShootingDurationCommandDef(uint id) => _shootingDurationCommandDef.GetValueOrDefault(id);
    public static SwitchWeaponCommandDef GetSwitchWeaponCommandDef(uint id) => _switchWeaponCommandDef.GetValueOrDefault(id);
    public static StatRequirementCommandDef GetStatRequirementCommandDef(uint id) => _statRequirementCommandDef.GetValueOrDefault(id);
    public static ConsumeEnergyCommandDef GetConsumeEnergyCommandDef(uint id) => _cconsumeEnergyCommandDef.GetValueOrDefault(id);
    public static TargetClassTypeCommandDef GetTargetClassTypeCommandDef(uint id) => _targetClassTypeCommandDef.GetValueOrDefault(id);
    public static ClimbLedgeCommandDef GetClimbLedgeCommandDef(uint id) => _climbLedgeCommandDef.GetValueOrDefault(id);
    public static CopyInitiationPositionCommandDef GetCopyInitiationPositionCommandDef(uint id) => _copyInitiationPositionCommandDef.GetValueOrDefault(id);
    public static SlotAmmoCommandDef GetSlotAmmoCommandDef(uint id) => _slotAmmoCommandDef.GetValueOrDefault(id);
    public static AddPhysicsCommandDef GetAddPhysicsCommandDef(uint id) => _addPhysicsCommandDef.GetValueOrDefault(id);
    public static TargetCurrentVehicleCommandDef GetTargetCurrentVehicleCommandDef(uint id) => _targetCurrentVehicleCommandDef.GetValueOrDefault(id);
    public static TargetPassengersCommandDef GetTargetPassengersCommandDef(uint id) => _targetPassengersCommandDef.GetValueOrDefault(id);
    public static TargetSquadmatesCommandDef GetTargetSquadmatesCommandDef(uint id) => _targetSquadmatesCommandDef.GetValueOrDefault(id);
    public static SetWeaponDamageCommandDef GetSetWeaponDamageCommandDef(uint id) => _setWeaponDamageCommandDef.GetValueOrDefault(id);
    public static ConsumeEnergyOverTimeCommandDef GetConsumeEnergyOverTimeCommandDef(uint id) => _consumeEnergyOverTimeCommandDef.GetValueOrDefault(id);
    public static RequestAbilitySelectionCommandDef GetRequestAbilitySelectionCommandDef(uint id) => _requestAbilitySelectionCommandDef.GetValueOrDefault(id);
    public static BombardmentCommandDef GetBombardmentCommandDef(uint id) => _bombardmentCommandDef.GetValueOrDefault(id);
    public static SetProjectileTargetCommandDef GetSetProjectileTargetCommandDef(uint id) => _setProjectileTargetCommandDef.GetValueOrDefault(id);
    public static MovementSlideCommandDef GetMovementSlideCommandDef(uint id) => _movementSlideCommandDef.GetValueOrDefault(id);
    public static TargetFromStatusEffectCommandDef GetTargetFromStatusEffectCommandDef(uint id) => _targetFromStatusEffectCommandDef.GetValueOrDefault(id);
    public static TargetByDamageResponseCommandDef GetTargetByDamageResponseCommandDef(uint id) => _targetByDamageResponseCommandDef.GetValueOrDefault(id);
    public static ForcedMovementDurationCommandDef GetForcedMovementDurationCommandDef(uint id) => _fforcedMovementDurationCommandDef.GetValueOrDefault(id);
    public static FireUiEventCommandDef GetFireUiEventCommandDef(uint id) => _ffireUiEventCommandDef.GetValueOrDefault(id);
    public static UiNamedVariableCommandDef GetUiNamedVariableCommandDef(uint id) => _uiNamedVariableCommandDef.GetValueOrDefault(id);
    public static DetonateProjectilesCommandDef GetDetonateProjectilesCommandDef(uint id) => _ddetonateProjectilesCommandDef.GetValueOrDefault(id);
    public static SetWeaponDamageTypeCommandDef GetSetWeaponDamageTypeCommandDef(uint id) => _setWeaponDamageTypeCommandDef.GetValueOrDefault(id);
    public static TargetFilterMovestateCommandDef GetTargetFilterMovestateCommandDef(uint id) => _targetFilterMovestateCommandDef.GetValueOrDefault(id);
    public static TargetByHostilityCommandDef GetTargetByHostilityCommandDef(uint id) => _ttargetByHostilityCommandDef.GetValueOrDefault(id);
    public static ConsumeSuperChargeCommandDef GetConsumeSuperChargeCommandDef(uint id) => _consumeSuperChargeCommandDef.GetValueOrDefault(id);
    public static TargetByHealthCommandDef GetTargetByHealthCommandDef(uint id) => _targetByHealthCommandDef.GetValueOrDefault(id);
    public static RegisterMovementEffectCommandDef GetRegisterMovementEffectCommandDef(uint id) => _registerMovementEffectCommandDef.GetValueOrDefault(id);
    public static ApplyAmmoRiderCommandDef GetApplyAmmoRiderCommandDef(uint id) => _applyAmmoRiderCommandDef.GetValueOrDefault(id);
    public static TargetFilterByRangeCommandDef GetTargetFilterByRangeCommandDef(uint id) => _targetFilterByRangeCommandDef.GetValueOrDefault(id);
    public static OverrideCollisionCommandDef GetOverrideCollisionCommandDef(uint id) => _ooverrideCollisionCommandDef.GetValueOrDefault(id);
    public static MovementFacingCommandDef GetMovementFacingCommandDef(uint id) => _movementFacingCommandDef.GetValueOrDefault(id);
    public static TargetFilterBySinAcquiredCommandDef GetTargetFilterBySinAcquiredCommandDef(uint id) => _targetFilterBySinAcquiredCommandDef.GetValueOrDefault(id);
    public static MovementTetherCommandDef GetMovementTetherCommandDef(uint id) => _movementTetherCommandDef.GetValueOrDefault(id);
    public static RegisterLoadFromWeaponCommandDef GetRegisterLoadFromWeaponCommandDef(uint id) => _registerLoadFromWeaponCommandDef.GetValueOrDefault(id);
    public static ApplyClientStatusEffectCommandDef GetApplyClientStatusEffectCommandDef(uint id) => _applyClientStatusEffectCommandDef.GetValueOrDefault(id);
    public static RemoveClientStatusEffectCommandDef GetRemoveClientStatusEffectCommandDef(uint id) => _removeClientStatusEffectCommandDef.GetValueOrDefault(id);
    public static DisableChatBubbleCommandDef GetDisableChatBubbleCommandDef(uint id) => _disableChatBubbleCommandDef.GetValueOrDefault(id);
    public static DisableHealthAndIconCommandDef GetDisableHealthAndIconCommandDef(uint id) => _disableHealthAndIconCommandDef.GetValueOrDefault(id);

    // vcs
    public static VehicleClass GetVehicleClass(byte id) => _vehicleClass.GetValueOrDefault(id);
    public static VehicleInfo GetVehicleInfo(ushort id) => _vehicleInfo.GetValueOrDefault(id);
    public static Dictionary<uint, BaseComponentDef> GetBaseComponentDef(ushort id) => _baseComponentDef.GetValueOrDefault(id);
    public static ScopingComponentDef GetScopingComponentDef(uint id) => _scopingComponentDef.GetValueOrDefault(id);
    public static DriverComponentDef GetDriverComponentDef(uint id) => _driverComponentDef.GetValueOrDefault(id);
    public static PassengerComponentDef GetPassengerComponentDef(uint id) => _passengerComponentDef.GetValueOrDefault(id);
    public static AbilityComponentDef GetAbilityComponentDef(uint id) => _abilityComponentDef.GetValueOrDefault(id);
    public static DamageComponentDef GetDamageComponentDef(uint id) => _damageComponentDef.GetValueOrDefault(id);
    public static StatusEffectComponentDef GetStatusEffectComponentDef(uint id) => _statusEffectComponentDef.GetValueOrDefault(id);
    public static TurretComponentDef GetTurretComponentDef(uint id) => _turretComponentDef.GetValueOrDefault(id);
    public static DeployableComponentDef GetDeployableComponentDef(uint id) => _deployableComponentDef.GetValueOrDefault(id);
    public static SpawnPointComponentDef GetSpawnPointComponentDef(uint id) => _spawnPointComponentDef.GetValueOrDefault(id);
    public static HullSegmentDef GetHullSegmentComponentDef(uint id) => _hullSegmentDef.GetValueOrDefault(id);
}