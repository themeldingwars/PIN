using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using GameServer.Data.SDB.Records.customdata;
using Shared.Common;

public class CustomDBLoader
{
    private static readonly SnakeCasePropertyNamingPolicy Policy = new SnakeCasePropertyNamingPolicy();
    private readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = Policy,
        IncludeFields = true
    };

    public Dictionary<uint, AuthorizeTerminalCommandDef> LoadAuthorizeTerminalCommandDef() 
    {
        return LoadJSON<AuthorizeTerminalCommandDef>("./StaticDB/CustomData/aptgss_AuthorizeTerminalCommandDef.json")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, SetGliderParametersCommandDef> LoadSetGliderParametersCommandDef() 
    {
        return LoadJSON<SetGliderParametersCommandDef>("./StaticDB/CustomData/aptgss_agsSetGliderParametersDef.json")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, ModifyPermissionCommandDef> LoadModifyPermissionCommandDef() 
    {
        return LoadJSON<ModifyPermissionCommandDef>("./StaticDB/CustomData/aptgss_agsModifyPermissionCommandDef.json")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, ImpactRemoveEffectCommandDef> LoadImpactRemoveEffectCommandDef() 
    {
        return LoadJSON<ImpactRemoveEffectCommandDef>("./StaticDB/CustomData/aptgss_agsImpactRemoveEffectCommandDef.json")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, DeployableSpawnCommandDef> LoadDeployableSpawnCommandDef() 
    {
        return LoadJSON<DeployableSpawnCommandDef>("./StaticDB/CustomData/aptgss_agsDeployableSpawnCommandDef.json")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, EncounterSignalCommandDef> LoadEncounterSignalCommandDef()
    {
        return LoadJSON<EncounterSignalCommandDef>("./StaticDB/CustomData/aptgss_agsEncounterSignalCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, MountVehicleCommandDef> LoadMountVehicleCommandDef()
    {
        return LoadJSON<MountVehicleCommandDef>("./StaticDB/CustomData/aptgss_agsMountVehicleCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, CarryableObjectSpawnCommandDef> LoadCarryableObjectSpawnCommandDef()
    {
        return LoadJSON<CarryableObjectSpawnCommandDef>("./StaticDB/CustomData/aptgss_CarryableObjectSpawnCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, ApplySinCardCommandDef> LoadApplySinCardCommandDef()
    {
        return LoadJSON<ApplySinCardCommandDef>("./StaticDB/CustomData/aptgss_ApplySinCardCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, CreateSpawnPointCommandDef> LoadCreateSpawnPointCommandDef()
    {
        return LoadJSON<CreateSpawnPointCommandDef>("./StaticDB/CustomData/aptgss_agsCreateSpawnPointCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, AbilityFinishedCommandDef> LoadAbilityFinishedCommandDef()
    {
        return LoadJSON<AbilityFinishedCommandDef>("./StaticDB/CustomData/aptgss_AbilityFinishedCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, ActivateAbilityTriggerCommandDef> LoadActivateAbilityTriggerCommandDef()
    {
        return LoadJSON<ActivateAbilityTriggerCommandDef>("./StaticDB/CustomData/aptgss_ActivateAbilityTriggerCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, AddAccountGroupCommandDef> LoadAddAccountGroupCommandDef()
    {
        return LoadJSON<AddAccountGroupCommandDef>("./StaticDB/CustomData/aptgss_AddAccountGroupCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, AddAppendageHealthPoolCommandDef> LoadAddAppendageHealthPoolCommandDef()
    {
        return LoadJSON<AddAppendageHealthPoolCommandDef>("./StaticDB/CustomData/aptgss_AddAppendageHealthPoolCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, AddFactionReputationCommandDef> LoadAddFactionReputationCommandDef()
    {
        return LoadJSON<AddFactionReputationCommandDef>("./StaticDB/CustomData/aptgss_AddFactionReputationCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, AddLootTableCommandDef> LoadAddLootTableCommandDef()
    {
        return LoadJSON<AddLootTableCommandDef>("./StaticDB/CustomData/aptgss_AddLootTableCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, AbilitySlottedCommandDef> LoadAbilitySlottedCommandDef()
    {
        return LoadJSON<AbilitySlottedCommandDef>("./StaticDB/CustomData/aptgss_agsAbilitySlottedCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, ActivateMissionCommandDef> LoadActivateMissionCommandDef()
    {
        return LoadJSON<ActivateMissionCommandDef>("./StaticDB/CustomData/aptgss_agsActivateMissionCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, ActivateSpawnTableCommandDef> LoadActivateSpawnTableCommandDef()
    {
        return LoadJSON<ActivateSpawnTableCommandDef>("./StaticDB/CustomData/aptgss_agsActivateSpawnTableCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, CancelRopePullCommandDef> LoadCancelRopePullCommandDef()
    {
        return LoadJSON<CancelRopePullCommandDef>("./StaticDB/CustomData/aptgss_agsCancelRopePullCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, CreateAbilityObjectCommandDef> LoadCreateAbilityObjectCommandDef()
    {
        return LoadJSON<CreateAbilityObjectCommandDef>("./StaticDB/CustomData/aptgss_agsCreateAbilityObjectCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, DeployableUpgradeCommandDef> LoadDeployableUpgradeCommandDef()
    {
        return LoadJSON<DeployableUpgradeCommandDef>("./StaticDB/CustomData/aptgss_agsDeployableUpgradeCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, DestroyAbilityObjectCommandDef> LoadDestroyAbilityObjectCommandDef()
    {
        return LoadJSON<DestroyAbilityObjectCommandDef>("./StaticDB/CustomData/aptgss_agsDestroyAbilityObjectCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, EncounterSpawnCommandDef> LoadEncounterSpawnCommandDef()
    {
        return LoadJSON<EncounterSpawnCommandDef>("./StaticDB/CustomData/aptgss_agsEncounterSpawnCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, ExecuteCommandDef> LoadExecuteCommandDef()
    {
        return LoadJSON<ExecuteCommandDef>("./StaticDB/CustomData/aptgss_agsExecuteCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, InteractionCompletionTimeCommandDef> LoadInteractionCompletionTimeCommandDef()
    {
        return LoadJSON<InteractionCompletionTimeCommandDef>("./StaticDB/CustomData/aptgss_agsInteractionCompletionTimeCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, InteractionInProgressCommandDef> LoadInteractionInProgressCommandDef()
    {
        return LoadJSON<InteractionInProgressCommandDef>("./StaticDB/CustomData/aptgss_agsInteractionInProgressCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, InterruptCommandDef> LoadInterruptCommandDef()
    {
        return LoadJSON<InterruptCommandDef>("./StaticDB/CustomData/aptgss_agsInterruptCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, LifespanDurationCommandDef> LoadLifespanDurationCommandDef()
    {
        return LoadJSON<LifespanDurationCommandDef>("./StaticDB/CustomData/aptgss_agsLifespanDurationCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, MatchMakingQueueCommandDef> LoadMatchMakingQueueCommandDef()
    {
        return LoadJSON<MatchMakingQueueCommandDef>("./StaticDB/CustomData/aptgss_agsMatchMakingQueueCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, NPCBehaviorChangeCommandDef> LoadNPCBehaviorChangeCommandDef()
    {
        return LoadJSON<NPCBehaviorChangeCommandDef>("./StaticDB/CustomData/aptgss_agsNPCBehaviorChangeCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, NPCDespawnCommandDef> LoadNPCDespawnCommandDef()
    {
        return LoadJSON<NPCDespawnCommandDef>("./StaticDB/CustomData/aptgss_agsNPCDespawnCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, NPCDroidModeChangeCommandDef> LoadNPCDroidModeChangeCommandDef()
    {
        return LoadJSON<NPCDroidModeChangeCommandDef>("./StaticDB/CustomData/aptgss_agsNPCDroidModeChangeCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, NPCEquipMonsterCommandDef> LoadNPCEquipMonsterCommandDef()
    {
        return LoadJSON<NPCEquipMonsterCommandDef>("./StaticDB/CustomData/aptgss_agsNPCEquipMonsterCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, NPCSpawnCommandDef> LoadNPCSpawnCommandDef()
    {
        return LoadJSON<NPCSpawnCommandDef>("./StaticDB/CustomData/aptgss_agsNPCSpawnCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, RequireTryingToMoveCommandDef> LoadRequireTryingToMoveCommandDef()
    {
        return LoadJSON<RequireTryingToMoveCommandDef>("./StaticDB/CustomData/aptgss_agsRequireTryingToMoveCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, ResetTraumaCommandDef> LoadResetTraumaCommandDef()
    {
        return LoadJSON<ResetTraumaCommandDef>("./StaticDB/CustomData/aptgss_agsResetTraumaCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, RestockAmmoCommandDef> LoadRestockAmmoCommandDef()
    {
        return LoadJSON<RestockAmmoCommandDef>("./StaticDB/CustomData/aptgss_agsRestockAmmoCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, ReviveCommandDef> LoadReviveCommandDef()
    {
        return LoadJSON<ReviveCommandDef>("./StaticDB/CustomData/aptgss_agsReviveCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, RewardAssistCommandDef> LoadRewardAssistCommandDef()
    {
        return LoadJSON<RewardAssistCommandDef>("./StaticDB/CustomData/aptgss_agsRewardAssistCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, SetHostilityCommandDef> LoadSetHostilityCommandDef()
    {
        return LoadJSON<SetHostilityCommandDef>("./StaticDB/CustomData/aptgss_agsSetHostilityCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, SetHoverParametersCommandDef> LoadSetHoverParametersCommandDef()
    {
        return LoadJSON<SetHoverParametersCommandDef>("./StaticDB/CustomData/aptgss_agsSetHoverParametersCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, SetObjectLifespanCommandDef> LoadSetObjectLifespanCommandDef()
    {
        return LoadJSON<SetObjectLifespanCommandDef>("./StaticDB/CustomData/aptgss_agsSetObjectLifespanCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, SetOrientationCommandDef> LoadSetOrientationCommandDef()
    {
        return LoadJSON<SetOrientationCommandDef>("./StaticDB/CustomData/aptgss_agsSetOrientationCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, SetRespawnFlagsCommandDef> LoadSetRespawnFlagsCommandDef()
    {
        return LoadJSON<SetRespawnFlagsCommandDef>("./StaticDB/CustomData/aptgss_agsSetRespawnFlagsCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, SetYawCommandDef> LoadSetYawCommandDef()
    {
        return LoadJSON<SetYawCommandDef>("./StaticDB/CustomData/aptgss_agsSetYawCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, SinLinkRevealCommandDef> LoadSinLinkRevealCommandDef()
    {
        return LoadJSON<SinLinkRevealCommandDef>("./StaticDB/CustomData/aptgss_agsSinLinkRevealCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, SlotAbilityCommandDef> LoadSlotAbilityCommandDef()
    {
        return LoadJSON<SlotAbilityCommandDef>("./StaticDB/CustomData/aptgss_agsSlotAbilityCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, TargetByExistsCommandDef> LoadTargetByExistsCommandDef()
    {
        return LoadJSON<TargetByExistsCommandDef>("./StaticDB/CustomData/aptgss_agsTargetByExistsCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, TargetByNPCCommandDef> LoadTargetByNPCCommandDef()
    {
        return LoadJSON<TargetByNPCCommandDef>("./StaticDB/CustomData/aptgss_agsTargetByNPCCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, TargetByNPCTypeCommandDef> LoadTargetByNPCTypeCommandDef()
    {
        return LoadJSON<TargetByNPCTypeCommandDef>("./StaticDB/CustomData/aptgss_agsTargetByNPCTypeCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, TargetCharacterNPCsCommandDef> LoadTargetCharacterNPCsCommandDef()
    {
        return LoadJSON<TargetCharacterNPCsCommandDef>("./StaticDB/CustomData/aptgss_agsTargetCharacterNPCsCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, TargetTinyObjectCommandDef> LoadTargetTinyObjectCommandDef()
    {
        return LoadJSON<TargetTinyObjectCommandDef>("./StaticDB/CustomData/aptgss_agsTargetTinyObjectCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, TeleportInstanceCommandDef> LoadTeleportInstanceCommandDef()
    {
        return LoadJSON<TeleportInstanceCommandDef>("./StaticDB/CustomData/aptgss_agsTeleportInstanceCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, TinyObjectCreateCommandDef> LoadTinyObjectCreateCommandDef()
    {
        return LoadJSON<TinyObjectCreateCommandDef>("./StaticDB/CustomData/aptgss_agsTinyObjectCreateCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, TinyObjectDestroyCommandDef> LoadTinyObjectDestroyCommandDef()
    {
        return LoadJSON<TinyObjectDestroyCommandDef>("./StaticDB/CustomData/aptgss_agsTinyObjectDestroyCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, TinyObjectUpdateCommandDef> LoadTinyObjectUpdateCommandDef()
    {
        return LoadJSON<TinyObjectUpdateCommandDef>("./StaticDB/CustomData/aptgss_agsTinyObjectUpdateCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, TurretControlCommandDef> LoadTurretControlCommandDef()
    {
        return LoadJSON<TurretControlCommandDef>("./StaticDB/CustomData/aptgss_agsTurretControlCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, UpdateSpawnTableCommandDef> LoadUpdateSpawnTableCommandDef()
    {
        return LoadJSON<UpdateSpawnTableCommandDef>("./StaticDB/CustomData/aptgss_agsUpdateSpawnTableCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, ApplyPermanentEffectCommandDef> LoadApplyPermanentEffectCommandDef()
    {
        return LoadJSON<ApplyPermanentEffectCommandDef>("./StaticDB/CustomData/aptgss_ApplyPermanentEffectCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, ApplyUnlockCommandDef> LoadApplyUnlockCommandDef()
    {
        return LoadJSON<ApplyUnlockCommandDef>("./StaticDB/CustomData/aptgss_ApplyUnlockCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, AwardRedBeansCommandDef> LoadAwardRedBeansCommandDef()
    {
        return LoadJSON<AwardRedBeansCommandDef>("./StaticDB/CustomData/aptgss_AwardRedBeansCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, CalculateTrajectoryCommandDef> LoadCalculateTrajectoryCommandDef()
    {
        return LoadJSON<CalculateTrajectoryCommandDef>("./StaticDB/CustomData/aptgss_CalculateTrajectoryCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, CalldownVehicleCommandDef> LoadCalldownVehicleCommandDef()
    {
        return LoadJSON<CalldownVehicleCommandDef>("./StaticDB/CustomData/aptgss_CalldownVehicleCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, ClearHostilityCommandDef> LoadClearHostilityCommandDef()
    {
        return LoadJSON<ClearHostilityCommandDef>("./StaticDB/CustomData/aptgss_ClearHostilityCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, ConsumeItemCommandDef> LoadConsumeItemCommandDef()
    {
        return LoadJSON<ConsumeItemCommandDef>("./StaticDB/CustomData/aptgss_ConsumeItemCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, DamageFeedbackCommandDef> LoadDamageFeedbackCommandDef()
    {
        return LoadJSON<DamageFeedbackCommandDef>("./StaticDB/CustomData/aptgss_DamageFeedbackCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, DamageItemSlotCommandDef> LoadDamageItemSlotCommandDef()
    {
        return LoadJSON<DamageItemSlotCommandDef>("./StaticDB/CustomData/aptgss_DamageItemSlotCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, DropAllCarryableCommandDef> LoadDropAllCarryableCommandDef()
    {
        return LoadJSON<DropAllCarryableCommandDef>("./StaticDB/CustomData/aptgss_DropAllCarryableCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, DropCarryableCommandDef> LoadDropCarryableCommandDef()
    {
        return LoadJSON<DropCarryableCommandDef>("./StaticDB/CustomData/aptgss_DropCarryableCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, EnableInteractionCommandDef> LoadEnableInteractionCommandDef()
    {
        return LoadJSON<EnableInteractionCommandDef>("./StaticDB/CustomData/aptgss_EnableInteractionCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, EquipLoadoutCommandDef> LoadEquipLoadoutCommandDef()
    {
        return LoadJSON<EquipLoadoutCommandDef>("./StaticDB/CustomData/aptgss_EquipLoadoutCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, FallToGroundCommandDef> LoadFallToGroundCommandDef()
    {
        return LoadJSON<FallToGroundCommandDef>("./StaticDB/CustomData/aptgss_FallToGroundCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, ForceRespawnCommandDef> LoadForceRespawnCommandDef()
    {
        return LoadJSON<ForceRespawnCommandDef>("./StaticDB/CustomData/aptgss_ForceRespawnCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, GrantOwnerItemCommandDef> LoadGrantOwnerItemCommandDef()
    {
        return LoadJSON<GrantOwnerItemCommandDef>("./StaticDB/CustomData/aptgss_GrantOwnerItemCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, HostilityHackCommandDef> LoadHostilityHackCommandDef()
    {
        return LoadJSON<HostilityHackCommandDef>("./StaticDB/CustomData/aptgss_HostilityHackCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, HostilityOverrideCommandDef> LoadHostilityOverrideCommandDef()
    {
        return LoadJSON<HostilityOverrideCommandDef>("./StaticDB/CustomData/aptgss_HostilityOverrideCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, InflictHitFeedbackCommandDef> LoadInflictHitFeedbackCommandDef()
    {
        return LoadJSON<InflictHitFeedbackCommandDef>("./StaticDB/CustomData/aptgss_InflictHitFeedbackCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, ItemAttributeModifierCommandDef> LoadItemAttributeModifierCommandDef()
    {
        return LoadJSON<ItemAttributeModifierCommandDef>("./StaticDB/CustomData/aptgss_ItemAttributeModifierCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, MeldingBubbleCommandDef> LoadMeldingBubbleCommandDef()
    {
        return LoadJSON<MeldingBubbleCommandDef>("./StaticDB/CustomData/aptgss_MeldingBubbleCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, MindControlCommandDef> LoadMindControlCommandDef()
    {
        return LoadJSON<MindControlCommandDef>("./StaticDB/CustomData/aptgss_MindControlCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, ModifyDamageByFactionCommandDef> LoadModifyDamageByFactionCommandDef()
    {
        return LoadJSON<ModifyDamageByFactionCommandDef>("./StaticDB/CustomData/aptgss_ModifyDamageByFactionCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, ModifyDamageByHeadshotCommandDef> LoadModifyDamageByHeadshotCommandDef()
    {
        return LoadJSON<ModifyDamageByHeadshotCommandDef>("./StaticDB/CustomData/aptgss_ModifyDamageByHeadshotCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, ModifyDamageByMyHealthCommandDef> LoadModifyDamageByMyHealthCommandDef()
    {
        return LoadJSON<ModifyDamageByMyHealthCommandDef>("./StaticDB/CustomData/aptgss_ModifyDamageByMyHealthCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, ModifyDamageByTargetCommandDef> LoadModifyDamageByTargetCommandDef()
    {
        return LoadJSON<ModifyDamageByTargetCommandDef>("./StaticDB/CustomData/aptgss_ModifyDamageByTargetCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, ModifyDamageByTargetDamageResponseCommandDef> LoadModifyDamageByTargetDamageResponseCommandDef()
    {
        return LoadJSON<ModifyDamageByTargetDamageResponseCommandDef>("./StaticDB/CustomData/aptgss_ModifyDamageByTargetDamageResponseCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, ModifyDamageByTargetHealthCommandDef> LoadModifyDamageByTargetHealthCommandDef()
    {
        return LoadJSON<ModifyDamageByTargetHealthCommandDef>("./StaticDB/CustomData/aptgss_ModifyDamageByTargetHealthCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, ModifyDamageByTypeCommandDef> LoadModifyDamageByTypeCommandDef()
    {
        return LoadJSON<ModifyDamageByTypeCommandDef>("./StaticDB/CustomData/aptgss_ModifyDamageByTypeCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, ModifyDamageForInflictCommandDef> LoadModifyDamageForInflictCommandDef()
    {
        return LoadJSON<ModifyDamageForInflictCommandDef>("./StaticDB/CustomData/aptgss_ModifyDamageForInflictCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, ModifyHostilityCommandDef> LoadModifyHostilityCommandDef()
    {
        return LoadJSON<ModifyHostilityCommandDef>("./StaticDB/CustomData/aptgss_ModifyHostilityCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, ModifyOwnerResourcesCommandDef> LoadModifyOwnerResourcesCommandDef()
    {
        return LoadJSON<ModifyOwnerResourcesCommandDef>("./StaticDB/CustomData/aptgss_ModifyOwnerResourcesCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, NetworkStealthCommandDef> LoadNetworkStealthCommandDef()
    {
        return LoadJSON<NetworkStealthCommandDef>("./StaticDB/CustomData/aptgss_NetworkStealthCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, ReduceCooldownsCommandDef> LoadReduceCooldownsCommandDef()
    {
        return LoadJSON<ReduceCooldownsCommandDef>("./StaticDB/CustomData/aptgss_ReduceCooldownsCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, RegisterAbilityTriggerCommandDef> LoadRegisterAbilityTriggerCommandDef()
    {
        return LoadJSON<RegisterAbilityTriggerCommandDef>("./StaticDB/CustomData/aptgss_RegisterAbilityTriggerCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, RegisterEffectTagTriggerCommandDef> LoadRegisterEffectTagTriggerCommandDef()
    {
        return LoadJSON<RegisterEffectTagTriggerCommandDef>("./StaticDB/CustomData/aptgss_RegisterEffectTagTriggerCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, RegisterHitTagTypeTriggerCommandDef> LoadRegisterHitTagTypeTriggerCommandDef()
    {
        return LoadJSON<RegisterHitTagTypeTriggerCommandDef>("./StaticDB/CustomData/aptgss_RegisterHitTagTypeTriggerCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, RegisterTimedTriggerCommandDef> LoadRegisterTimedTriggerCommandDef()
    {
        return LoadJSON<RegisterTimedTriggerCommandDef>("./StaticDB/CustomData/aptgss_RegisterTimedTriggerCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, RemoteAbilityCallCommandDef> LoadRemoteAbilityCallCommandDef()
    {
        return LoadJSON<RemoteAbilityCallCommandDef>("./StaticDB/CustomData/aptgss_RemoteAbilityCallCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, RemoveEffectByTagCommandDef> LoadRemoveEffectByTagCommandDef()
    {
        return LoadJSON<RemoveEffectByTagCommandDef>("./StaticDB/CustomData/aptgss_RemoveEffectByTagCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, RemovePermanentEffectCommandDef> LoadRemovePermanentEffectCommandDef()
    {
        return LoadJSON<RemovePermanentEffectCommandDef>("./StaticDB/CustomData/aptgss_RemovePermanentEffectCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, ReplenishableDurationCommandDef> LoadReplenishableDurationCommandDef()
    {
        return LoadJSON<ReplenishableDurationCommandDef>("./StaticDB/CustomData/aptgss_ReplenishableDurationCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, ReplenishEffectDurationCommandDef> LoadReplenishEffectDurationCommandDef()
    {
        return LoadJSON<ReplenishEffectDurationCommandDef>("./StaticDB/CustomData/aptgss_ReplenishEffectDurationCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, RepositionClonesCommandDef> LoadRepositionClonesCommandDef()
    {
        return LoadJSON<RepositionClonesCommandDef>("./StaticDB/CustomData/aptgss_RepositionClonesCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, ReputationModifierCommandDef> LoadReputationModifierCommandDef()
    {
        return LoadJSON<ReputationModifierCommandDef>("./StaticDB/CustomData/aptgss_ReputationModifierCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, RequestArcJobsCommandDef> LoadRequestArcJobsCommandDef()
    {
        return LoadJSON<RequestArcJobsCommandDef>("./StaticDB/CustomData/aptgss_RequestArcJobsCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, RequireAbilityPhysicsCommandDef> LoadRequireAbilityPhysicsCommandDef()
    {
        return LoadJSON<RequireAbilityPhysicsCommandDef>("./StaticDB/CustomData/aptgss_RequireAbilityPhysicsCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, RequireAppliedUnlockCommandDef> LoadRequireAppliedUnlockCommandDef()
    {
        return LoadJSON<RequireAppliedUnlockCommandDef>("./StaticDB/CustomData/aptgss_RequireAppliedUnlockCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, RequireInitiatorExistsCommandDef> LoadRequireInitiatorExistsCommandDef()
    {
        return LoadJSON<RequireInitiatorExistsCommandDef>("./StaticDB/CustomData/aptgss_RequireInitiatorExistsCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, RequireLootStoreCommandDef> LoadRequireLootStoreCommandDef()
    {
        return LoadJSON<RequireLootStoreCommandDef>("./StaticDB/CustomData/aptgss_RequireLootStoreCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, ResetCooldownsCommandDef> LoadResetCooldownsCommandDef()
    {
        return LoadJSON<ResetCooldownsCommandDef>("./StaticDB/CustomData/aptgss_ResetCooldownsCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, ResourceNodeScanDefCommandDef> LoadResourceNodeScanDefCommandDef()
    {
        return LoadJSON<ResourceNodeScanDefCommandDef>("./StaticDB/CustomData/aptgss_ResourceNodeScanDefCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, SendTipMessageCommandDef> LoadSendTipMessageCommandDef()
    {
        return LoadJSON<SendTipMessageCommandDef>("./StaticDB/CustomData/aptgss_SendTipMessageCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, SetDefaultDamageBonusCommandDef> LoadSetDefaultDamageBonusCommandDef()
    {
        return LoadJSON<SetDefaultDamageBonusCommandDef>("./StaticDB/CustomData/aptgss_SetDefaultDamageBonusCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, SetGuardianCommandDef> LoadSetGuardianCommandDef()
    {
        return LoadJSON<SetGuardianCommandDef>("./StaticDB/CustomData/aptgss_SetGuardianCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, SetInteractionTypeCommandDef> LoadSetInteractionTypeCommandDef()
    {
        return LoadJSON<SetInteractionTypeCommandDef>("./StaticDB/CustomData/aptgss_SetInteractionTypeCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, SetLookAtTargetCommandDef> LoadSetLookAtTargetCommandDef()
    {
        return LoadJSON<SetLookAtTargetCommandDef>("./StaticDB/CustomData/aptgss_SetLookAtTargetCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, SetPoweredStateCommandDef> LoadSetPoweredStateCommandDef()
    {
        return LoadJSON<SetPoweredStateCommandDef>("./StaticDB/CustomData/aptgss_SetPoweredStateCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, SetScopeBubbleCommandDef> LoadSetScopeBubbleCommandDef()
    {
        return LoadJSON<SetScopeBubbleCommandDef>("./StaticDB/CustomData/aptgss_SetScopeBubbleCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, SetVisualInfoIndexCommandDef> LoadSetVisualInfoIndexCommandDef()
    {
        return LoadJSON<SetVisualInfoIndexCommandDef>("./StaticDB/CustomData/aptgss_SetVisualInfoIndexCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, ShoppingInvitationCommandDef> LoadShoppingInvitationCommandDef()
    {
        return LoadJSON<ShoppingInvitationCommandDef>("./StaticDB/CustomData/aptgss_ShoppingInvitationCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, ShowRewardScreenCommandDef> LoadShowRewardScreenCommandDef()
    {
        return LoadJSON<ShowRewardScreenCommandDef>("./StaticDB/CustomData/aptgss_ShowRewardScreenCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, SinAcquireCommandDef> LoadSinAcquireCommandDef()
    {
        return LoadJSON<SinAcquireCommandDef>("./StaticDB/CustomData/aptgss_SinAcquireCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, SpawnLootCommandDef> LoadSpawnLootCommandDef()
    {
        return LoadJSON<SpawnLootCommandDef>("./StaticDB/CustomData/aptgss_SpawnLootCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, StartArcCommandDef> LoadStartArcCommandDef()
    {
        return LoadJSON<StartArcCommandDef>("./StaticDB/CustomData/aptgss_StartArcCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, TargetAiTargetCommandDef> LoadTargetAiTargetCommandDef()
    {
        return LoadJSON<TargetAiTargetCommandDef>("./StaticDB/CustomData/aptgss_TargetAiTargetCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, TargetBySinVulnerableCommandDef> LoadTargetBySinVulnerableCommandDef()
    {
        return LoadJSON<TargetBySinVulnerableCommandDef>("./StaticDB/CustomData/aptgss_TargetBySinVulnerableCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, TargetMyTinyObjectsCommandDef> LoadTargetMyTinyObjectsCommandDef()
    {
        return LoadJSON<TargetMyTinyObjectsCommandDef>("./StaticDB/CustomData/aptgss_TargetMyTinyObjectsCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, TargetOwnedDeployablesCommandDef> LoadTargetOwnedDeployablesCommandDef()
    {
        return LoadJSON<TargetOwnedDeployablesCommandDef>("./StaticDB/CustomData/aptgss_TargetOwnedDeployablesCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, TauntCommandDef> LoadTauntCommandDef()
    {
        return LoadJSON<TauntCommandDef>("./StaticDB/CustomData/aptgss_TauntCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, TeleportCommandDef> LoadTeleportCommandDef()
    {
        return LoadJSON<TeleportCommandDef>("./StaticDB/CustomData/aptgss_TeleportCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, TemporaryEquipmentCommandDef> LoadTemporaryEquipmentCommandDef()
    {
        return LoadJSON<TemporaryEquipmentCommandDef>("./StaticDB/CustomData/aptgss_TemporaryEquipmentCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, TemporaryEquipmentStatMappingCommandDef> LoadTemporaryEquipmentStatMappingCommandDef()
    {
        return LoadJSON<TemporaryEquipmentStatMappingCommandDef>("./StaticDB/CustomData/aptgss_TemporaryEquipmentStatMappingCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, UnlockBattleframesCommandDef> LoadUnlockBattleframesCommandDef()
    {
        return LoadJSON<UnlockBattleframesCommandDef>("./StaticDB/CustomData/aptgss_UnlockBattleframesCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, UnlockCertsCommandDef> LoadUnlockCertsCommandDef()
    {
        return LoadJSON<UnlockCertsCommandDef>("./StaticDB/CustomData/aptgss_UnlockCertsCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, UnlockContentCommandDef> LoadUnlockContentCommandDef()
    {
        return LoadJSON<UnlockContentCommandDef>("./StaticDB/CustomData/aptgss_UnlockContentCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, UnlockDecalsCommandDef> LoadUnlockDecalsCommandDef()
    {
        return LoadJSON<UnlockDecalsCommandDef>("./StaticDB/CustomData/aptgss_UnlockDecalsCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, UnlockHeadAccessoriesCommandDef> LoadUnlockHeadAccessoriesCommandDef()
    {
        return LoadJSON<UnlockHeadAccessoriesCommandDef>("./StaticDB/CustomData/aptgss_UnlockHeadAccessoriesCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, UnlockOrnamentsCommandDef> LoadUnlockOrnamentsCommandDef()
    {
        return LoadJSON<UnlockOrnamentsCommandDef>("./StaticDB/CustomData/aptgss_UnlockOrnamentsCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, UnlockPatternsCommandDef> LoadUnlockPatternsCommandDef()
    {
        return LoadJSON<UnlockPatternsCommandDef>("./StaticDB/CustomData/aptgss_UnlockPatternsCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, UnlockTitlesCommandDef> LoadUnlockTitlesCommandDef()
    {
        return LoadJSON<UnlockTitlesCommandDef>("./StaticDB/CustomData/aptgss_UnlockTitlesCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, UnlockVisualOverridesCommandDef> LoadUnlockVisualOverridesCommandDef()
    {
        return LoadJSON<UnlockVisualOverridesCommandDef>("./StaticDB/CustomData/aptgss_UnlockVisualOverridesCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, UnlockWarpaintsCommandDef> LoadUnlockWarpaintsCommandDef()
    {
        return LoadJSON<UnlockWarpaintsCommandDef>("./StaticDB/CustomData/aptgss_UnlockWarpaintsCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, UnpackItemCommandDef> LoadUnpackItemCommandDef()
    {
        return LoadJSON<UnpackItemCommandDef>("./StaticDB/CustomData/aptgss_UnpackItemCommandDef.json")
            .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, Dictionary<uint, Deployable>> LoadDeployable() 
    {
        return LoadJSON<Deployable>("./StaticDB/CustomData/deployable.json")
        .GroupBy(row => row.ZoneId)
        .ToDictionary(group => group.Key, group => group.ToDictionary(row => row.Id, row => row));
    }

    public Dictionary<uint, Dictionary<uint, Melding>> LoadMelding() 
    {
        return LoadJSON<Melding>("./StaticDB/CustomData/melding.json")
        .GroupBy(row => row.ZoneId)
        .ToDictionary(group => group.Key, group => group.ToDictionary(row => row.Id, row => row));
    }

    public Dictionary<uint, Dictionary<uint, Outpost>> LoadOutpost() 
    {
        return LoadJSON<Outpost>("./StaticDB/CustomData/outpost.json")
        .GroupBy(row => row.ZoneId)
        .ToDictionary(group => group.Key, group => group.ToDictionary(row => row.Id, row => row));
    }

    private T[] LoadJSON<T>(string fileName)
    {
        string jsonString = File.ReadAllText(fileName);
        return JsonSerializer.Deserialize<T[]>(jsonString, SerializerOptions);
    }
}
