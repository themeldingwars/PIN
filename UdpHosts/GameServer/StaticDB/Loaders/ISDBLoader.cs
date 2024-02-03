namespace GameServer.Data.SDB;

using Records.apt;
using Records.aptfs;
using Records.dbviusalrecords;
using Records.dbitems;
using System.Collections.Generic;
using Records.dbcharacter;

public interface ISDBLoader
{
    // dbcharacter
    Dictionary<uint, CharCreateLoadout> LoadCharCreateLoadout();
    Dictionary<uint, Dictionary<byte, CharCreateLoadoutSlots>> LoadCharCreateLoadoutSlots();
    Dictionary<uint, Deployable> LoadDeployable();

    // dbvisualrecords
    Dictionary<uint, WarpaintPalette> LoadWarpaintPalettes();

    // dbitems
    Dictionary<uint, AttributeCategory> LoadAttributeCategory();
    Dictionary<uint, AttributeDefinition> LoadAttributeDefinition();
    Dictionary<KeyValuePair<uint, ushort>, AttributeRange> LoadAttributeRange();
    Dictionary<KeyValuePair<uint, ushort>, ItemCharacterScalars> LoadItemCharacterScalars();
    Dictionary<uint, RootItem> LoadRootItem();
    Dictionary<uint, Battleframe> LoadBattleframe();

    // apt
    Dictionary<uint, BaseCommandDef> LoadBaseCommandDef();
    Dictionary<uint, CommandType> LoadCommandType();
    Dictionary<uint, AbilityData> LoadAbilityData();
    Dictionary<uint, ImpactApplyEffectCommandDef> LoadImpactApplyEffectCommandDef();
    Dictionary<uint, ConditionalBranchCommandDef> LoadConditionalBranchCommandDef();
    Dictionary<uint, WhileLoopCommandDef> LoadWhileLoopCommandDef();
    Dictionary<uint, LogicNegateCommandDef> LoadLogicNegateCommandDef();
    Dictionary<uint, LogicOrCommandDef> LoadLogicOrCommandDef();
    Dictionary<uint, LogicOrChainCommandDef> LoadLogicOrChainCommandDef();
    Dictionary<uint, LogicAndChainCommandDef> LoadLogicAndChainCommandDef();
    Dictionary<uint, CallCommandDef> LoadCallCommandDef();
    Dictionary<uint, InstantActivationCommandDef> LoadInstantActivationCommandDef();
    Dictionary<uint, StatusEffectData> LoadStatusEffectData();
    Dictionary<uint, TargetPBAECommandDef> LoadTargetPBAECommandDef();
    Dictionary<uint, TargetConeAECommandDef> LoadTargetConeAECommandDef();
    Dictionary<uint, TargetClearCommandDef> LoadTargetClearCommandDef();
    Dictionary<uint, TargetSelfCommandDef> LoadTargetSelfCommandDef();
    Dictionary<uint, TargetInitiatorCommandDef> LoadTargetInitiatorCommandDef();
    Dictionary<uint, TimeDurationCommandDef> LoadTimeDurationCommandDef();
    Dictionary<uint, AirborneDurationCommandDef> LoadAirborneDurationCommandDef();
    Dictionary<uint, ActivationDurationCommandDef> LoadActivationDurationCommandDef();
    Dictionary<uint, ReturnCommandDef> LoadReturnCommandDef();
    Dictionary<uint, LoadRegisterFromItemStatCommandDef> LoadLoadRegisterFromItemStatCommandDef();
    Dictionary<uint, LoadRegisterFromBonusCommandDef> LoadLoadRegisterFromBonusCommandDef();
    Dictionary<uint, LoadRegisterFromDamageCommandDef> LoadLoadRegisterFromDamageCommandDef();
    Dictionary<uint, LoadRegisterFromLevelCommandDef> LoadLoadRegisterFromLevelCommandDef();
    Dictionary<uint, LoadRegisterFromModulePowerCommandDef> LoadLoadRegisterFromModulePowerCommandDef();
    Dictionary<uint, LoadRegisterFromNamedVarCommandDef> LoadLoadRegisterFromNamedVarCommandDef();
    Dictionary<uint, LoadRegisterFromResourceCommandDef> LoadLoadRegisterFromResourceCommandDef();
    Dictionary<uint, LoadRegisterFromStatCommandDef> LoadLoadRegisterFromStatCommandDef();
    Dictionary<uint, RegisterComparisonCommandDef> LoadRegisterComparisonCommandDef();
    Dictionary<uint, RegisterRandomCommandDef> LoadRegisterRandomCommandDef();
    Dictionary<uint, SetRegisterCommandDef> LoadSetRegisterCommandDef();
    Dictionary<uint, NamedVariableAssignCommandDef> LoadNamedVariableAssignCommandDef();
    Dictionary<uint, InflictCooldownCommandDef> LoadInflictCooldownCommandDef();
    Dictionary<uint, RequireDamageTypeCommandDef> LoadRequireDamageTypeCommandDef();

