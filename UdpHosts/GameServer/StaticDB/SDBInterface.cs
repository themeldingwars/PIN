namespace GameServer.Data.SDB;

using System.Collections.Generic;
using FauFau.Formats;
using Records.dbitems;
using Records.dbviusalrecords;
using Records.apt;
using Records.aptfs;
using Records.dbcharacter;
using Records;
using System.Linq;

public class SDBInterface
{
    // dbcharacter
    private static Dictionary<uint, CharCreateLoadout> CharCreateLoadout;
    private static Dictionary<uint, Dictionary<byte, CharCreateLoadoutSlots>> CharCreateLoadoutSlots;
    private static Dictionary<uint, Deployable> Deployable;

    // dbvisualrecords
    private static Dictionary<uint, WarpaintPalette> WarpaintPalettes;

    // dbitems
    private static Dictionary<uint, AttributeCategory> AttributeCategory;
    private static Dictionary<uint, AttributeDefinition> AttributeDefinition;
    private static Dictionary<KeyValuePair<uint, ushort>, AttributeRange> AttributeRange;
    private static Dictionary<KeyValuePair<uint, ushort>, ItemCharacterScalars> ItemCharacterScalars;
    private static Dictionary<uint, RootItem> RootItem;
    private static Dictionary<uint, AbilityModule> AbilityModule;
    private static Dictionary<uint, Battleframe> Battleframe;

    // apt
    private static Dictionary<uint, BaseCommandDef> BaseCommandDef;
    private static Dictionary<uint, CommandType> CommandType;
    private static Dictionary<uint, StatusEffectData> StatusEffectData;
    private static Dictionary<uint, AbilityData> AbilityData;
    private static Dictionary<uint, ImpactApplyEffectCommandDef> ImpactApplyEffectCommandDef;
    private static Dictionary<uint, ConditionalBranchCommandDef> ConditionalBranchCommandDef;
    private static Dictionary<uint, WhileLoopCommandDef> WhileLoopCommandDef;
    private static Dictionary<uint, LogicNegateCommandDef> LogicNegateCommandDef;
    private static Dictionary<uint, LogicOrCommandDef> LogicOrCommandDef;
    private static Dictionary<uint, LogicOrChainCommandDef> LogicOrChainCommandDef;
    private static Dictionary<uint, LogicAndChainCommandDef> LogicAndChainCommandDef;
    private static Dictionary<uint, CallCommandDef> CallCommandDef;
    private static Dictionary<uint, InstantActivationCommandDef> InstantActivationCommandDef;
    private static Dictionary<uint, TargetPBAECommandDef> TargetPBAECommandDef;
    private static Dictionary<uint, TargetConeAECommandDef> TargetConeAECommandDef;
    private static Dictionary<uint, TargetClearCommandDef> TargetClearCommandDef;
    private static Dictionary<uint, TargetSelfCommandDef> TargetSelfCommandDef;
    private static Dictionary<uint, TargetInitiatorCommandDef> TargetInitiatorCommandDef;
    private static Dictionary<uint, TimeDurationCommandDef> TimeDurationCommandDef;
    private static Dictionary<uint, AirborneDurationCommandDef> AirborneDurationCommandDef;
    private static Dictionary<uint, ActivationDurationCommandDef> ActivationDurationCommandDef;
    private static Dictionary<uint, ReturnCommandDef> ReturnCommandDef;
    private static Dictionary<uint, LoadRegisterFromItemStatCommandDef> LoadRegisterFromItemStatCommandDef;
    private static Dictionary<uint, LoadRegisterFromBonusCommandDef> LoadRegisterFromBonusCommandDef;
    private static Dictionary<uint, LoadRegisterFromDamageCommandDef> LoadRegisterFromDamageCommandDef;
    private static Dictionary<uint, LoadRegisterFromLevelCommandDef> LoadRegisterFromLevelCommandDef;
    private static Dictionary<uint, LoadRegisterFromModulePowerCommandDef> LoadRegisterFromModulePowerCommandDef;
    private static Dictionary<uint, LoadRegisterFromNamedVarCommandDef> LoadRegisterFromNamedVarCommandDef;
    private static Dictionary<uint, LoadRegisterFromResourceCommandDef> LoadRegisterFromResourceCommandDef;
    private static Dictionary<uint, LoadRegisterFromStatCommandDef> LoadRegisterFromStatCommandDef;
    private static Dictionary<uint, RegisterComparisonCommandDef> RegisterComparisonCommandDef;
    private static Dictionary<uint, RegisterRandomCommandDef> RegisterRandomCommandDef;
    private static Dictionary<uint, SetRegisterCommandDef> SetRegisterCommandDef;
    private static Dictionary<uint, NamedVariableAssignCommandDef> NamedVariableAssignCommandDef;
    private static Dictionary<uint, InflictCooldownCommandDef> InflictCooldownCommandDef;
    private static Dictionary<uint, RequireDamageTypeCommandDef> RequireDamageTypeCommandDef;

