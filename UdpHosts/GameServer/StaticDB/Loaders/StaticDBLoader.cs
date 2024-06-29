namespace GameServer.Data.SDB;

using FauFau.Formats;
using Records.apt;
using Records.aptfs;
using Records.dbitems;
using Records.dbviusalrecords;
using Records.dbcharacter;
using Records.vcs;
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

    public Dictionary<uint, Monster> LoadMonster()
    {
        return LoadStaticDB<Monster>("dbcharacter::Monster")
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

    public Dictionary<uint, CarryableObject> LoadCarryableObject()
    {
        return LoadStaticDB<CarryableObject>("dbitems::CarryableObject")
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

    public Dictionary<uint, ImpactToggleEffectCommandDef> LoadImpactToggleEffectCommandDef()
    {
        return LoadStaticDB<ImpactToggleEffectCommandDef>("apt::ImpactToggleEffectCommandDef")
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

    public Dictionary<uint, StagedActivationCommandDef> LoadStagedActivationCommandDef()
    {
        return LoadStaticDB<StagedActivationCommandDef>("apt::StagedActivationCommandDef")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, StatusEffectData> LoadStatusEffectData()
    {
        return LoadStaticDB<StatusEffectData>("apt::StatusEffectData")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, HashSet<uint>> LoadStatusEffectTags()
    {
        return LoadStaticDB<StatusEffectTags>("apt::StatusEffectTags")
               .GroupBy(row => row.TagtypeId)
            .ToDictionary(group => group.Key, group => group.Select(item => item.StatusfxId).ToHashSet());
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

    public Dictionary<uint, TargetSwapCommandDef> LoadTargetSwapCommandDef()
    {
        return LoadStaticDB<TargetSwapCommandDef>("apt::TargetSwapCommandDef")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, TargetStackEmptyCommandDef> LoadTargetStackEmptyCommandDef()
    {
        return LoadStaticDB<TargetStackEmptyCommandDef>("apt::TargetStackEmptyCommandDef")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, PeekTargetsCommandDef> LoadPeekTargetsCommandDef()
    {
        return LoadStaticDB<PeekTargetsCommandDef>("apt::PeekTargetsCommandDef")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, PopTargetsCommandDef> LoadPopTargetsCommandDef()
    {
        return LoadStaticDB<PopTargetsCommandDef>("apt::PopTargetsCommandDef")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, PushTargetsCommandDef> LoadPushTargetsCommandDef()
    {
        return LoadStaticDB<PushTargetsCommandDef>("apt::PushTargetsCommandDef")
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

    public Dictionary<uint, TargetByEffectTagCommandDef> LoadTargetByEffectTagCommandDef()
    {
        return LoadStaticDB<TargetByEffectTagCommandDef>("aptfs::TargetByEffectTagCommandDef")
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

    public Dictionary<uint, RequestBattleFrameListCommandDef> LoadRequestBattleFrameListCommandDef()
    {
        return LoadStaticDB<RequestBattleFrameListCommandDef>("aptfs::RequestBattleFrameListCommandDef")
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

    public Dictionary<uint, VehicleCalldownCommandDef> LoadVehicleCalldownCommandDef()
    {
        return LoadStaticDB<VehicleCalldownCommandDef>("aptfs::VehicleCalldownCommandDef")
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

    public Dictionary<uint, AttemptToCalldownVehicleCommandDef> LoadAttemptToCalldownVehicleCommandDef()
    {
        return LoadStaticDB<AttemptToCalldownVehicleCommandDef>("aptfs::AttemptToCalldownVehicleCommandDef")
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

    public Dictionary<byte, VehicleClass> LoadVehicleClass()
    {
        return LoadStaticDB<VehicleClass>("vcs::VehicleClass")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<ushort, VehicleInfo> LoadVehicleInfo()
    {
        return LoadStaticDB<VehicleInfo>("vcs::VehicleInfo")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<ushort, Dictionary<uint, BaseComponentDef>> LoadBaseComponentDef()
    {
        return LoadStaticDB<BaseComponentDef>("vcs::BaseComponentDef")
        .GroupBy(row => row.VehicleId)
        .ToDictionary(group => group.Key, group => group.ToDictionary(row => row.Id, row => row));
    }

    public Dictionary<uint, ScopingComponentDef> LoadScopingComponentDef()
    {
        return LoadStaticDB<ScopingComponentDef>("vcs::ScopingComponentDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, DriverComponentDef> LoadDriverComponentDef()
    {
        return LoadStaticDB<DriverComponentDef>("vcs::DriverComponentDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, PassengerComponentDef> LoadPassengerComponentDef()
    {
        return LoadStaticDB<PassengerComponentDef>("vcs::PassengerComponentDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, AbilityComponentDef> LoadAbilityComponentDef()
    {
        return LoadStaticDB<AbilityComponentDef>("vcs::AbilityComponentDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, DamageComponentDef> LoadDamageComponentDef()
    {
        return LoadStaticDB<DamageComponentDef>("vcs::DamageComponentDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, StatusEffectComponentDef> LoadStatusEffectComponentDef()
    {
        return LoadStaticDB<StatusEffectComponentDef>("vcs::StatusEffectComponentDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, TurretComponentDef> LoadTurretComponentDef()
    {
        return LoadStaticDB<TurretComponentDef>("vcs::TurretComponentDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, DeployableComponentDef> LoadDeployableComponentDef()
    {
        return LoadStaticDB<DeployableComponentDef>("vcs::DeployableComponentDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, SpawnPointComponentDef> LoadSpawnPointComponentDef()
    {
        return LoadStaticDB<SpawnPointComponentDef>("vcs::SpawnPointComponentDef")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, TargetSingleCommandDef> LoadTargetSingleCommandDef()
    {
        return LoadStaticDB<TargetSingleCommandDef>("apt::TargetSingleCommandDef")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, TimeCooldownCommandDef> LoadTimeCooldownCommandDef()
    {
        return LoadStaticDB<TimeCooldownCommandDef>("apt::TimeCooldownCommandDef")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, TimedActivationCommandDef> LoadTimedActivationCommandDef()
    {
        return LoadStaticDB<TimedActivationCommandDef>("apt::TimedActivationCommandDef")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, PassiveInitiationCommandDef> LoadPassiveInitiationCommandDef()
    {
        return LoadStaticDB<PassiveInitiationCommandDef>("apt::PassiveInitiationCommandDef")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, TargetInteractivesCommandDef> LoadTargetInteractivesCommandDef()
    {
        return LoadStaticDB<TargetInteractivesCommandDef>("aptfs::TargetInteractivesCommandDef")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, ImpactMarkInteractivesCommandDef> LoadImpactMarkInteractivesCommandDef()
    {
        return LoadStaticDB<ImpactMarkInteractivesCommandDef>("aptfs::ImpactMarkInteractivesCommandDef")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, TargetPreviousCommandDef> LoadTargetPreviousCommandDef()
    {
        return LoadStaticDB<TargetPreviousCommandDef>("apt::TargetPreviousCommandDef")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, HasTargetsDurationCommandDef> LoadHasTargetsDurationCommandDef()
    {
        return LoadStaticDB<HasTargetsDurationCommandDef>("aptfs::HasTargetsDurationCommandDef")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, UpdateYieldCommandDef> LoadUpdateYieldCommandDef()
    {
        return LoadStaticDB<UpdateYieldCommandDef>("apt::UpdateYieldCommandDef")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, RopePullCommandDef> LoadRopePullCommandDef()
    {
        return LoadStaticDB<RopePullCommandDef>("aptfs::RopePullCommandDef")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, SetTargetOffsetCommandDef> LoadSetTargetOffsetCommandDef()
    {
        return LoadStaticDB<SetTargetOffsetCommandDef>("aptfs::SetTargetOffsetCommandDef")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, HealDamageCommandDef> LoadHealDamageCommandDef()
    {
        return LoadStaticDB<HealDamageCommandDef>("aptfs::HealDamageCommandDef")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, BullrushCommandDef> LoadBullrushCommandDef()
    {
        return LoadStaticDB<BullrushCommandDef>("aptfs::BullrushCommandDef")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, EnergyToDamageCommandDef> LoadEnergyToDamageCommandDef()
    {
        return LoadStaticDB<EnergyToDamageCommandDef>("aptfs::EnergyToDamageCommandDef")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, BattleFrameDurationCommandDef> LoadBattleFrameDurationCommandDef()
    {
        return LoadStaticDB<BattleFrameDurationCommandDef>("aptfs::BattleFrameDurationCommandDef")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, ShootingDurationCommandDef> LoadShootingDurationCommandDef()
    {
        return LoadStaticDB<ShootingDurationCommandDef>("aptfs::ShootingDurationCommandDef")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, SwitchWeaponCommandDef> LoadSwitchWeaponCommandDef()
    {
        return LoadStaticDB<SwitchWeaponCommandDef>("aptfs::SwitchWeaponCommandDef")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, StatRequirementCommandDef> LoadStatRequirementCommandDef()
    {
        return LoadStaticDB<StatRequirementCommandDef>("aptfs::StatRequirementCommandDef")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, ConsumeEnergyCommandDef> LoadConsumeEnergyCommandDef()
    {
        return LoadStaticDB<ConsumeEnergyCommandDef>("aptfs::ConsumeEnergyCommandDef")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, TargetClassTypeCommandDef> LoadTargetClassTypeCommandDef()
    {
        return LoadStaticDB<TargetClassTypeCommandDef>("aptfs::TargetClassTypeCommandDef")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, TargetDifferenceCommandDef> LoadTargetDifferenceCommandDef()
    {
        return LoadStaticDB<TargetDifferenceCommandDef>("apt::TargetDifferenceCommandDef")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, ClimbLedgeCommandDef> LoadClimbLedgeCommandDef()
    {
        return LoadStaticDB<ClimbLedgeCommandDef>("aptfs::ClimbLedgeCommandDef")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, AimRangeDurationCommandDef> LoadAimRangeDurationCommandDef()
    {
        return LoadStaticDB<AimRangeDurationCommandDef>("apt::AimRangeDurationCommandDef")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, CopyInitiationPositionCommandDef> LoadCopyInitiationPositionCommandDef()
    {
        return LoadStaticDB<CopyInitiationPositionCommandDef>("aptfs::CopyInitiationPositionCommandDef")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, SlotAmmoCommandDef> LoadSlotAmmoCommandDef()
    {
        return LoadStaticDB<SlotAmmoCommandDef>("aptfs::SlotAmmoCommandDef")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, AddPhysicsCommandDef> LoadAddPhysicsCommandDef()
    {
        return LoadStaticDB<AddPhysicsCommandDef>("aptfs::AddPhysicsCommandDef")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, TargetCurrentVehicleCommandDef> LoadTargetCurrentVehicleCommandDef()
    {
        return LoadStaticDB<TargetCurrentVehicleCommandDef>("aptfs::TargetCurrentVehicleCommandDef")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, TargetPassengersCommandDef> LoadTargetPassengersCommandDef()
    {
        return LoadStaticDB<TargetPassengersCommandDef>("aptfs::TargetPassengersCommandDef")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, TargetSquadmatesCommandDef> LoadTargetSquadmatesCommandDef()
    {
        return LoadStaticDB<TargetSquadmatesCommandDef>("aptfs::TargetSquadmatesCommandDef")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, TargetTrimCommandDef> LoadTargetTrimCommandDef()
    {
        return LoadStaticDB<TargetTrimCommandDef>("apt::TargetTrimCommandDef")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, SetWeaponDamageCommandDef> LoadSetWeaponDamageCommandDef()
    {
        return LoadStaticDB<SetWeaponDamageCommandDef>("aptfs::SetWeaponDamageCommandDef")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, ConsumeEnergyOverTimeCommandDef> LoadConsumeEnergyOverTimeCommandDef()
    {
        return LoadStaticDB<ConsumeEnergyOverTimeCommandDef>("aptfs::ConsumeEnergyOverTimeCommandDef")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, RequestAbilitySelectionCommandDef> LoadRequestAbilitySelectionCommandDef()
    {
        return LoadStaticDB<RequestAbilitySelectionCommandDef>("aptfs::RequestAbilitySelectionCommandDef")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, BonusGreaterThanCommandDef> LoadBonusGreaterThanCommandDef()
    {
        return LoadStaticDB<BonusGreaterThanCommandDef>("apt::BonusGreaterThanCommandDef")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, BombardmentCommandDef> LoadBombardmentCommandDef()
    {
        return LoadStaticDB<BombardmentCommandDef>("aptfs::BombardmentCommandDef")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, SetProjectileTargetCommandDef> LoadSetProjectileTargetCommandDef()
    {
        return LoadStaticDB<SetProjectileTargetCommandDef>("aptfs::SetProjectileTargetCommandDef")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, UpdateWaitCommandDef> LoadUpdateWaitCommandDef()
    {
        return LoadStaticDB<UpdateWaitCommandDef>("apt::UpdateWaitCommandDef")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, PushRegisterCommandDef> LoadPushRegisterCommandDef()
    {
        return LoadStaticDB<PushRegisterCommandDef>("apt::PushRegisterCommandDef")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, PopRegisterCommandDef> LoadPopRegisterCommandDef()
    {
        return LoadStaticDB<PopRegisterCommandDef>("apt::PopRegisterCommandDef")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, PeekRegisterCommandDef> LoadPeekRegisterCommandDef()
    {
        return LoadStaticDB<PeekRegisterCommandDef>("apt::PeekRegisterCommandDef")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, MovementSlideCommandDef> LoadMovementSlideCommandDef()
    {
        return LoadStaticDB<MovementSlideCommandDef>("aptfs::MovementSlideCommandDef")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, TargetFromStatusEffectCommandDef> LoadTargetFromStatusEffectCommandDef()
    {
        return LoadStaticDB<TargetFromStatusEffectCommandDef>("aptfs::TargetFromStatusEffectCommandDef")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, TargetByDamageResponseCommandDef> LoadTargetByDamageResponseCommandDef()
    {
        return LoadStaticDB<TargetByDamageResponseCommandDef>("aptfs::TargetByDamageResponseCommandDef")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, ForcedMovementDurationCommandDef> LoadForcedMovementDurationCommandDef()
    {
        return LoadStaticDB<ForcedMovementDurationCommandDef>("aptfs::ForcedMovementDurationCommandDef")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, FireUiEventCommandDef> LoadFireUiEventCommandDef()
    {
        return LoadStaticDB<FireUiEventCommandDef>("aptfs::FireUiEventCommandDef")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, UiNamedVariableCommandDef> LoadUiNamedVariableCommandDef()
    {
        return LoadStaticDB<UiNamedVariableCommandDef>("aptfs::UiNamedVariableCommandDef")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, DetonateProjectilesCommandDef> LoadDetonateProjectilesCommandDef()
    {
        return LoadStaticDB<DetonateProjectilesCommandDef>("aptfs::DetonateProjectilesCommandDef")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, SetWeaponDamageTypeCommandDef> LoadSetWeaponDamageTypeCommandDef()
    {
        return LoadStaticDB<SetWeaponDamageTypeCommandDef>("aptfs::SetWeaponDamageTypeCommandDef")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, TargetFilterMovestateCommandDef> LoadTargetFilterMovestateCommandDef()
    {
        return LoadStaticDB<TargetFilterMovestateCommandDef>("aptfs::TargetFilterMovestateCommandDef")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, TargetByHostilityCommandDef> LoadTargetByHostilityCommandDef()
    {
        return LoadStaticDB<TargetByHostilityCommandDef>("aptfs::TargetByHostilityCommandDef")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, ConsumeSuperChargeCommandDef> LoadConsumeSuperChargeCommandDef()
    {
        return LoadStaticDB<ConsumeSuperChargeCommandDef>("aptfs::ConsumeSuperChargeCommandDef")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, TargetByHealthCommandDef> LoadTargetByHealthCommandDef()
    {
        return LoadStaticDB<TargetByHealthCommandDef>("aptfs::TargetByHealthCommandDef")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, RegisterMovementEffectCommandDef> LoadRegisterMovementEffectCommandDef()
    {
        return LoadStaticDB<RegisterMovementEffectCommandDef>("aptfs::RegisterMovementEffectCommandDef")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, UpdateWaitAndFireOnceCommandDef> LoadUpdateWaitAndFireOnceCommandDef()
    {
        return LoadStaticDB<UpdateWaitAndFireOnceCommandDef>("apt::UpdateWaitAndFireOnceCommandDef")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, ApplyAmmoRiderCommandDef> LoadApplyAmmoRiderCommandDef()
    {
        return LoadStaticDB<ApplyAmmoRiderCommandDef>("aptfs::ApplyAmmoRiderCommandDef")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, TargetFilterByRangeCommandDef> LoadTargetFilterByRangeCommandDef()
    {
        return LoadStaticDB<TargetFilterByRangeCommandDef>("aptfs::TargetFilterByRangeCommandDef")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, OverrideCollisionCommandDef> LoadOverrideCollisionCommandDef()
    {
        return LoadStaticDB<OverrideCollisionCommandDef>("aptfs::OverrideCollisionCommandDef")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, RegisterLoadScaleCommandDef> LoadRegisterLoadScaleCommandDef()
    {
        return LoadStaticDB<RegisterLoadScaleCommandDef>("apt::RegisterLoadScaleCommandDef")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, MovementFacingCommandDef> LoadMovementFacingCommandDef()
    {
        return LoadStaticDB<MovementFacingCommandDef>("aptfs::MovementFacingCommandDef")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, TargetFilterBySinAcquiredCommandDef> LoadTargetFilterBySinAcquiredCommandDef()
    {
        return LoadStaticDB<TargetFilterBySinAcquiredCommandDef>("aptfs::TargetFilterBySinAcquiredCommandDef")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, MovementTetherCommandDef> LoadMovementTetherCommandDef()
    {
        return LoadStaticDB<MovementTetherCommandDef>("aptfs::MovementTetherCommandDef")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, RegisterLoadFromWeaponCommandDef> LoadRegisterLoadFromWeaponCommandDef()
    {
        return LoadStaticDB<RegisterLoadFromWeaponCommandDef>("aptfs::RegisterLoadFromWeaponCommandDef")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, ApplyClientStatusEffectCommandDef> LoadApplyClientStatusEffectCommandDef()
    {
        return LoadStaticDB<ApplyClientStatusEffectCommandDef>("aptfs::ApplyClientStatusEffectCommandDef")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, RemoveClientStatusEffectCommandDef> LoadRemoveClientStatusEffectCommandDef()
    {
        return LoadStaticDB<RemoveClientStatusEffectCommandDef>("aptfs::RemoveClientStatusEffectCommandDef")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, DisableChatBubbleCommandDef> LoadDisableChatBubbleCommandDef()
    {
        return LoadStaticDB<DisableChatBubbleCommandDef>("aptfs::DisableChatBubbleCommandDef")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, DisableHealthAndIconCommandDef> LoadDisableHealthAndIconCommandDef()
    {
        return LoadStaticDB<DisableHealthAndIconCommandDef>("aptfs::DisableHealthAndIconCommandDef")
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
                int backupIndex = table.GetColumnIndexByName(propInfo.Name);
                if (index != -1)
                {
                    propInfo.SetValue(entry, row[index], null);
                }
                else if (backupIndex != -1)
                {
                    propInfo.SetValue(entry, row[backupIndex], null);
                }
                else if (propInfo.Name == "DamageType")
                {
                    // Appears in InflictDamageCommandDef and HealDamageCommandDef
                    propInfo.SetValue(entry, row[table.GetColumnIndexByName("damageType")], null);
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