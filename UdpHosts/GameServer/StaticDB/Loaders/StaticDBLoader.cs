namespace GameServer.Data.SDB;

using FauFau.Formats;
using Records.apt;
using Records.aptfs;
using Records.dbitems;
using Records.dbviusalrecords;
using Records.dbcharacter;
using System;
using System.Collections.Generic;
using System.Linq;
using static FauFau.Formats.StaticDB;
using Shared.Common;

public class StaticDBLoader : ISDBLoader
{
    private static readonly SnakeCasePropertyNamingPolicy Policy = new SnakeCasePropertyNamingPolicy();
    private static StaticDB sdb;

    public StaticDBLoader(StaticDB instance)
    {
        sdb = instance;
    }

    public Dictionary<uint, CharCreateLoadout> LoadCharCreateLoadout() 
    {
        return LoadStaticDB<CharCreateLoadout>("dbcharacter::CharCreateLoadout")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, Dictionary<byte, CharCreateLoadoutSlots>> LoadCharCreateLoadoutSlots() 
    {
        return LoadStaticDB<CharCreateLoadoutSlots>("dbcharacter::CharCreateLoadoutSlots")
        .GroupBy(row => row.LoadoutId)
        .ToDictionary(group => group.Key, group => group.ToDictionary(row => row.SlotType, row => row));
    }

    public Dictionary<uint, Deployable> LoadDeployable()
    {
        return LoadStaticDB<Deployable>("dbcharacter::Deployable")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, WarpaintPalette> LoadWarpaintPalettes() 
    {
        return LoadStaticDB<WarpaintPalette>("dbvisualrecords::WarpaintPalette")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, AttributeCategory> LoadAttributeCategory() 
    {
        return LoadStaticDB<AttributeCategory>("dbitems::AttributeCategory")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, AttributeDefinition> LoadAttributeDefinition() 
    {
        return LoadStaticDB<AttributeDefinition>("dbitems::AttributeDefinition")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<KeyValuePair<uint, ushort>, AttributeRange> LoadAttributeRange() 
    {
        // There are duplicates, like item 78084 which has the range attribute twice. Ingame, it seems too use only one result for that one, so chosing to do the same here.
        return LoadStaticDB<AttributeRange>("dbitems::AttributeRange")
        .GroupBy(row => new KeyValuePair<uint, ushort>(row.ItemId, row.AttributeId))
        .ToDictionary(group => group.Key, group => group.Last());
    }
    
    public Dictionary<KeyValuePair<uint, ushort>, ItemCharacterScalars> LoadItemCharacterScalars() 
    {
        return LoadStaticDB<ItemCharacterScalars>("dbitems::ItemCharacterScalars")
        .ToDictionary(row => new KeyValuePair<uint, ushort>(row.ItemId, row.AttributeCategory));
    }

    public Dictionary<uint, RootItem> LoadRootItem() 
    {
        return LoadStaticDB<RootItem>("dbitems::RootItem")
        .ToDictionary(row => row.SdbId);
    }

    public Dictionary<uint, Battleframe> LoadBattleframe() 
    {
        return LoadStaticDB<Battleframe>("dbitems::Battleframe")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, AbilityModule> LoadAbilityModule()
    {
        return LoadStaticDB<AbilityModule>("dbitems::AbilityModule")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, BaseCommandDef> LoadBaseCommandDef()
    {
        return LoadStaticDB<BaseCommandDef>("apt::BaseCommandDef")
        .ToDictionary(row => row.Id); 
    }

    public Dictionary<uint, CommandType> LoadCommandType()
    {
        return LoadStaticDB<CommandType>("apt::CommandType")
        .ToDictionary(row => row.Id); 
    }

    public Dictionary<uint, AbilityData> LoadAbilityData()
    {
        return LoadStaticDB<AbilityData>("apt::AbilityData")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, ImpactApplyEffectCommandDef> LoadImpactApplyEffectCommandDef()
    {
        return LoadStaticDB<ImpactApplyEffectCommandDef>("apt::ImpactApplyEffectCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, ConditionalBranchCommandDef> LoadConditionalBranchCommandDef()
    {
        return LoadStaticDB<ConditionalBranchCommandDef>("apt::ConditionalBranchCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, WhileLoopCommandDef> LoadWhileLoopCommandDef()
    {
        return LoadStaticDB<WhileLoopCommandDef>("apt::WhileLoopCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, LogicNegateCommandDef> LoadLogicNegateCommandDef()
    {
        return LoadStaticDB<LogicNegateCommandDef>("apt::LogicNegateCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, LogicOrCommandDef> LoadLogicOrCommandDef()
    {
        return LoadStaticDB<LogicOrCommandDef>("apt::LogicOrCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, LogicOrChainCommandDef> LoadLogicOrChainCommandDef()
    {
        return LoadStaticDB<LogicOrChainCommandDef>("apt::LogicOrChainCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, LogicAndChainCommandDef> LoadLogicAndChainCommandDef()
    {
        return LoadStaticDB<LogicAndChainCommandDef>("apt::LogicAndChainCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, CallCommandDef> LoadCallCommandDef()
    {
        return LoadStaticDB<CallCommandDef>("apt::CallCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, InstantActivationCommandDef> LoadInstantActivationCommandDef()
    {
        return LoadStaticDB<InstantActivationCommandDef>("apt::InstantActivationCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, StatusEffectData> LoadStatusEffectData()
    {
        return LoadStaticDB<StatusEffectData>("apt::StatusEffectData")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, TargetPBAECommandDef> LoadTargetPBAECommandDef()
    {
        return LoadStaticDB<TargetPBAECommandDef>("apt::TargetPBAECommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, TargetConeAECommandDef> LoadTargetConeAECommandDef()
    {
        return LoadStaticDB<TargetConeAECommandDef>("apt::TargetConeAECommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, TargetClearCommandDef> LoadTargetClearCommandDef()
    {
        return LoadStaticDB<TargetClearCommandDef>("apt::TargetClearCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, TargetSelfCommandDef> LoadTargetSelfCommandDef()
    {
        return LoadStaticDB<TargetSelfCommandDef>("apt::TargetSelfCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, TargetInitiatorCommandDef> LoadTargetInitiatorCommandDef()
    {
        return LoadStaticDB<TargetInitiatorCommandDef>("apt::TargetInitiatorCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, TargetFriendliesCommandDef> LoadTargetFriendliesCommandDef()
    {
        return LoadStaticDB<TargetFriendliesCommandDef>("aptfs::TargetFriendliesCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, TargetByEffectCommandDef> LoadTargetByEffectCommandDef()
    {
        return LoadStaticDB<TargetByEffectCommandDef>("aptfs::TargetByEffectCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, TargetOwnerCommandDef> LoadTargetOwnerCommandDef()
    {
        return LoadStaticDB<TargetOwnerCommandDef>("aptfs::TargetOwnerCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, TargetByObjectTypeCommandDef> LoadTargetByObjectTypeCommandDef()
    {
        return LoadStaticDB<TargetByObjectTypeCommandDef>("aptfs::TargetByObjectTypeCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, TargetHostilesCommandDef> LoadTargetHostilesCommandDef()
    {
        return LoadStaticDB<TargetHostilesCommandDef>("aptfs::TargetHostilesCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, TargetByCharacterStateCommandDef>  LoadTargetByCharacterStateCommandDef()
    {
        return LoadStaticDB<TargetByCharacterStateCommandDef>("aptfs::TargetByCharacterStateCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, InflictDamageCommandDef> LoadInflictDamageCommandDef()
    {
        return LoadStaticDB<InflictDamageCommandDef>("aptfs::InflictDamageCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, ForcePushCommandDef> LoadForcePushCommandDef()
    {
        return LoadStaticDB<ForcePushCommandDef>("aptfs::ForcePushCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, ApplyImpulseCommandDef> LoadApplyImpulseCommandDef()
    {
        return LoadStaticDB<ApplyImpulseCommandDef>("aptfs::ApplyImpulseCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, DeployableCalldownCommandDef> LoadDeployableCalldownCommandDef()
    {
        return LoadStaticDB<DeployableCalldownCommandDef>("aptfs::DeployableCalldownCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, FireProjectileCommandDef> LoadFireProjectileCommandDef()
    {
        return LoadStaticDB<FireProjectileCommandDef>("aptfs::FireProjectileCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, ResourceNodeBeaconCalldownCommandDef> LoadResourceNodeBeaconCalldownCommandDef()
    {
        return LoadStaticDB<ResourceNodeBeaconCalldownCommandDef>("aptfs::ResourceNodeBeaconCalldownCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, RegisterClientProximityCommandDef> LoadRegisterClientProximityCommandDef()
    {
        return LoadStaticDB<RegisterClientProximityCommandDef>("aptfs::RegisterClientProximityCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, CombatFlagsCommandDef> LoadCombatFlagsCommandDef()
    {
        return LoadStaticDB<CombatFlagsCommandDef>("aptfs::CombatFlagsCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, ApplyFreezeCommandDef> LoadApplyFreezeCommandDef()
    {
        return LoadStaticDB<ApplyFreezeCommandDef>("aptfs::ApplyFreezeCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, OrientationLockCommandDef> LoadOrientationLockCommandDef()
    {
        return LoadStaticDB<OrientationLockCommandDef>("aptfs::OrientationLockCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, StatModifierCommandDef> LoadStatModifierCommandDef()
    {
        return LoadStaticDB<StatModifierCommandDef>("aptfs::StatModifierCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, RequireAimModeCommandDef> LoadRequireAimModeCommandDef()
    {
        return LoadStaticDB<RequireAimModeCommandDef>("aptfs::RequireAimModeCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, RequireArmyCommandDef> LoadRequireArmyCommandDef()
    {
        return LoadStaticDB<RequireArmyCommandDef>("aptfs::RequireArmyCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, RequireBackstabCommandDef> LoadRequireBackstabCommandDef()
    {
        return LoadStaticDB<RequireBackstabCommandDef>("aptfs::RequireBackstabCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, RequireBulletHitCommandDef> LoadRequireBulletHitCommandDef()
    {
        return LoadStaticDB<RequireBulletHitCommandDef>("aptfs::RequireBulletHitCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, RequireCAISStateCommandDef> LoadRequireCAISStateCommandDef()
    {
        return LoadStaticDB<RequireCAISStateCommandDef>("aptfs::RequireCAISStateCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, RequireCStateCommandDef> LoadRequireCStateCommandDef()
    {
        return LoadStaticDB<RequireCStateCommandDef>("aptfs::RequireCStateCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, RequireDamageResponseCommandDef> LoadRequireDamageResponseCommandDef()
    {
        return LoadStaticDB<RequireDamageResponseCommandDef>("aptfs::RequireDamageResponseCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, RequireEliteLevelCommandDef> LoadRequireEliteLevelCommandDef()
    {
        return LoadStaticDB<RequireEliteLevelCommandDef>("aptfs::RequireEliteLevelCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, RequireEnergyByRangeCommandDef> LoadRequireEnergyByRangeCommandDef()
    {
        return LoadStaticDB<RequireEnergyByRangeCommandDef>("aptfs::RequireEnergyByRangeCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, RequireEnergyCommandDef> LoadRequireEnergyCommandDef()
    {
        return LoadStaticDB<RequireEnergyCommandDef>("aptfs::RequireEnergyCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, RequireEnergyFromTargetCommandDef> LoadRequireEnergyFromTargetCommandDef()
    {
        return LoadStaticDB<RequireEnergyFromTargetCommandDef>("aptfs::RequireEnergyFromTargetCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, RequireEquippedItemCommandDef> LoadRequireEquippedItemCommandDef()
    {
        return LoadStaticDB<RequireEquippedItemCommandDef>("aptfs::RequireEquippedItemCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, RequireFriendsCommandDef> LoadRequireFriendsCommandDef()
    {
        return LoadStaticDB<RequireFriendsCommandDef>("aptfs::RequireFriendsCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, RequireHasCertificateCommandDef> LoadRequireHasCertificateCommandDef()
    {
        return LoadStaticDB<RequireHasCertificateCommandDef>("aptfs::RequireHasCertificateCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, RequireHasEffectCommandDef> LoadRequireHasEffectCommandDef()
    {
        return LoadStaticDB<RequireHasEffectCommandDef>("aptfs::RequireHasEffectCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, RequireHasEffectTagCommandDef> LoadRequireHasEffectTagCommandDef()
    {
        return LoadStaticDB<RequireHasEffectTagCommandDef>("aptfs::RequireHasEffectTagCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, RequireHasItemCommandDef> LoadRequireHasItemCommandDef()
    {
        return LoadStaticDB<RequireHasItemCommandDef>("aptfs::RequireHasItemCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, RequireHasUnlockCommandDef> LoadRequireHasUnlockCommandDef()
    {
        return LoadStaticDB<RequireHasUnlockCommandDef>("aptfs::RequireHasUnlockCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, RequireHeadshotCommandDef> LoadRequireHeadshotCommandDef()
    {
        return LoadStaticDB<RequireHeadshotCommandDef>("aptfs::RequireHeadshotCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, RequireInCombatCommandDef> LoadRequireInCombatCommandDef()
    {
        return LoadStaticDB<RequireInCombatCommandDef>("aptfs::RequireInCombatCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, RequireInRangeCommandDef> LoadRequireInRangeCommandDef()
    {
        return LoadStaticDB<RequireInRangeCommandDef>("aptfs::RequireInRangeCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, RequireInVehicleCommandDef> LoadRequireInVehicleCommandDef()
    {
        return LoadStaticDB<RequireInVehicleCommandDef>("aptfs::RequireInVehicleCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, RequireIsNPCCommandDef> LoadRequireIsNPCCommandDef()
    {
        return LoadStaticDB<RequireIsNPCCommandDef>("aptfs::RequireIsNPCCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, RequireItemAttributeCommandDef> LoadRequireItemAttributeCommandDef()
    {
        return LoadStaticDB<RequireItemAttributeCommandDef>("aptfs::RequireItemAttributeCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, RequireItemDurabilityCommandDef> LoadRequireItemDurabilityCommandDef()
    {
        return LoadStaticDB<RequireItemDurabilityCommandDef>("aptfs::RequireItemDurabilityCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, RequireJumpedCommandDef> LoadRequireJumpedCommandDef()
    {
        return LoadStaticDB<RequireJumpedCommandDef>("aptfs::RequireJumpedCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, RequireLevelCommandDef> LoadRequireLevelCommandDef()
    {
        return LoadStaticDB<RequireLevelCommandDef>("aptfs::RequireLevelCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, RequireLineOfSightCommandDef> LoadRequireLineOfSightCommandDef()
    {
        return LoadStaticDB<RequireLineOfSightCommandDef>("aptfs::RequireLineOfSightCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, RequirementServerCommandDef> LoadRequirementServerCommandDef()
    {
        return LoadStaticDB<RequirementServerCommandDef>("aptfs::RequirementServerCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, RequireMovementFlagsCommandDef> LoadRequireMovementFlagsCommandDef()
    {
        return LoadStaticDB<RequireMovementFlagsCommandDef>("aptfs::RequireMovementFlagsCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, RequireMovestateCommandDef> LoadRequireMovestateCommandDef()
    {
        return LoadStaticDB<RequireMovestateCommandDef>("aptfs::RequireMovestateCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, RequireMovingCommandDef> LoadRequireMovingCommandDef()
    {
        return LoadStaticDB<RequireMovingCommandDef>("aptfs::RequireMovingCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, RequireNeedsAmmoCommandDef> LoadRequireNeedsAmmoCommandDef()
    {
        return LoadStaticDB<RequireNeedsAmmoCommandDef>("aptfs::RequireNeedsAmmoCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, RequireNotRespawnedCommandDef> LoadRequireNotRespawnedCommandDef()
    {
        return LoadStaticDB<RequireNotRespawnedCommandDef>("aptfs::RequireNotRespawnedCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, RequirePermissionCommandDef> LoadRequirePermissionCommandDef()
    {
        return LoadStaticDB<RequirePermissionCommandDef>("aptfs::RequirePermissionCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, RequireProjectileSlopeCommandDef> LoadRequireProjectileSlopeCommandDef()
    {
        return LoadStaticDB<RequireProjectileSlopeCommandDef>("aptfs::RequireProjectileSlopeCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, RequireReloadCommandDef> LoadRequireReloadCommandDef()
    {
        return LoadStaticDB<RequireReloadCommandDef>("aptfs::RequireReloadCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, RequireResourceCommandDef> LoadRequireResourceCommandDef()
    {
        return LoadStaticDB<RequireResourceCommandDef>("aptfs::RequireResourceCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, RequireResourceFromTargetCommandDef> LoadRequireResourceFromTargetCommandDef()
    {
        return LoadStaticDB<RequireResourceFromTargetCommandDef>("aptfs::RequireResourceFromTargetCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, RequireSinAcquiredCommandDef> LoadRequireSinAcquiredCommandDef()
    {
        return LoadStaticDB<RequireSinAcquiredCommandDef>("aptfs::RequireSinAcquiredCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, RequireSprintModifierCommandDef> LoadRequireSprintModifierCommandDef()
    {
        return LoadStaticDB<RequireSprintModifierCommandDef>("aptfs::RequireSprintModifierCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, RequireSquadLeaderCommandDef> LoadRequireSquadLeaderCommandDef()
    {
        return LoadStaticDB<RequireSquadLeaderCommandDef>("aptfs::RequireSquadLeaderCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, RequireSuperChargeCommandDef> LoadRequireSuperChargeCommandDef()
    {
        return LoadStaticDB<RequireSuperChargeCommandDef>("aptfs::RequireSuperChargeCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, RequireTookDamageCommandDef> LoadRequireTookDamageCommandDef()
    {
        return LoadStaticDB<RequireTookDamageCommandDef>("aptfs::RequireTookDamageCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, RequireWeaponArmedCommandDef> LoadRequireWeaponArmedCommandDef()
    {
        return LoadStaticDB<RequireWeaponArmedCommandDef>("aptfs::RequireWeaponArmedCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, RequireWeaponTemplateCommandDef> LoadRequireWeaponTemplateCommandDef()
    {
        return LoadStaticDB<RequireWeaponTemplateCommandDef>("aptfs::RequireWeaponTemplateCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, RequireZoneTypeCommandDef> LoadRequireZoneTypeCommandDef()
    {
        return LoadStaticDB<RequireZoneTypeCommandDef>("aptfs::RequireZoneTypeCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, RequireDamageTypeCommandDef> LoadRequireDamageTypeCommandDef()
    {
        return LoadStaticDB<RequireDamageTypeCommandDef>("apt::RequireDamageTypeCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, TimeDurationCommandDef> LoadTimeDurationCommandDef()
    {
        return LoadStaticDB<TimeDurationCommandDef>("apt::TimeDurationCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, AirborneDurationCommandDef> LoadAirborneDurationCommandDef()
    {
        return LoadStaticDB<AirborneDurationCommandDef>("aptfs::AirborneDurationCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, ActivationDurationCommandDef> LoadActivationDurationCommandDef()
    {
        return LoadStaticDB<ActivationDurationCommandDef>("aptfs::ActivationDurationCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, ReturnCommandDef> LoadReturnCommandDef()
    {
        return LoadStaticDB<ReturnCommandDef>("apt::ReturnCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, LoadRegisterFromItemStatCommandDef> LoadLoadRegisterFromItemStatCommandDef()
    {
        return LoadStaticDB<LoadRegisterFromItemStatCommandDef>("apt::LoadRegisterFromItemStatCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, LoadRegisterFromBonusCommandDef> LoadLoadRegisterFromBonusCommandDef()
    {
        return LoadStaticDB<LoadRegisterFromBonusCommandDef>("apt::LoadRegisterFromBonusCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, LoadRegisterFromDamageCommandDef> LoadLoadRegisterFromDamageCommandDef()
    {
        return LoadStaticDB<LoadRegisterFromDamageCommandDef>("apt::LoadRegisterFromDamageCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, LoadRegisterFromLevelCommandDef> LoadLoadRegisterFromLevelCommandDef()
    {
        return LoadStaticDB<LoadRegisterFromLevelCommandDef>("apt::LoadRegisterFromLevelCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, LoadRegisterFromModulePowerCommandDef> LoadLoadRegisterFromModulePowerCommandDef()
    {
        return LoadStaticDB<LoadRegisterFromModulePowerCommandDef>("apt::LoadRegisterFromModulePowerCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, LoadRegisterFromNamedVarCommandDef> LoadLoadRegisterFromNamedVarCommandDef()
    {
        return LoadStaticDB<LoadRegisterFromNamedVarCommandDef>("apt::LoadRegisterFromNamedVarCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, LoadRegisterFromResourceCommandDef> LoadLoadRegisterFromResourceCommandDef()
    {
        return LoadStaticDB<LoadRegisterFromResourceCommandDef>("apt::LoadRegisterFromResourceCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, LoadRegisterFromStatCommandDef> LoadLoadRegisterFromStatCommandDef()
    {
        return LoadStaticDB<LoadRegisterFromStatCommandDef>("apt::LoadRegisterFromStatCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, RegisterComparisonCommandDef> LoadRegisterComparisonCommandDef()
    {
        return LoadStaticDB<RegisterComparisonCommandDef>("apt::RegisterComparisonCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, RegisterRandomCommandDef> LoadRegisterRandomCommandDef()
    {
        return LoadStaticDB<RegisterRandomCommandDef>("apt::RegisterRandomCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, SetRegisterCommandDef> LoadSetRegisterCommandDef()
    {
        return LoadStaticDB<SetRegisterCommandDef>("apt::SetRegisterCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, NamedVariableAssignCommandDef> LoadNamedVariableAssignCommandDef()
    {
        return LoadStaticDB<NamedVariableAssignCommandDef>("apt::NamedVariableAssignCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, InflictCooldownCommandDef> LoadInflictCooldownCommandDef()
    {
        return LoadStaticDB<InflictCooldownCommandDef>("apt::InflictCooldownCommandDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, InteractionTypeCommandDef> LoadInteractionTypeCommandDef()
    {
        return LoadStaticDB<InteractionTypeCommandDef>("aptfs::InteractionTypeCommandDef")
        .ToDictionary(row => row.Id);
    }

    private static T[] LoadStaticDB<T>(string tableName)
    where T : class, new()
    {
        HashSet<string> warningsSet = new HashSet<string>();

        Table table = sdb.GetTableByName(tableName);
        var list = new List<T>();
        foreach(Row row in table.Rows)
        {
            T entry = new T();
            var propInfoList = entry.GetType().GetProperties().ToList();
            foreach(var propInfo in propInfoList)
            {
                string convertedName = Policy.ConvertName(propInfo.Name);
                int index = table.GetColumnIndexByName(convertedName);
                if (index != -1)
                {
                    propInfo.SetValue(entry, row[index], null);
                }
                else
                {
                    warningsSet.Add($"Could not find column for {propInfo.Name} (converted to {convertedName}) in {tableName}");
                }
            }

            list.Add(entry);
        }

        foreach(string text in warningsSet)
        {
            Console.WriteLine(text);
        }

        return list.ToArray();
    }
}