    // aptfs
    private static Dictionary<uint, TargetFriendliesCommandDef> TargetFriendliesCommandDef;
    private static Dictionary<uint, TargetByEffectCommandDef> TargetByEffectCommandDef;
    private static Dictionary<uint, TargetOwnerCommandDef> TargetOwnerCommandDef;
    private static Dictionary<uint, TargetByObjectTypeCommandDef> TargetByObjectTypeCommandDef;
    private static Dictionary<uint, TargetHostilesCommandDef> TargetHostilesCommandDef;
    private static Dictionary<uint, TargetByCharacterStateCommandDef> TargetByCharacterStateCommandDef;
    private static Dictionary<uint, InflictDamageCommandDef> InflictDamageCommandDef;
    private static Dictionary<uint, ForcePushCommandDef> ForcePushCommandDef;
    private static Dictionary<uint, ApplyImpulseCommandDef> ApplyImpulseCommandDef;
    private static Dictionary<uint, DeployableCalldownCommandDef> DeployableCalldownCommandDef;
    private static Dictionary<uint, FireProjectileCommandDef> FireProjectileCommandDef;
    private static Dictionary<uint, ResourceNodeBeaconCalldownCommandDef> ResourceNodeBeaconCalldownCommandDef;
    private static Dictionary<uint, RegisterClientProximityCommandDef> RegisterClientProximityCommandDef;
    private static Dictionary<uint, CombatFlagsCommandDef> CombatFlagsCommandDef;
    private static Dictionary<uint, ApplyFreezeCommandDef> ApplyFreezeCommandDef;
    private static Dictionary<uint, OrientationLockCommandDef> OrientationLockCommandDef;
    private static Dictionary<uint, StatModifierCommandDef> StatModifierCommandDef;
    private static Dictionary<uint, RequireAimModeCommandDef> RequireAimModeCommandDef;
    private static Dictionary<uint, RequireArmyCommandDef> RequireArmyCommandDef;
    private static Dictionary<uint, RequireBackstabCommandDef> RequireBackstabCommandDef;
    private static Dictionary<uint, RequireBulletHitCommandDef> RequireBulletHitCommandDef;
    private static Dictionary<uint, RequireCAISStateCommandDef> RequireCAISStateCommandDef;
    private static Dictionary<uint, RequireCStateCommandDef> RequireCStateCommandDef;
    private static Dictionary<uint, RequireDamageResponseCommandDef> RequireDamageResponseCommandDef;
    private static Dictionary<uint, RequireEliteLevelCommandDef> RequireEliteLevelCommandDef;
    private static Dictionary<uint, RequireEnergyByRangeCommandDef> RequireEnergyByRangeCommandDef;
    private static Dictionary<uint, RequireEnergyCommandDef> RequireEnergyCommandDef;
    private static Dictionary<uint, RequireEnergyFromTargetCommandDef> RequireEnergyFromTargetCommandDef;
    private static Dictionary<uint, RequireEquippedItemCommandDef> RequireEquippedItemCommandDef;
    private static Dictionary<uint, RequireFriendsCommandDef> RequireFriendsCommandDef;
    private static Dictionary<uint, RequireHasCertificateCommandDef> RequireHasCertificateCommandDef;
    private static Dictionary<uint, RequireHasEffectCommandDef> RequireHasEffectCommandDef;
    private static Dictionary<uint, RequireHasEffectTagCommandDef> RequireHasEffectTagCommandDef;
    private static Dictionary<uint, RequireHasItemCommandDef> RequireHasItemCommandDef;
    private static Dictionary<uint, RequireHasUnlockCommandDef> RequireHasUnlockCommandDef;
    private static Dictionary<uint, RequireHeadshotCommandDef> RequireHeadshotCommandDef;
    private static Dictionary<uint, RequireInCombatCommandDef> RequireInCombatCommandDef;
    private static Dictionary<uint, RequireInRangeCommandDef> RequireInRangeCommandDef;
    private static Dictionary<uint, RequireInVehicleCommandDef> RequireInVehicleCommandDef;
    private static Dictionary<uint, RequireIsNPCCommandDef> RequireIsNPCCommandDef;
    private static Dictionary<uint, RequireItemAttributeCommandDef> RequireItemAttributeCommandDef;
    private static Dictionary<uint, RequireItemDurabilityCommandDef> RequireItemDurabilityCommandDef;
    private static Dictionary<uint, RequireJumpedCommandDef> RequireJumpedCommandDef;
    private static Dictionary<uint, RequireLevelCommandDef> RequireLevelCommandDef;
    private static Dictionary<uint, RequireLineOfSightCommandDef> RequireLineOfSightCommandDef;
    private static Dictionary<uint, RequirementServerCommandDef> RequirementServerCommandDef;
    private static Dictionary<uint, RequireMovementFlagsCommandDef> RequireMovementFlagsCommandDef;
    private static Dictionary<uint, RequireMovestateCommandDef> RequireMovestateCommandDef;
    private static Dictionary<uint, RequireMovingCommandDef> RequireMovingCommandDef;
    private static Dictionary<uint, RequireNeedsAmmoCommandDef> RequireNeedsAmmoCommandDef;
    private static Dictionary<uint, RequireNotRespawnedCommandDef> RequireNotRespawnedCommandDef;
    private static Dictionary<uint, RequirePermissionCommandDef> RequirePermissionCommandDef;
    private static Dictionary<uint, RequireProjectileSlopeCommandDef> RequireProjectileSlopeCommandDef;
    private static Dictionary<uint, RequireReloadCommandDef> RequireReloadCommandDef;
    private static Dictionary<uint, RequireResourceCommandDef> RequireResourceCommandDef;
    private static Dictionary<uint, RequireResourceFromTargetCommandDef> RequireResourceFromTargetCommandDef;
    private static Dictionary<uint, RequireSinAcquiredCommandDef> RequireSinAcquiredCommandDef;
    private static Dictionary<uint, RequireSprintModifierCommandDef> RequireSprintModifierCommandDef;
    private static Dictionary<uint, RequireSquadLeaderCommandDef> RequireSquadLeaderCommandDef;
    private static Dictionary<uint, RequireSuperChargeCommandDef> RequireSuperChargeCommandDef;
    private static Dictionary<uint, RequireTookDamageCommandDef> RequireTookDamageCommandDef;
    private static Dictionary<uint, RequireWeaponArmedCommandDef> RequireWeaponArmedCommandDef;
    private static Dictionary<uint, RequireWeaponTemplateCommandDef> RequireWeaponTemplateCommandDef;
    private static Dictionary<uint, RequireZoneTypeCommandDef> RequireZoneTypeCommandDef;
    private static Dictionary<uint, InteractionTypeCommandDef> InteractionTypeCommandDef;