    // aptfs
    Dictionary<uint, TargetFriendliesCommandDef> LoadTargetFriendliesCommandDef();
    Dictionary<uint, TargetByEffectCommandDef> LoadTargetByEffectCommandDef();
    Dictionary<uint, TargetOwnerCommandDef> LoadTargetOwnerCommandDef();
    Dictionary<uint, TargetByObjectTypeCommandDef> LoadTargetByObjectTypeCommandDef();
    Dictionary<uint, TargetHostilesCommandDef> LoadTargetHostilesCommandDef();
    Dictionary<uint, TargetByCharacterStateCommandDef> LoadTargetByCharacterStateCommandDef();
    Dictionary<uint, InflictDamageCommandDef> LoadInflictDamageCommandDef();
    Dictionary<uint, ForcePushCommandDef> LoadForcePushCommandDef();
    Dictionary<uint, ApplyImpulseCommandDef> LoadApplyImpulseCommandDef();
    Dictionary<uint, DeployableCalldownCommandDef> LoadDeployableCalldownCommandDef();
    Dictionary<uint, FireProjectileCommandDef> LoadFireProjectileCommandDef();
    Dictionary<uint, ResourceNodeBeaconCalldownCommandDef> LoadResourceNodeBeaconCalldownCommandDef();
    Dictionary<uint, RegisterClientProximityCommandDef> LoadRegisterClientProximityCommandDef();
    Dictionary<uint, CombatFlagsCommandDef> LoadCombatFlagsCommandDef();
    Dictionary<uint, ApplyFreezeCommandDef> LoadApplyFreezeCommandDef();
    Dictionary<uint, OrientationLockCommandDef> LoadOrientationLockCommandDef();
    Dictionary<uint, StatModifierCommandDef> LoadStatModifierCommandDef();
    Dictionary<uint, RequireAimModeCommandDef> LoadRequireAimModeCommandDef();
    Dictionary<uint, RequireArmyCommandDef> LoadRequireArmyCommandDef();
    Dictionary<uint, RequireBackstabCommandDef> LoadRequireBackstabCommandDef();
    Dictionary<uint, RequireBulletHitCommandDef> LoadRequireBulletHitCommandDef();
    Dictionary<uint, RequireCAISStateCommandDef> LoadRequireCAISStateCommandDef();
    Dictionary<uint, RequireCStateCommandDef> LoadRequireCStateCommandDef();
    Dictionary<uint, RequireDamageResponseCommandDef> LoadRequireDamageResponseCommandDef();
    Dictionary<uint, RequireEliteLevelCommandDef> LoadRequireEliteLevelCommandDef();
    Dictionary<uint, RequireEnergyByRangeCommandDef> LoadRequireEnergyByRangeCommandDef();
    Dictionary<uint, RequireEnergyCommandDef> LoadRequireEnergyCommandDef();
    Dictionary<uint, RequireEnergyFromTargetCommandDef> LoadRequireEnergyFromTargetCommandDef();
    Dictionary<uint, RequireEquippedItemCommandDef> LoadRequireEquippedItemCommandDef();
    Dictionary<uint, RequireFriendsCommandDef> LoadRequireFriendsCommandDef();
    Dictionary<uint, RequireHasCertificateCommandDef> LoadRequireHasCertificateCommandDef();
    Dictionary<uint, RequireHasEffectCommandDef> LoadRequireHasEffectCommandDef();
    Dictionary<uint, RequireHasEffectTagCommandDef> LoadRequireHasEffectTagCommandDef();
    Dictionary<uint, RequireHasItemCommandDef> LoadRequireHasItemCommandDef();
    Dictionary<uint, RequireHasUnlockCommandDef> LoadRequireHasUnlockCommandDef();
    Dictionary<uint, RequireHeadshotCommandDef> LoadRequireHeadshotCommandDef();
    Dictionary<uint, RequireInCombatCommandDef> LoadRequireInCombatCommandDef();
    Dictionary<uint, RequireInRangeCommandDef> LoadRequireInRangeCommandDef();
    Dictionary<uint, RequireInVehicleCommandDef> LoadRequireInVehicleCommandDef();
    Dictionary<uint, RequireIsNPCCommandDef> LoadRequireIsNPCCommandDef();
    Dictionary<uint, RequireItemAttributeCommandDef> LoadRequireItemAttributeCommandDef();
    Dictionary<uint, RequireItemDurabilityCommandDef> LoadRequireItemDurabilityCommandDef();
    Dictionary<uint, RequireJumpedCommandDef> LoadRequireJumpedCommandDef();
    Dictionary<uint, RequireLevelCommandDef> LoadRequireLevelCommandDef();
    Dictionary<uint, RequireLineOfSightCommandDef> LoadRequireLineOfSightCommandDef();
    Dictionary<uint, RequirementServerCommandDef> LoadRequirementServerCommandDef();
    Dictionary<uint, RequireMovementFlagsCommandDef> LoadRequireMovementFlagsCommandDef();
    Dictionary<uint, RequireMovestateCommandDef> LoadRequireMovestateCommandDef();
    Dictionary<uint, RequireMovingCommandDef> LoadRequireMovingCommandDef();
    Dictionary<uint, RequireNeedsAmmoCommandDef> LoadRequireNeedsAmmoCommandDef();
    Dictionary<uint, RequireNotRespawnedCommandDef> LoadRequireNotRespawnedCommandDef();
    Dictionary<uint, RequirePermissionCommandDef> LoadRequirePermissionCommandDef();
    Dictionary<uint, RequireProjectileSlopeCommandDef> LoadRequireProjectileSlopeCommandDef();
    Dictionary<uint, RequireReloadCommandDef> LoadRequireReloadCommandDef();
    Dictionary<uint, RequireResourceCommandDef> LoadRequireResourceCommandDef();
    Dictionary<uint, RequireResourceFromTargetCommandDef> LoadRequireResourceFromTargetCommandDef();
    Dictionary<uint, RequireSinAcquiredCommandDef> LoadRequireSinAcquiredCommandDef();
    Dictionary<uint, RequireSprintModifierCommandDef> LoadRequireSprintModifierCommandDef();
    Dictionary<uint, RequireSquadLeaderCommandDef> LoadRequireSquadLeaderCommandDef();
    Dictionary<uint, RequireSuperChargeCommandDef> LoadRequireSuperChargeCommandDef();
    Dictionary<uint, RequireTookDamageCommandDef> LoadRequireTookDamageCommandDef();
    Dictionary<uint, RequireWeaponArmedCommandDef> LoadRequireWeaponArmedCommandDef();
    Dictionary<uint, RequireWeaponTemplateCommandDef> LoadRequireWeaponTemplateCommandDef();
    Dictionary<uint, RequireZoneTypeCommandDef> LoadRequireZoneTypeCommandDef();
    Dictionary<uint, InteractionTypeCommandDef> LoadInteractionTypeCommandDef();
}