    public static void Init(StaticDB instance)
    {
        var loader = new StaticDBLoader(instance);

        // dbcharacter
        CharCreateLoadout = loader.LoadCharCreateLoadout();
        CharCreateLoadoutSlots = loader.LoadCharCreateLoadoutSlots();
        Deployable = loader.LoadDeployable();

        // dbvisualrecords
        WarpaintPalettes = loader.LoadWarpaintPalettes();

        // dbitems
        AttributeCategory = loader.LoadAttributeCategory();
        AttributeDefinition = loader.LoadAttributeDefinition();
        AttributeRange = loader.LoadAttributeRange();
        ItemCharacterScalars = loader.LoadItemCharacterScalars();
        RootItem = loader.LoadRootItem();
        AbilityModule = loader.LoadAbilityModule();
        Battleframe = loader.LoadBattleframe();

        // apt
        StatusEffectData = loader.LoadStatusEffectData();
        BaseCommandDef = loader.LoadBaseCommandDef();
        CommandType = loader.LoadCommandType();
        AbilityData = loader.LoadAbilityData();
        ImpactApplyEffectCommandDef = loader.LoadImpactApplyEffectCommandDef();
        WhileLoopCommandDef = loader.LoadWhileLoopCommandDef();
        LogicNegateCommandDef = loader.LoadLogicNegateCommandDef();
        LogicOrCommandDef = loader.LoadLogicOrCommandDef();
        LogicOrChainCommandDef = loader.LoadLogicOrChainCommandDef();
        LogicAndChainCommandDef = loader.LoadLogicAndChainCommandDef();
        CallCommandDef = loader.LoadCallCommandDef();
        InstantActivationCommandDef = loader.LoadInstantActivationCommandDef();
        ConditionalBranchCommandDef = loader.LoadConditionalBranchCommandDef();
        TargetPBAECommandDef = loader.LoadTargetPBAECommandDef();
        TargetConeAECommandDef = loader.LoadTargetConeAECommandDef();
        TargetClearCommandDef = loader.LoadTargetClearCommandDef();
        TargetSelfCommandDef = loader.LoadTargetSelfCommandDef();
        TargetInitiatorCommandDef = loader.LoadTargetInitiatorCommandDef();
        TimeDurationCommandDef = loader.LoadTimeDurationCommandDef();
        ReturnCommandDef = loader.LoadReturnCommandDef();
        LoadRegisterFromItemStatCommandDef = loader.LoadLoadRegisterFromItemStatCommandDef();
        LoadRegisterFromBonusCommandDef = loader.LoadLoadRegisterFromBonusCommandDef();
        LoadRegisterFromDamageCommandDef = loader.LoadLoadRegisterFromDamageCommandDef();
        LoadRegisterFromLevelCommandDef = loader.LoadLoadRegisterFromLevelCommandDef();
        LoadRegisterFromModulePowerCommandDef = loader.LoadLoadRegisterFromModulePowerCommandDef();
        LoadRegisterFromNamedVarCommandDef = loader.LoadLoadRegisterFromNamedVarCommandDef();
        LoadRegisterFromResourceCommandDef = loader.LoadLoadRegisterFromResourceCommandDef();
        LoadRegisterFromStatCommandDef = loader.LoadLoadRegisterFromStatCommandDef();
        RegisterComparisonCommandDef = loader.LoadRegisterComparisonCommandDef();
        RegisterRandomCommandDef = loader.LoadRegisterRandomCommandDef();
        SetRegisterCommandDef = loader.LoadSetRegisterCommandDef();
        NamedVariableAssignCommandDef = loader.LoadNamedVariableAssignCommandDef();
        InflictCooldownCommandDef = loader.LoadInflictCooldownCommandDef();
        RequireDamageTypeCommandDef = loader.LoadRequireDamageTypeCommandDef();

        // aptfs
        TargetFriendliesCommandDef = loader.LoadTargetFriendliesCommandDef();
        TargetByEffectCommandDef = loader.LoadTargetByEffectCommandDef();
        TargetOwnerCommandDef = loader.LoadTargetOwnerCommandDef();
        TargetByObjectTypeCommandDef = loader.LoadTargetByObjectTypeCommandDef();
        TargetHostilesCommandDef = loader.LoadTargetHostilesCommandDef();
        TargetByCharacterStateCommandDef = loader.LoadTargetByCharacterStateCommandDef();
        InflictDamageCommandDef = loader.LoadInflictDamageCommandDef();
        ForcePushCommandDef = loader.LoadForcePushCommandDef();
        ApplyImpulseCommandDef = loader.LoadApplyImpulseCommandDef();
        DeployableCalldownCommandDef = loader.LoadDeployableCalldownCommandDef();
        FireProjectileCommandDef = loader.LoadFireProjectileCommandDef();
        ResourceNodeBeaconCalldownCommandDef = loader.LoadResourceNodeBeaconCalldownCommandDef();
        RegisterClientProximityCommandDef = loader.LoadRegisterClientProximityCommandDef();
        CombatFlagsCommandDef = loader.LoadCombatFlagsCommandDef();
        ApplyFreezeCommandDef = loader.LoadApplyFreezeCommandDef();
        OrientationLockCommandDef = loader.LoadOrientationLockCommandDef();
        StatModifierCommandDef = loader.LoadStatModifierCommandDef();
        RequireAimModeCommandDef = loader.LoadRequireAimModeCommandDef();
        RequireArmyCommandDef = loader.LoadRequireArmyCommandDef();
        RequireBackstabCommandDef = loader.LoadRequireBackstabCommandDef();
        RequireBulletHitCommandDef = loader.LoadRequireBulletHitCommandDef();
        RequireCAISStateCommandDef = loader.LoadRequireCAISStateCommandDef();
        RequireCStateCommandDef = loader.LoadRequireCStateCommandDef();
        RequireDamageResponseCommandDef = loader.LoadRequireDamageResponseCommandDef();
        RequireEliteLevelCommandDef = loader.LoadRequireEliteLevelCommandDef();
        RequireEnergyByRangeCommandDef = loader.LoadRequireEnergyByRangeCommandDef();
        RequireEnergyCommandDef = loader.LoadRequireEnergyCommandDef();
        RequireEnergyFromTargetCommandDef = loader.LoadRequireEnergyFromTargetCommandDef();
        RequireEquippedItemCommandDef = loader.LoadRequireEquippedItemCommandDef();
        RequireFriendsCommandDef = loader.LoadRequireFriendsCommandDef();
        RequireHasCertificateCommandDef = loader.LoadRequireHasCertificateCommandDef();
        RequireHasEffectCommandDef = loader.LoadRequireHasEffectCommandDef();
        RequireHasEffectTagCommandDef = loader.LoadRequireHasEffectTagCommandDef();
        RequireHasItemCommandDef = loader.LoadRequireHasItemCommandDef();
        RequireHasUnlockCommandDef = loader.LoadRequireHasUnlockCommandDef();
        RequireHeadshotCommandDef = loader.LoadRequireHeadshotCommandDef();
        RequireInCombatCommandDef = loader.LoadRequireInCombatCommandDef();
        RequireInRangeCommandDef = loader.LoadRequireInRangeCommandDef();
        RequireInVehicleCommandDef = loader.LoadRequireInVehicleCommandDef();
        RequireIsNPCCommandDef = loader.LoadRequireIsNPCCommandDef();
        RequireItemAttributeCommandDef = loader.LoadRequireItemAttributeCommandDef();
        RequireItemDurabilityCommandDef = loader.LoadRequireItemDurabilityCommandDef();
        RequireJumpedCommandDef = loader.LoadRequireJumpedCommandDef();
        RequireLevelCommandDef = loader.LoadRequireLevelCommandDef();
        RequireLineOfSightCommandDef = loader.LoadRequireLineOfSightCommandDef();
        RequirementServerCommandDef = loader.LoadRequirementServerCommandDef();
        RequireMovementFlagsCommandDef = loader.LoadRequireMovementFlagsCommandDef();
        RequireMovestateCommandDef = loader.LoadRequireMovestateCommandDef();
        RequireMovingCommandDef = loader.LoadRequireMovingCommandDef();
        RequireNeedsAmmoCommandDef = loader.LoadRequireNeedsAmmoCommandDef();
        RequireNotRespawnedCommandDef = loader.LoadRequireNotRespawnedCommandDef();
        RequirePermissionCommandDef = loader.LoadRequirePermissionCommandDef();
        RequireProjectileSlopeCommandDef = loader.LoadRequireProjectileSlopeCommandDef();
        RequireReloadCommandDef = loader.LoadRequireReloadCommandDef();
        RequireResourceCommandDef = loader.LoadRequireResourceCommandDef();
        RequireResourceFromTargetCommandDef = loader.LoadRequireResourceFromTargetCommandDef();
        RequireSinAcquiredCommandDef = loader.LoadRequireSinAcquiredCommandDef();
        RequireSprintModifierCommandDef = loader.LoadRequireSprintModifierCommandDef();
        RequireSquadLeaderCommandDef = loader.LoadRequireSquadLeaderCommandDef();
        RequireSuperChargeCommandDef = loader.LoadRequireSuperChargeCommandDef();
        RequireTookDamageCommandDef = loader.LoadRequireTookDamageCommandDef();
        RequireWeaponArmedCommandDef = loader.LoadRequireWeaponArmedCommandDef();
        RequireWeaponTemplateCommandDef = loader.LoadRequireWeaponTemplateCommandDef();
        RequireZoneTypeCommandDef = loader.LoadRequireZoneTypeCommandDef();
        AirborneDurationCommandDef = loader.LoadAirborneDurationCommandDef();
        ActivationDurationCommandDef = loader.LoadActivationDurationCommandDef();
        InteractionTypeCommandDef = loader.LoadInteractionTypeCommandDef();
    }

    // dbcharacter
    public static CharCreateLoadout GetCharCreateLoadout(uint id) => CharCreateLoadout.GetValueOrDefault(id);
    public static CharCreateLoadout[] GetCharCreateLoadoutsByFrame(uint frameId) => CharCreateLoadout
    .Select(pair => pair.Value)
    .Where(value => value.FrameId == frameId)
    .ToArray();

    public static Dictionary<byte, CharCreateLoadoutSlots> GetCharCreateLoadoutSlots(uint id) => CharCreateLoadoutSlots.GetValueOrDefault(id);
    public static Deployable GetDeployable(uint id) => Deployable.GetValueOrDefault(id);

    // dbvisaulrecords
    public static WarpaintPalette GetWarpaintPalette(uint id) => WarpaintPalettes.GetValueOrDefault(id);

    // dbitems
    public static AbilityModule GetAbilityModule(uint id) => AbilityModule.GetValueOrDefault(id);
    public static Battleframe GetBattleframe(uint id) => Battleframe.GetValueOrDefault(id);

    // apt
    public static BaseCommandDef GetBaseCommandDef(uint id) => BaseCommandDef.GetValueOrDefault(id);
    public static CommandType GetCommandType(uint id) => CommandType.GetValueOrDefault(id);
    public static AbilityData GetAbilityData(uint id) => AbilityData.GetValueOrDefault(id);
    public static StatusEffectData GetStatusEffectData(uint id) => StatusEffectData.GetValueOrDefault(id);
    public static ImpactApplyEffectCommandDef GetImpactApplyEffectCommandDef(uint id) => ImpactApplyEffectCommandDef.GetValueOrDefault(id);
    public static ConditionalBranchCommandDef GetConditionalBranchCommandDef(uint id) => ConditionalBranchCommandDef.GetValueOrDefault(id);
    public static WhileLoopCommandDef GetWhileLoopCommandDef(uint id) => WhileLoopCommandDef.GetValueOrDefault(id);
    public static LogicNegateCommandDef GetLogicNegateCommandDef(uint id) => LogicNegateCommandDef.GetValueOrDefault(id);
    public static LogicOrCommandDef GetLogicOrCommandDef(uint id) => LogicOrCommandDef.GetValueOrDefault(id);
    public static LogicOrChainCommandDef GetLogicOrChainCommandDef(uint id) => LogicOrChainCommandDef.GetValueOrDefault(id);
    public static LogicAndChainCommandDef GetLogicAndChainCommandDef(uint id) => LogicAndChainCommandDef.GetValueOrDefault(id);
    public static CallCommandDef GetCallCommandDef(uint id) => CallCommandDef.GetValueOrDefault(id);
    public static InstantActivationCommandDef GetInstantActivationCommandDef(uint id) => InstantActivationCommandDef.GetValueOrDefault(id);
    public static TargetPBAECommandDef GetTargetPBAECommandDef(uint id) => TargetPBAECommandDef.GetValueOrDefault(id);
    public static TargetConeAECommandDef GetTargetConeAECommandDef(uint id) => TargetConeAECommandDef.GetValueOrDefault(id);
    public static TargetClearCommandDef GetTargetClearCommandDef(uint id) => TargetClearCommandDef.GetValueOrDefault(id);
    public static TargetSelfCommandDef GetTargetSelfCommandDef(uint id) => TargetSelfCommandDef.GetValueOrDefault(id);
    public static TargetInitiatorCommandDef GetTargetInitiatorCommandDef(uint id) => TargetInitiatorCommandDef.GetValueOrDefault(id);
    public static TimeDurationCommandDef GetTimeDurationCommandDef(uint id) => TimeDurationCommandDef.GetValueOrDefault(id);
    public static AirborneDurationCommandDef GetAirborneDurationCommandDef(uint id) => AirborneDurationCommandDef.GetValueOrDefault(id);
    public static ActivationDurationCommandDef GetActivationDurationCommandDef(uint id) => ActivationDurationCommandDef.GetValueOrDefault(id);
    public static ReturnCommandDef GetReturnCommandDef(uint id) => ReturnCommandDef.GetValueOrDefault(id);
    public static LoadRegisterFromItemStatCommandDef GetLoadRegisterFromItemStatCommandDef(uint id) => LoadRegisterFromItemStatCommandDef.GetValueOrDefault(id);
    public static LoadRegisterFromBonusCommandDef GetLoadRegisterFromBonusCommandDef(uint id) => LoadRegisterFromBonusCommandDef.GetValueOrDefault(id);
    public static LoadRegisterFromDamageCommandDef GetLoadRegisterFromDamageCommandDef(uint id) => LoadRegisterFromDamageCommandDef.GetValueOrDefault(id);
    public static LoadRegisterFromLevelCommandDef GetLoadRegisterFromLevelCommandDef(uint id) => LoadRegisterFromLevelCommandDef.GetValueOrDefault(id);
    public static LoadRegisterFromModulePowerCommandDef GetLoadRegisterFromModulePowerCommandDef(uint id) => LoadRegisterFromModulePowerCommandDef.GetValueOrDefault(id);
    public static LoadRegisterFromNamedVarCommandDef GetLoadRegisterFromNamedVarCommandDef(uint id) => LoadRegisterFromNamedVarCommandDef.GetValueOrDefault(id);
    public static LoadRegisterFromResourceCommandDef GetLoadRegisterFromResourceCommandDef(uint id) => LoadRegisterFromResourceCommandDef.GetValueOrDefault(id);
    public static LoadRegisterFromStatCommandDef GetLoadRegisterFromStatCommandDef(uint id) => LoadRegisterFromStatCommandDef.GetValueOrDefault(id);
    public static RegisterComparisonCommandDef GetRegisterComparisonCommandDef(uint id) => RegisterComparisonCommandDef.GetValueOrDefault(id);
    public static RegisterRandomCommandDef GetRegisterRandomCommandDef(uint id) => RegisterRandomCommandDef.GetValueOrDefault(id);
    public static SetRegisterCommandDef GetSetRegisterCommandDef(uint id) => SetRegisterCommandDef.GetValueOrDefault(id);
    public static NamedVariableAssignCommandDef GetNamedVariableAssignCommandDef(uint id) => NamedVariableAssignCommandDef.GetValueOrDefault(id);
    public static InflictCooldownCommandDef GetInflictCooldownCommandDef(uint id) => InflictCooldownCommandDef.GetValueOrDefault(id);
    public static RequireDamageTypeCommandDef GetRequireDamageTypeCommandDef(uint id) => RequireDamageTypeCommandDef.GetValueOrDefault(id);

    // aptfs
    public static TargetFriendliesCommandDef GetTargetFriendliesCommandDef(uint id) => TargetFriendliesCommandDef.GetValueOrDefault(id);
    public static TargetByEffectCommandDef GetTargetByEffectCommandDef(uint id) => TargetByEffectCommandDef.GetValueOrDefault(id);
    public static TargetOwnerCommandDef GetTargetOwnerCommandDef(uint id) => TargetOwnerCommandDef.GetValueOrDefault(id);
    public static TargetByObjectTypeCommandDef GetTargetByObjectTypeCommandDef(uint id) => TargetByObjectTypeCommandDef.GetValueOrDefault(id);
    public static TargetHostilesCommandDef GetTargetHostilesCommandDef(uint id) => TargetHostilesCommandDef.GetValueOrDefault(id);
    public static TargetByCharacterStateCommandDef GetTargetByCharacterStateCommandDef(uint id) => TargetByCharacterStateCommandDef.GetValueOrDefault(id);
    public static InflictDamageCommandDef GetInflictDamageCommandDef(uint id) => InflictDamageCommandDef.GetValueOrDefault(id);
    public static ForcePushCommandDef GetForcePushCommandDef(uint id) => ForcePushCommandDef.GetValueOrDefault(id);
    public static ApplyImpulseCommandDef GetApplyImpulseCommandDef(uint id) => ApplyImpulseCommandDef.GetValueOrDefault(id);
    public static DeployableCalldownCommandDef GetDeployableCalldownCommandDef(uint id) => DeployableCalldownCommandDef.GetValueOrDefault(id);
    public static FireProjectileCommandDef GetFireProjectileCommandDef(uint id) => FireProjectileCommandDef.GetValueOrDefault(id);
    public static ResourceNodeBeaconCalldownCommandDef GetResourceNodeBeaconCalldownCommandDef(uint id) => ResourceNodeBeaconCalldownCommandDef.GetValueOrDefault(id);
    public static RegisterClientProximityCommandDef GetRegisterClientProximityCommandDef(uint id) => RegisterClientProximityCommandDef.GetValueOrDefault(id);
    public static CombatFlagsCommandDef GetCombatFlagsCommandDef(uint id) => CombatFlagsCommandDef.GetValueOrDefault(id);
    public static ApplyFreezeCommandDef GetApplyFreezeCommandDef(uint id) => ApplyFreezeCommandDef.GetValueOrDefault(id);
    public static OrientationLockCommandDef GetOrientationLockCommandDef(uint id) => OrientationLockCommandDef.GetValueOrDefault(id);
    public static StatModifierCommandDef GetStatModifierCommandDef(uint id) => StatModifierCommandDef.GetValueOrDefault(id);
    public static RequireAimModeCommandDef GetRequireAimModeCommandDef(uint id) => RequireAimModeCommandDef.GetValueOrDefault(id);
    public static RequireArmyCommandDef GetRequireArmyCommandDef(uint id) => RequireArmyCommandDef.GetValueOrDefault(id);
    public static RequireBackstabCommandDef GetRequireBackstabCommandDef(uint id) => RequireBackstabCommandDef.GetValueOrDefault(id);
    public static RequireBulletHitCommandDef GetRequireBulletHitCommandDef(uint id) => RequireBulletHitCommandDef.GetValueOrDefault(id);
    public static RequireCAISStateCommandDef GetRequireCAISStateCommandDef(uint id) => RequireCAISStateCommandDef.GetValueOrDefault(id);
    public static RequireCStateCommandDef GetRequireCStateCommandDef(uint id) => RequireCStateCommandDef.GetValueOrDefault(id);
    public static RequireDamageResponseCommandDef GetRequireDamageResponseCommandDef(uint id) => RequireDamageResponseCommandDef.GetValueOrDefault(id);
    public static RequireEliteLevelCommandDef GetRequireEliteLevelCommandDef(uint id) => RequireEliteLevelCommandDef.GetValueOrDefault(id);
    public static RequireEnergyByRangeCommandDef GetRequireEnergyByRangeCommandDef(uint id) => RequireEnergyByRangeCommandDef.GetValueOrDefault(id);
    public static RequireEnergyCommandDef GetRequireEnergyCommandDef(uint id) => RequireEnergyCommandDef.GetValueOrDefault(id);
    public static RequireEnergyFromTargetCommandDef GetRequireEnergyFromTargetCommandDef(uint id) => RequireEnergyFromTargetCommandDef.GetValueOrDefault(id);
    public static RequireEquippedItemCommandDef GetRequireEquippedItemCommandDef(uint id) => RequireEquippedItemCommandDef.GetValueOrDefault(id);
    public static RequireFriendsCommandDef GetRequireFriendsCommandDef(uint id) => RequireFriendsCommandDef.GetValueOrDefault(id);
    public static RequireHasCertificateCommandDef GetRequireHasCertificateCommandDef(uint id) => RequireHasCertificateCommandDef.GetValueOrDefault(id);
    public static RequireHasEffectCommandDef GetRequireHasEffectCommandDef(uint id) => RequireHasEffectCommandDef.GetValueOrDefault(id);
    public static RequireHasEffectTagCommandDef GetRequireHasEffectTagCommandDef(uint id) => RequireHasEffectTagCommandDef.GetValueOrDefault(id);
    public static RequireHasItemCommandDef GetRequireHasItemCommandDef(uint id) => RequireHasItemCommandDef.GetValueOrDefault(id);
    public static RequireHasUnlockCommandDef GetRequireHasUnlockCommandDef(uint id) => RequireHasUnlockCommandDef.GetValueOrDefault(id);
    public static RequireHeadshotCommandDef GetRequireHeadshotCommandDef(uint id) => RequireHeadshotCommandDef.GetValueOrDefault(id);
    public static RequireInCombatCommandDef GetRequireInCombatCommandDef(uint id) => RequireInCombatCommandDef.GetValueOrDefault(id);
    public static RequireInRangeCommandDef GetRequireInRangeCommandDef(uint id) => RequireInRangeCommandDef.GetValueOrDefault(id);
    public static RequireInVehicleCommandDef GetRequireInVehicleCommandDef(uint id) => RequireInVehicleCommandDef.GetValueOrDefault(id);
    public static RequireIsNPCCommandDef GetRequireIsNPCCommandDef(uint id) => RequireIsNPCCommandDef.GetValueOrDefault(id);
    public static RequireItemAttributeCommandDef GetRequireItemAttributeCommandDef(uint id) => RequireItemAttributeCommandDef.GetValueOrDefault(id);
    public static RequireItemDurabilityCommandDef GetRequireItemDurabilityCommandDef(uint id) => RequireItemDurabilityCommandDef.GetValueOrDefault(id);
    public static RequireJumpedCommandDef GetRequireJumpedCommandDef(uint id) => RequireJumpedCommandDef.GetValueOrDefault(id);
    public static RequireLevelCommandDef GetRequireLevelCommandDef(uint id) => RequireLevelCommandDef.GetValueOrDefault(id);
    public static RequireLineOfSightCommandDef GetRequireLineOfSightCommandDef(uint id) => RequireLineOfSightCommandDef.GetValueOrDefault(id);
    public static RequirementServerCommandDef GetRequirementServerCommandDef(uint id) => RequirementServerCommandDef.GetValueOrDefault(id);
    public static RequireMovementFlagsCommandDef GetRequireMovementFlagsCommandDef(uint id) => RequireMovementFlagsCommandDef.GetValueOrDefault(id);
    public static RequireMovestateCommandDef GetRequireMovestateCommandDef(uint id) => RequireMovestateCommandDef.GetValueOrDefault(id);
    public static RequireMovingCommandDef GetRequireMovingCommandDef(uint id) => RequireMovingCommandDef.GetValueOrDefault(id);
    public static RequireNeedsAmmoCommandDef GetRequireNeedsAmmoCommandDef(uint id) => RequireNeedsAmmoCommandDef.GetValueOrDefault(id);
    public static RequireNotRespawnedCommandDef GetRequireNotRespawnedCommandDef(uint id) => RequireNotRespawnedCommandDef.GetValueOrDefault(id);
    public static RequirePermissionCommandDef GetRequirePermissionCommandDef(uint id) => RequirePermissionCommandDef.GetValueOrDefault(id);
    public static RequireProjectileSlopeCommandDef GetRequireProjectileSlopeCommandDef(uint id) => RequireProjectileSlopeCommandDef.GetValueOrDefault(id);
    public static RequireReloadCommandDef GetRequireReloadCommandDef(uint id) => RequireReloadCommandDef.GetValueOrDefault(id);
    public static RequireResourceCommandDef GetRequireResourceCommandDef(uint id) => RequireResourceCommandDef.GetValueOrDefault(id);
    public static RequireResourceFromTargetCommandDef GetRequireResourceFromTargetCommandDef(uint id) => RequireResourceFromTargetCommandDef.GetValueOrDefault(id);
    public static RequireSinAcquiredCommandDef GetRequireSinAcquiredCommandDef(uint id) => RequireSinAcquiredCommandDef.GetValueOrDefault(id);
    public static RequireSprintModifierCommandDef GetRequireSprintModifierCommandDef(uint id) => RequireSprintModifierCommandDef.GetValueOrDefault(id);
    public static RequireSquadLeaderCommandDef GetRequireSquadLeaderCommandDef(uint id) => RequireSquadLeaderCommandDef.GetValueOrDefault(id);
    public static RequireSuperChargeCommandDef GetRequireSuperChargeCommandDef(uint id) => RequireSuperChargeCommandDef.GetValueOrDefault(id);
    public static RequireTookDamageCommandDef GetRequireTookDamageCommandDef(uint id) => RequireTookDamageCommandDef.GetValueOrDefault(id);
    public static RequireWeaponArmedCommandDef GetRequireWeaponArmedCommandDef(uint id) => RequireWeaponArmedCommandDef.GetValueOrDefault(id);
    public static RequireWeaponTemplateCommandDef GetRequireWeaponTemplateCommandDef(uint id) => RequireWeaponTemplateCommandDef.GetValueOrDefault(id);
    public static RequireZoneTypeCommandDef GetRequireZoneTypeCommandDef(uint id) => RequireZoneTypeCommandDef.GetValueOrDefault(id);
    public static InteractionTypeCommandDef GetInteractionTypeCommandDef(uint id) => InteractionTypeCommandDef.GetValueOrDefault(id);
}