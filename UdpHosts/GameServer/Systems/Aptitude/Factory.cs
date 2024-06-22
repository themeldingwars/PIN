using System;
using System.Collections.Generic;
using System.Numerics;
using AeroMessages.GSS.V66.Character.Event;
using GameServer.Data.SDB;
using GameServer.Data.SDB.Records.apt;
using GameServer.Data.SDB.Records.aptfs;
using GameServer.Data.SDB.Records.customdata;
using GameServer.Enums.GSS.Character;

namespace GameServer.Aptitude;

public class Factory
{
    public Effect LoadEffect(uint effectId)
    {
        var statusEffectData = SDBInterface.GetStatusEffectData(effectId);
        var effect = new Effect()
        {
            Data = statusEffectData,
        };

        if (statusEffectData.ApplyChain != 0)
        {
            effect.ApplyChain = LoadChain(statusEffectData.ApplyChain);
        }

        if (statusEffectData.RemoveChain != 0)
        {
            effect.RemoveChain = LoadChain(statusEffectData.RemoveChain);
        }

        if (statusEffectData.UpdateChain != 0)
        {
            effect.UpdateChain = LoadChain(statusEffectData.UpdateChain);
        }

        if (statusEffectData.DurationChain != 0)
        {
            effect.DurationChain = LoadChain(statusEffectData.DurationChain);
        }

        return effect;
    }

    public Chain LoadChain(uint chainId)
    {
        var chain = new Chain();
        chain.Id = chainId;
        chain.Commands = new List<ICommand>();

        uint next = chainId;
        while (next != 0)
        {
            var baseCommandDef = SDBInterface.GetBaseCommandDef(next);
            var command = LoadCommand(baseCommandDef.Id, baseCommandDef.Subtype);
            chain.Commands.Add(command);
            next = baseCommandDef.Next;
        }

        if (chain.Commands.Count == 0)
        {
            Console.WriteLine($"Loaded empty chain {chainId}");
        }

        return chain;
    }

    public ICommand LoadCommand(uint commandId, uint typeId)
    {
        var commandTypeRec = SDBInterface.GetCommandType(typeId);
        var commandType = (CommandType)commandTypeRec.Id;

        // :) Fix this null terminator later
        if (commandTypeRec.Environment == "client\0")
        {
            // Far as I know we don't care about client commands on the server, though the params can be helpful.
            return new CustomNOOPCommand(commandType.ToString(), commandId);
        }

        // All command types having environment of either `both` or `server` were added below
        // If they're commented out they haven't been implemented yet
        // or have zero instances in SDB (for environment `both`) or BaseCommandDef (for environment `server`)
        switch ((CommandType)commandTypeRec.Id)
        {
            // case CommandType.ActiveInitiation:
            //     return new ActiveInitiationCommand();
            case CommandType.ImpactApplyEffect:
                return new ImpactApplyEffectCommand(SDBInterface.GetImpactApplyEffectCommandDef(commandId));
            case CommandType.InstantActivation:
                return new InstantActivationCommand(SDBInterface.GetInstantActivationCommandDef(commandId));
            // case CommandType.TargetFriendlies:
            //     return new TargetFriendliesCommand(SDBInterface.GetTargetFriendliesCommandDef(commandId));
            // case CommandType.TargetHostiles:
            //     return new TargetHostilesCommand(SDBInterface.GetTargetHostilesCommandDef(commandId));
            // case CommandType.TargetPBAE:
            //     return new TargetPBAECommand(SDBInterface.GetTargetPBAECommandDef(commandId));
            case CommandType.TargetSelf:
                return new TargetSelfCommand(SDBInterface.GetTargetSelfCommandDef(commandId));
            // case CommandType.TargetSingle:
            //     return new TargetSingleCommand(SDBInterface.GetTargetSingleCommandDef(commandId));
            // case CommandType.TimeCooldown:
            //     return new TimeCooldownCommand(SDBInterface.GetTimeCooldownCommandDef(commandId));
            // case CommandType.ImpactAura:
            //     Zero instances in BaseCommandDef
            case CommandType.ImpactRemoveEffect:
                return new ImpactRemoveEffectCommand(CustomDBInterface.GetImpactRemoveEffectCommandDef(commandId));
            // case CommandType.TimedActivation:
            //     return new TimedActivationCommand(SDBInterface.GetTimedActivationCommandDef(commandId));
            // case CommandType.TargetByEffect:
            //     return new TargetByEffectCommand(SDBInterface.GetTargetByEffectCommandDef(commandId));
            case CommandType.TargetClear:
                return new TargetClearCommand(SDBInterface.GetTargetClearCommandDef(commandId));
            // case CommandType.TargetConeAE:
            //     return new TargetConeAECommand(SDBInterface.GetTargetConeAECommandDef(commandId));
            case CommandType.StatModifier:
                return new StatModifierCommand(SDBInterface.GetStatModifierCommandDef(commandId));
            case CommandType.TimeDuration:
                return new TimeDurationCommand(SDBInterface.GetTimeDurationCommandDef(commandId));
            // case CommandType.PassiveInitiation:
            //     return new PassiveInitiationCommand(SDBInterface.GetPassiveInitiationCommandDef(commandId));
            case CommandType.StagedActivation:
                return new StagedActivationCommand(SDBInterface.GetStagedActivationCommandDef(commandId));
            // case CommandType.ActivationDuration:
            //     return new ActivationDurationCommand(SDBInterface.GetActivationDurationCommandDef(commandId));
            // case CommandType.TeleportInstance:
            //     return new TeleportInstanceCommand(CustomDBInterface.GetTeleportInstanceCommandDef(commandId));
            // case CommandType.ResetTrauma:
            //     return new ResetTraumaCommand(CustomDBInterface.GetResetTraumaCommandDef(commandId));
            case CommandType.TargetInitiator:
                return new TargetInitiatorCommand(SDBInterface.GetTargetInitiatorCommandDef(commandId));
            // case CommandType.Interrupt:
            //     return new InterruptCommand(CustomDBInterface.GetInterruptCommandDef(commandId));
            case CommandType.BeginInteraction:
                return new BeginInteractionCommand();
            case CommandType.EndInteraction:
                return new EndInteractionCommand();
            // case CommandType.TargetInteractives:
            //     return new TargetInteractivesCommand(SDBInterface.GetTargetInteractivesCommandDef(commandId));
            // case CommandType.ImpactMarkInteractives:
            //     return new ImpactMarkInteractivesCommand(SDBInterface.GetImpactMarkInteractivesCommandDef(commandId));
            case CommandType.TargetPrevious:
                return new TargetPreviousCommand(SDBInterface.GetTargetPreviousCommandDef(commandId));
            case CommandType.HasTargetsDuration:
                return new HasTargetsDurationCommand(SDBInterface.GetHasTargetsDurationCommandDef(commandId));
            // case CommandType.InflictDamage:
            //     return new InflictDamageCommand(SDBInterface.GetInflictDamageCommandDef(commandId));
            // case CommandType.CreateAbilityObject:
            //     return new CreateAbilityObjectCommand(CustomDBInterface.GetCreateAbilityObjectCommandDef(commandId));
            // case CommandType.DestroyAbilityObject:
            //     return new DestroyAbilityObjectCommand(CustomDBInterface.GetDestroyAbilityObjectCommandDef(commandId));
            // case CommandType.SetPosition:
            //     Zero instances in BaseCommandDef
            // case CommandType.SetOrientation:
            //     return new SetOrientationCommand(CustomDBInterface.GetSetOrientationCommandDef(commandId));
            // case CommandType.SetPitch:
            //     Zero instances in BaseCommandDef
            // case CommandType.SetYaw:
            //     return new SetYawCommand(CustomDBInterface.GetSetYawCommandDef(commandId));
            // case CommandType.SetObjectLifespan:
            //     return new SetObjectLifespan(CustomDBInterface.GetSetObjectLifespanCommandDef(commandId));
            // case CommandType.ExtendObjectLifespan:
            //     Zero instances in BaseCommandDef
            // case CommandType.LifespanDuration:
            //     return new LifespanDurationCommand(CustomDBInterface.GetLifespanDurationCommandDef(commandId));
            case CommandType.ForcePush:
                return new ForcePushCommand(SDBInterface.GetForcePushCommandDef(commandId));
            // case CommandType.UpdateYield:
            //     return new UpdateYieldCommand(SDBInterface.GetUpdateYieldCommandDef(commandId));
            case CommandType.AirborneDuration:
                return new AirborneDurationCommand(SDBInterface.GetAirborneDurationCommandDef(commandId));
            // case CommandType.CombatFlags:
            //     return new CombatFlagsCommand(SDBInterface.GetCombatFlagsCommandDef(commandId));
            // case CommandType.RequestEffect:
            //     Zero instances in BaseCommandDef
            case CommandType.RequireCState:
                return new RequireCStateCommand(SDBInterface.GetRequireCStateCommandDef(commandId));
            case CommandType.RequireSprintModifier:
                return new RequireSprintModifierCommand(SDBInterface.GetRequireSprintModifierCommandDef(commandId));
            // case CommandType.RopePull:
            //     return new RopePullCommand(SDBInterface.GetRopePullCommandDef(commandId));
            // case CommandType.SetTargetOffset:
            //     return new SetTargetOffsetCommand(SDBInterface.GetSetTargetOffsetCommandDef(commandId));
            // case CommandType.RequireEnergy:
            //     return new RequireEnergyCommand(SDBInterface.GetRequireEnergyCommandDef(commandId));
            // case CommandType.HealDamage:
            //     return new HealDamageCommand(SDBInterface.GetHealDamageCommandDef(commandId));
            // case CommandType.Bullrush:
            //     return new BullrushCommand(SDBInterface.GetBullrushCommandDef(commandId));
            // case CommandType.EnergyToDamage:
            //     return new EnergyToDamageCommand(SDBInterface.GetEnergyToDamageCommandDef(commandId));
            // case CommandType.RequireGrapple:
            //     Zero instances in BaseCommandDef
            // case CommandType.RequireAbilityObject:
            //     Zero instances in BaseCommandDef
            case CommandType.RequireMoving:
                return new RequireMovingCommand(SDBInterface.GetRequireMovingCommandDef(commandId));
            // case CommandType.RequireTryingToMove:
            //     return new RequireTryingToMoveCommand(CustomDBInterface.GetRequireTryingToMoveCommandDef(commandId));
            // case CommandType.RequireInRange:
            //     return new RequireInRangeCommand(SDBInterface.GetRequireInRangeCommandDef(commandId));
            // case CommandType.TinyObjectCreate:
            //     return new TinyObjectCreateCommand(CustomDBInterface.GetTinyObjectCreateCommandDef(commandId));
            // case CommandType.TinyObjectDestroy:
            //     return new TinyObjectDestroyCommand(CustomDBInterface.GetTinyObjectDestroyCommandDef(commandId));
            // case CommandType.TinyObjectUpdate:
            //     return new TinyObjectUpdateCommand(CustomDBInterface.GetTinyObjectUpdateCommandDef(commandId));
            // case CommandType.TargetTinyObject:
            //     return new TargetTinyObjectCommand(CustomDBInterface.GetTargetTinyObjectCommandDef(commandId));
            // case CommandType.RequireGrappleAttached:
            //     Zero instances in BaseCommandDef
            // case CommandType.CancelRopePull:
            //     return new CancelRopePullCommand(CustomDBInterface.GetCancelRopePullCommandDef(commandId));
            case CommandType.RequestBattleFrameList:
                return new RequestBattleFrameListCommand();
            // case CommandType.NPCSpawn:
            //     return new NPCSpawnCommand(CustomDBInterface.GetNPCSpawnCommandDef(commandId));
            // case CommandType.ApplyImpulse:
            //     return new ApplyImpulseCommand(SDBInterface.GetApplyImpulseCommandDef(commandId));
            case CommandType.DeployableSpawn:
                return new DeployableSpawnCommand(CustomDBInterface.GetDeployableSpawnCommandDef(commandId));
            // case CommandType.NPCDroidModeChange:
            //     return new NPCDroidModeChangeCommand(CustomDBInterface.GetNPCDroidModeChangeCommandDef(commandId));
            // case CommandType.BattleFrameDuration:
            //     return new BattleFrameDurationCommand(SDBInterface.GetBattleFrameDurationCommandDef(commandId));
            // case CommandType.ShootingDuration:
            //     return new ShootingDurationCommand(SDBInterface.GetShootingDurationCommandDef(commandId));
            // case CommandType.RequireWeaponTemplate:
            //     return new RequireWeaponTemplateCommand(SDBInterface.GetRequireWeaponTemplateCommandDef(commandId));
            // case CommandType.SwitchWeapon:
            //     return new SwitchWeaponCommand(SDBInterface.GetSwitchWeaponCommandDef(commandId));
            // case CommandType.StatRequirement:
            //     return new StatRequirementCommand(SDBInterface.GetStatRequirementCommandDef(commandId));
            // case CommandType.ConsumeEnergy:
            //     return new ConsumeEnergyCommand(SDBInterface.GetConsumeEnergyCommandDef(commandId));
            // case CommandType.TargetClassType:
            //     return new TargetClassTypeCommand(SDBInterface.GetTargetClassTypeCommandDef(commandId));
            // case CommandType.TargetDifference:
            //     return new TargetDifferenceCommand(SDBInterface.GetTargetDifferenceCommandDef(commandId));
            case CommandType.ConditionalBranch:
                return new ConditionalBranchCommand(SDBInterface.GetConditionalBranchCommandDef(commandId));
            case CommandType.LogicOr:
                return new LogicOrCommand(SDBInterface.GetLogicOrCommandDef(commandId));
            case CommandType.LogicNegate:
                return new LogicNegateCommand(SDBInterface.GetLogicNegateCommandDef(commandId));
            // case CommandType.Return:
            //     return new ReturnCommand(SDBInterface.GetReturnCommandDef(commandId));
            case CommandType.Call:
                return new CallCommand(SDBInterface.GetCallCommandDef(commandId));
            case CommandType.PushTargets:
                return new PushTargetsCommand(SDBInterface.GetPushTargetsCommandDef(commandId));
            case CommandType.PopTargets:
                return new PopTargetsCommand(SDBInterface.GetPopTargetsCommandDef(commandId));
            case CommandType.PeekTargets:
                return new PeekTargetsCommand(SDBInterface.GetPeekTargetsCommandDef(commandId));
            case CommandType.RequirementServer:
                return new RequirementServerCommand(SDBInterface.GetRequirementServerCommandDef(commandId));
            // case CommandType.FireProjectile:
            //     return new FireProjectileCommand(SDBInterface.GetFireProjectileCommandDef(commandId));
            case CommandType.ApplyFreeze:
                return new ApplyFreezeCommand(SDBInterface.GetApplyFreezeCommandDef(commandId));
            // case CommandType.ClimbLedge:
            //     return new ClimbLedgeCommand(SDBInterface.GetClimbLedgeCommandDef(commandId));
            case CommandType.TargetByObjectType:
                return new TargetByObjectTypeCommand(SDBInterface.GetTargetByObjectTypeCommandDef(commandId));
            // case CommandType.AimRangeDuration:
            //     return new AimRangeDurationCommand(SDBInterface.GetAimRangeDurationCommandDef(commandId));
            // case CommandType.SinAcquire:
            //     return new SinAcquireCommand(CustomDBInterface.GetSinAcquireCommandDef(commandId));
            case CommandType.TargetByCharacterState:
                return new TargetByCharacterStateCommand(SDBInterface.GetTargetByCharacterStateCommandDef(commandId));
            // case CommandType.RequireLineOfSight:
            //     return new RequireLineOfSightCommand(SDBInterface.GetRequireLineOfSightCommandDef(commandId));
            // case CommandType.CopyInitiationPosition:
            //     return new CopyInitiationPositionCommand(SDBInterface.GetCopyInitiationPositionCommandDef(commandId));
            // case CommandType.RequireLevel:
            //     return new RequireLevelCommand(SDBInterface.GetRequireLevelCommandDef(commandId));
            // case CommandType.RequireJumped:
            //     return new RequireJumpedCommand(SDBInterface.GetRequireJumpedCommandDef(commandId));
            // case CommandType.RequireProjectileSlope:
            //     return new RequireProjectileSlopeCommand(SDBInterface.GetRequireProjectileSlopeCommandDef(commandId));
            // case CommandType.CreateSpawnPoint:
            //     return new CreateSpawnPointCommand(CustomDBInterface.GetCreateSpawnPointCommandDef(commandId));
            // case CommandType.TargetCharacterNPCs:
            //     return new TargetCharacterNPCsCommand(CustomDBInterface.GetTargetCharacterNPCsCommandDef(commandId));
            // case CommandType.NPCBehaviorChange:
            //     return new NPCBehaviorChangeCommand(CustomDBInterface.GetNPCBehaviorChangeCommandDef(commandId));
            // case CommandType.RequireAimMode:
            //     return new RequireAimModeCommand(SDBInterface.GetRequireAimModeCommandDef(commandId));
            // case CommandType.SlotAmmo:
            //     return new SlotAmmoCommand(SDBInterface.GetSlotAmmoCommandDef(commandId));
            // case CommandType.AddPhysics:
            //     return new AddPhysicsCommand(SDBInterface.GetAddPhysicsCommandDef(commandId));
            // case CommandType.RequireReload:
            //     return new RequireReloadCommand(SDBInterface.GetRequireReloadCommandDef(commandId));
            // case CommandType.TargetByExists:
            //     return new TargetByExistsCommand(CustomDBInterface.GetTargetByExistsCommandDef(commandId));
            case CommandType.InteractionType:
                return new InteractionTypeCommand(SDBInterface.GetInteractionTypeCommandDef(commandId));
            case CommandType.TargetStackEmpty:
                return new TargetStackEmptyCommand(SDBInterface.GetTargetStackEmptyCommandDef(commandId));
            // case CommandType.InteractionInProgress:
            //     return new InteractionInProgressCommand(CustomDBInterface.GetInteractionInProgressCommandDef(commandId));
            // case CommandType.InteractionCompletionTime:
            //     return new InteractionCompletionTimeCommand(CustomDBInterface.GetInteractionCompletionTimeCommandDef(commandId));
            // case CommandType.Execute:
            //     return new ExecuteCommand(CustomDBInterface.GetExecuteCommandDef(commandId));
            // case CommandType.Revive:
            //     return new ReviveCommand(CustomDBInterface.GetReviveCommandDef(commandId));
            case CommandType.RequireHasEffect:
                return new RequireHasEffectCommand(SDBInterface.GetRequireHasEffectCommandDef(commandId));
            case CommandType.RequireInVehicle:
                return new RequireInVehicleCommand(SDBInterface.GetRequireInVehicleCommandDef(commandId));
            case CommandType.TargetCurrentVehicle:
                return new TargetCurrentVehicleCommand(SDBInterface.GetTargetCurrentVehicleCommandDef(commandId));
            case CommandType.TargetOwner:
                return new TargetOwnerCommand(SDBInterface.GetTargetOwnerCommandDef(commandId));
            // case CommandType.RequireTookDamage:
            //     return new RequireTookDamageCommand(SDBInterface.GetRequireTookDamageCommandDef(commandId));
            // case CommandType.ModifyOwnerResources:
            //     return new ModifyOwnerResourcesCommand(CustomDBInterface.GetModifyOwnerResourcesCommandDef(commandId));
            case CommandType.ModifyPermission:
                return new ModifyPermissionCommand(CustomDBInterface.GetModifyPermissionCommandDef(commandId));
            // case CommandType.RequirePermission:
            //     return new RequirePermissionCommand(SDBInterface.GetRequirePermissionCommandDef(commandId));
            // case CommandType.TargetPassengers:
            //     return new TargetPassengersCommand(SDBInterface.GetTargetPassengersCommandDef(commandId));
            // case CommandType.TargetSquadmates:
            //     return new TargetSquadmatesCommand(SDBInterface.GetTargetSquadmatesCommandDef(commandId));
            // case CommandType.SlotAbility:
            //     return new SlotAbilityCommand(CustomDBInterface.GetSlotAbilityCommandDef(commandId));
            case CommandType.TargetTrim:
                return new TargetTrimCommand(SDBInterface.GetTargetTrimCommandDef(commandId));
            // case CommandType.SetWeaponDamage:
            //     return new SetWeaponDamageCommand(SDBInterface.GetSetWeaponDamageCommandDef(commandId));
            // case CommandType.ConsumeEnergyOverTime:
            //     return new ConsumeEnergyOverTimeCommand(SDBInterface.GetConsumeEnergyOverTimeCommandDef(commandId));
            // case CommandType.RequestAbilitySelection:
            //     return new RequestAbilitySelectionCommand(SDBInterface.GetRequestAbilitySelectionCommandDef(commandId));
            // case CommandType.NPCDespawn:
            //     return new NPCDespawnCommand(CustomDBInterface.GetNPCDespawnCommandDef(commandId));
            // case CommandType.RestockAmmo:
            //     return new RestockAmmoCommand(CustomDBInterface.GetRestockAmmoCommandDef(commandId));
            case CommandType.SetRegister:
                return new SetRegisterCommand(SDBInterface.GetSetRegisterCommandDef(commandId));
            // case CommandType.LoadRegisterFromBonus:
            //     return new LoadRegisterFromBonusCommand(SDBInterface.GetLoadRegisterFromBonusCommandDef(commandId));
            // case CommandType.BonusGreaterThan:
            //     SDB has zero instances of this command
            // case CommandType.TargetByNPC:
            //     return new TargetByNPCCommand(CustomDBInterface.GetTargetByNPCCommandDef(commandId));
            case CommandType.ImpactToggleEffect:
                return new ImpactToggleEffectCommand(SDBInterface.GetImpactToggleEffectCommandDef(commandId));
            case CommandType.DeployableCalldown:
                return new DeployableCalldownCommand(SDBInterface.GetDeployableCalldownCommandDef(commandId));
            // case CommandType.TurretControl:
            //     return new TurretControlCommand(CustomDBInterface.GetTurretControlCommandDef(commandId));
            // case CommandType.Bombardment:
            //     return new BombardmentCommand(SDBInterface.GetBombardmentCommandDef(commandId));
            // case CommandType.RequireResource:
            //     return new RequireResourceCommand(SDBInterface.GetRequireResourceCommandDef(commandId));
            // case CommandType.InflictCooldown:
            //     return new InflictCooldownCommand(SDBInterface.GetInflictCooldownCommandDef(commandId));
            case CommandType.RequireMovestate:
                return new RequireMovestateCommand(SDBInterface.GetRequireMovestateCommandDef(commandId));
            // case CommandType.GrantOwnerItem:
            //     return new GrantOwnerItemCommand(CustomDBInterface.GetGrantOwnerItemCommandDef(commandId));
            // case CommandType.ShoppingInvitation:
            //     return new ShoppingInvitationCommand(CustomDBInterface.GetShoppingInvitationCommandDef(commandId));
            // case CommandType.SetGuardian:
            //     return new SetGuardianCommand(CustomDBInterface.GetSetGuardianCommandDef(commandId));
            // case CommandType.DamageFeedback:
            //     return new DamageFeedbackCommand(CustomDBInterface.GetDamageFeedbackCommandDef(commandId));
            // case CommandType.RequireBackstab:
            //     return new RequireBackstabCommand(SDBInterface.GetRequireBackstabCommandDef(commandId));
            // case CommandType.CalldownVehicle:
            //     return new CalldownVehicleCommand(CustomDBInterface.GetCalldownVehicleCommandDef(commandId));
            // case CommandType.SetProjectileTarget:
            //     return new SetProjectileTargetCommand(SDBInterface.GetSetProjectileTargetCommandDef(commandId));
            // case CommandType.SetScopeBubble:
            //     return new SetScopeBubbleCommand(CustomDBInterface.GetSetScopeBubbleCommandDef(commandId));
            // case CommandType.MeldingBubble:
            //     return new MeldingBubbleCommand(CustomDBInterface.GetMeldingBubbleCommandDef(commandId));
            // case CommandType.MindControl:
            //     return new MindControlCommand(CustomDBInterface.GetMindControlCommandDef(commandId));
            // case CommandType.RequireEnergyFromTarget:
            //     Zero instances in BaseCommandDef
            // case CommandType.RequireResourceFromTarget:
            //     has environment `server` but is in SDB
            //     return new RequireResourceFromTargetCommand(SDBInterface.GetRequireResourceFromTargetCommandDef(commandId));
            // case CommandType.SpawnLoot:
            //     return new SpawnLootCommand(CustomDBInterface.GetSpawnLootCommandDef(commandId));
            // case CommandType.AbilitySlotted:
            //     return new AbilitySlottedCommand(CustomDBInterface.GetAbilitySlottedCommandDef(commandId));
            // case CommandType.LoadRegisterFromResource:
            //     return new LoadRegisterFromResourceCommand(SDBInterface.GetLoadRegisterFromResourceCommandDef(commandId));
            // case CommandType.SetLookAtTarget:
            //     return new SetLookAtTargetCommand(CustomDBInterface.GetSetLookAtTargetCommandDef(commandId));
            case CommandType.EncounterSpawn:
                return new EncounterSpawnCommand(CustomDBInterface.GetEncounterSpawnCommandDef(commandId));
            // case CommandType.MatchMakingQueue:
            //     return new MatchMakingQueueCommand(CustomDBInterface.GetMatchMakingQueueCommandDef(commandId));
            // case CommandType.ActivateMission:
            //     return new ActivateMissionCommand(CustomDBInterface.GetActivateMissionCommandDef(commandId));
            // case CommandType.UpdateWait:
            //     return new UpdateWaitCommand(SDBInterface.GetUpdateWaitCommandDef(commandId));
            // case CommandType.LoadRegisterFromStat:
            //     return new LoadRegisterFromStatCommand(SDBInterface.GetLoadRegisterFromStatCommandDef(commandId));
            // case CommandType.PushRegister:
            //     return new PushRegisterCommand(SDBInterface.GetPushRegisterCommandDef(commandId));
            // case CommandType.PopRegister:
            //     return new PopRegisterCommand(SDBInterface.GetPopRegisterCommandDef(commandId));
            // case CommandType.PeekRegister:
            //     return new PeekRegisterCommand(SDBInterface.GetPeekRegisterCommandDef(commandId));
            case CommandType.WhileLoop:
                return new WhileLoopCommand(SDBInterface.GetWhileLoopCommandDef(commandId));
            // case CommandType.MovementSlide:
            //     return new MovementSlideCommand(SDBInterface.GetMovementSlideCommandDef(commandId));
            // case CommandType.RequireEnergyByRange:
            //     return new RequireEnergyByRangeCommand(SDBInterface.GetRequireEnergyByRangeCommandDef(commandId));
            // case CommandType.NetworkStealth:
            //     return new NetworkStealthCommand(CustomDBInterface.GetNetworkStealthCommandDef(commandId));
            // case CommandType.ResetCooldowns:
            //     return new ResetCooldownsCommand(CustomDBInterface.GetResetCooldownsCommandDef(commandId));
            // case CommandType.RewardAssist:
            //     return new RewardAssistCommand(CustomDBInterface.GetRewardAssistCommandDef(commandId));
            // case CommandType.DeployableUpgrade:
            //     return new DeployableUpgradeCommand(CustomDBInterface.GetDeployableUpgradeCommandDef(commandId));
            // case CommandType.NPCEquipMonster:
            //     return new NPCEquipMonsterCommand(CustomDBInterface.GetNPCEquipMonsterCommandDef(commandId));
            // case CommandType.RequireArmy:
            //     return new RequireArmyCommand(SDBInterface.GetRequireArmyCommandDef(commandId));
            // case CommandType.SetHostility:
            //     return new SetHostilityCommand(CustomDBInterface.GetSetHostilityCommandDef(commandId));
            // case CommandType.Teleport:
            //     return new TeleportCommand(CustomDBInterface.GetTeleportCommandDef(commandId));
            // case CommandType.TargetFromStatusEffect:
            //     return new TargetFromStatusEffectCommand(SDBInterface.GetTargetFromStatusEffectCommandDef(commandId));
            // case CommandType.TemporaryEquipment:
            //     return new TemporaryEquipmentCommand(CustomDBInterface.GetTemporaryEquipmentCommandDef(commandId));
            // case CommandType.RequireDamageResponse:
            //     return new RequireDamageResponseCommand(SDBInterface.GetRequireDamageResponseCommandDef(commandId));
            // case CommandType.TargetByDamageResponse:
            //     return new TargetByDamageResponseCommand(SDBInterface.GetTargetByDamageResponseCommandDef(commandId));
            case CommandType.OrientationLock:
                return new OrientationLockCommand(SDBInterface.GetOrientationLockCommandDef(commandId));
            // case CommandType.LoadRegisterFromModulePower:
            //     return new LoadRegisterFromModulePowerCommand(SDBInterface.GetLoadRegisterFromModulePowerCommandDef(commandId));
            // case CommandType.ForcedMovementDuration:
            //     return new ForcedMovementDurationCommand(SDBInterface.GetForcedMovementDurationCommandDef(commandId));
            // case CommandType.ActivateSpawnTable:
            //     return new ActivateSpawnTableCommand(CustomDBInterface.GetActivateSpawnTableCommandDef(commandId));
            // case CommandType.SinLinkReveal:
            //     return new SinLinkRevealCommand(CustomDBInterface.GetSinLinkRevealCommandDef(commandId));
            // case CommandType.SinLinkUnlock:
            //     Zero instances in BaseCommandDef
            // case CommandType.ResourceNodeScanDef:
            //     return new ResourceNodeScanDefCommand(CustomDBInterface.GetResourceNodeScanDefCommandDef(commandId));
            case CommandType.ResourceNodeBeaconCalldown:
                return new ResourceNodeBeaconCalldownCommand(SDBInterface.GetResourceNodeBeaconCalldownCommandDef(commandId));
            // case CommandType.SendTipMessage:
            //     return new SendTipMessageCommand(CustomDBInterface.GetSendTipMessageCommandDef(commandId));
            // case CommandType.FallToGround:
            //     return new FallToGroundCommand(CustomDBInterface.GetFallToGroundCommandDef(commandId));
            // case CommandType.BulletTime:
            //     Zero instances in BaseCommandDef
            // case CommandType.SetPoweredState:
            //     return new SetPoweredStateCommand(CustomDBInterface.GetSetPoweredStateCommandDef(commandId));
            // case CommandType.NamedVariableAssign:
            //     has environment `server` but is in SDB
            //     return new NamedVariableAssignCommand(SDBInterface.GetNamedVariableAssignCommandDef(commandId));
            // case CommandType.LoadRegisterFromNamedVar:
            //     return new LoadRegisterFromNamedVarCommand(SDBInterface.GetLoadRegisterFromNamedVarCommandDef(commandId));
            // case CommandType.FireUiEvent:
            //     return new FireUiEventCommand(SDBInterface.GetFireUiEventCommandDef(commandId));
            // case CommandType.CalculateTrajectory:
            //     return new CalculateTrajectoryCommand(CustomDBInterface.GetCalculateTrajectoryCommandDef(commandId));
            case CommandType.RegisterComparison:
                return new RegisterComparisonCommand(SDBInterface.GetRegisterComparisonCommandDef(commandId));
            // case CommandType.ConsumeItem:
            //     return new ConsumeItemCommand(CustomDBInterface.GetConsumeItemCommandDef(commandId));
            case CommandType.RegisterRandom:
                return new RegisterRandomCommand(SDBInterface.GetRegisterRandomCommandDef(commandId));
            case CommandType.SetGliderParametersDef:
                return new SetGliderParametersCommand(CustomDBInterface.GetSetGliderParametersCommandDef(commandId));
            // case CommandType.SetHoverParametersDef:
            //     return new SetHoverParametersCommand(CustomDBInterface.GetSetHoverParametersCommandDef(commandId));
            // case CommandType.SetVisualInfoIndex:
            //     return new SetVisualInfoIndexCommand(CustomDBInterface.GetSetVisualInfoIndexCommandDef(commandId));
            // case CommandType.UiNamedVariable:
            //     return new UiNamedVariableCommand(SDBInterface.GetUiNamedVariableCommandDef(commandId));
            // case CommandType.SetRespawnFlags:
            //     return new SetRespawnFlagsCommand(CustomDBInterface.GetSetRespawnFlagsCommandDef(commandId));
            case CommandType.VehicleCalldown:
                return new VehicleCalldownCommand(SDBInterface.GetVehicleCalldownCommandDef(commandId));
            case CommandType.EncounterSignal:
                return new EncounterSignalCommand(CustomDBInterface.GetEncounterSignalCommandDef(commandId));
            // case CommandType.RequireNeedsAmmo:
            //     return new RequireNeedsAmmoCommand(SDBInterface.GetRequireNeedsAmmoCommandDef(commandId));
            // case CommandType.TargetByNPCType:
            //     return new TargetByNPCTypeCommand(CustomDBInterface.GetTargetByNPCTypeCommandDef(commandId));
            case CommandType.LoadRegisterFromItemStat:
                return new LoadRegisterFromItemStatCommand(SDBInterface.GetLoadRegisterFromItemStatCommandDef(commandId));
            // case CommandType.HostilityHack:
            //     return new HostilityHackCommand(CustomDBInterface.GetHostilityHackCommandDef(commandId));
            // case CommandType.DetonateProjectiles:
            //     return new DetonateProjectilesCommand(SDBInterface.GetDetonateProjectilesCommandDef(commandId));
            // case CommandType.RequireBulletHit:
            //     return new RequireBulletHitCommand(SDBInterface.GetRequireBulletHitCommandDef(commandId));
            // case CommandType.LoadRegisterFromDamage:
            //     return new LoadRegisterFromDamageCommand(SDBInterface.GetLoadRegisterFromDamageCommandDef(commandId));
            case CommandType.TargetSwap:
                return new TargetSwapCommand(SDBInterface.GetTargetSwapCommandDef(commandId));
            // case CommandType.ApplyPermanentEffect:
            //     return new ApplyPermanentEffectCommand(CustomDBInterface.GetApplyPermanentEffectCommandDef(commandId));
            // case CommandType.ModifyHostility:
            //     return new ModifyHostilityCommand(CustomDBInterface.GetModifyHostilityCommandDef(commandId));
            // case CommandType.RegisterAbilityTrigger:
            //     return new RegisterAbilityTriggerCommand(CustomDBInterface.GetRegisterAbilityTriggerCommandDef(commandId));
            // case CommandType.SetWeaponDamageType:
            //     return new SetWeaponDamageTypeCommand(SDBInterface.GetSetWeaponDamageTypeCommandDef(commandId));
            // case CommandType.RequireNotRespawned:
            //     return new RequireNotRespawnedCommand(SDBInterface.GetRequireNotRespawnedCommandDef(commandId));
            // case CommandType.AbilityFinished:
            //     return new AbilityFinishedCommand(CustomDBInterface.GetAbilityFinishedCommandDef(commandId));
            case CommandType.TargetFilterMovestate:
                return new TargetFilterMovestateCommand(SDBInterface.GetTargetFilterMovestateCommandDef(commandId));
            // case CommandType.RequireAbilityPhysics:
            //     return new RequireAbilityPhysicsCommand(CustomDBInterface.GetRequireAbilityPhysicsCommandDef(commandId));
            // case CommandType.ClearHostility:
            //     return new ClearHostilityCommand(CustomDBInterface.GetClearHostilityCommandDef(commandId));
            // case CommandType.UpdateSpawnTable:
            //     return new UpdateSpawnTableCommand(CustomDBInterface.GetUpdateSpawnTableCommandDef(commandId));
            // case CommandType.TargetByHostility:
            //     return new TargetByHostilityCommand(SDBInterface.GetTargetByHostilityCommandDef(commandId));
            // case CommandType.RegisterClientProximity:
            //     return new RegisterClientProximityCommand(SDBInterface.GetRegisterClientProximityCommandDef(commandId));
            case CommandType.ApplySinCard:
                return new ApplySinCardCommand(CustomDBInterface.GetApplySinCardCommandDef(commandId));
            // case CommandType.UnlockOrnaments:
            //     return new UnlockOrnamentsCommand(CustomDBInterface.GetUnlockOrnamentsCommandDef(commandId));
            // case CommandType.DropCarryable:
            //     return new DropCarryableCommand(CustomDBInterface.GetDropCarryableCommandDef(commandId));
            // case CommandType.RequireSinAcquired:
            //     return new RequireSinAcquiredCommand(SDBInterface.GetRequireSinAcquiredCommandDef(commandId));
            // case CommandType.EquipLoadout:
            //     return new EquipLoadoutCommand(CustomDBInterface.GetEquipLoadoutCommandDef(commandId));
            case CommandType.RequireHasEffectTag:
                return new RequireHasEffectTagCommand(SDBInterface.GetRequireHasEffectTagCommandDef(commandId));
            // case CommandType.TargetByEffectTag:
            //     return new TargetByEffectTagCommand(SDBInterface.GetTargetByEffectTagCommandDef(commandId));
            // case CommandType.RemoveEffectByTag:
            //     return new RemoveEffectByTagCommand(CustomDBInterface.GetRemoveEffectByTagCommandDef(commandId));
            // case CommandType.RegisterEffectTagTrigger:
            //     return new RegisterEffectTagTriggerCommand(CustomDBInterface.GetRegisterEffectTagTriggerCommandDef(commandId));
            // case CommandType.ReplenishableDuration:
            //     return new ReplenishableDurationCommand(CustomDBInterface.GetReplenishableDurationCommandDef(commandId));
            // case CommandType.ReplenishEffectDuration:
            //     return new ReplenishEffectDurationCommand(CustomDBInterface.GetReplenishEffectDurationCommandDef(commandId));
            // case CommandType.ConsumeSuperCharge:
            //     return new ConsumeSuperChargeCommand(SDBInterface.GetConsumeSuperChargeCommandDef(commandId));
            // case CommandType.RequireSuperCharge:
            //     return new RequireSuperChargeCommand(SDBInterface.GetRequireSuperChargeCommandDef(commandId));
            // case CommandType.ActivateAbilityTrigger:
            //     return new ActivateAbilityTriggerCommand(CustomDBInterface.GetActivateAbilityTriggerCommandDef(commandId));
            // case CommandType.TargetByHealth:
            //     return new TargetByHealthCommand(SDBInterface.GetTargetByHealthCommandDef(commandId));
            // case CommandType.RegisterHitTagTypeTrigger:
            //     return new RegisterHitTagTypeTriggerCommand(CustomDBInterface.GetRegisterHitTagTypeTriggerCommandDef(commandId));
            case CommandType.LogicOrChain:
                return new LogicOrChainCommand(SDBInterface.GetLogicOrChainCommandDef(commandId));
            case CommandType.LogicAndChain:
                return new LogicAndChainCommand(SDBInterface.GetLogicAndChainCommandDef(commandId));
            // case CommandType.RegisterMovementEffect:
            //     return new RegisterMovementEffectCommand(SDBInterface.GetRegisterMovementEffectCommandDef(commandId));
            case CommandType.AuthorizeTerminal:
                return new AuthorizeTerminalCommand(CustomDBInterface.GetAuthorizeTerminalCommandDef(commandId));
            // case CommandType.TemporaryEquipmentStatMapping:
            //     return new TemporaryEquipmentStatMappingCommand(CustomDBInterface.GetTemporaryEquipmentStatMappingCommandDef(commandId));
            // case CommandType.LoadRegisterFromLevel:
            //     return new LoadRegisterFromLevelCommand(SDBInterface.GetLoadRegisterFromLevelCommandDef(commandId));
            // case CommandType.UnlockCerts:
            //     return new UnlockCertsCommand(CustomDBInterface.GetUnlockCertsCommandDef(commandId));
            // case CommandType.UnlockPatterns:
            //     return new UnlockPatternsCommand(CustomDBInterface.GetUnlockPatternsCommandDef(commandId));
            // case CommandType.UnlockTitles:
            //     return new UnlockTitlesCommand(CustomDBInterface.GetUnlockTitlesCommandDef(commandId));
            // case CommandType.UnlockWarpaints:
            //     return new UnlockWarpaintsCommand(CustomDBInterface.GetUnlockWarpaintsCommandDef(commandId));
            // case CommandType.UnlockDecals:
            //     return new UnlockDecalsCommand(CustomDBInterface.GetUnlockDecalsCommandDef(commandId));
            case CommandType.AwardRedBeans:
                return new AwardRedBeansCommand(CustomDBInterface.GetAwardRedBeansCommandDef(commandId));
            // case CommandType.SetDefaultDamageBonus:
            //     return new SetDefaultDamageBonusCommand(CustomDBInterface.GetSetDefaultDamageBonusCommandDef(commandId));
            // case CommandType.RequireEquippedItem:
            //     return new RequireEquippedItemCommand(SDBInterface.GetRequireEquippedItemCommandDef(commandId));
            case CommandType.CarryableObjectSpawn:
                return new CarryableObjectSpawnCommand(CustomDBInterface.GetCarryableObjectSpawnCommandDef(commandId));
            // case CommandType.UnlockVisualOverrides:
            //     return new UnlockVisualOverridesCommand(CustomDBInterface.GetUnlockVisualOverridesCommandDef(commandId));
            // case CommandType.RequireItemAttribute:
            //     return new RequireItemAttributeCommand(SDBInterface.GetRequireItemAttributeCommandDef(commandId));
            // case CommandType.AddLootTable:
            //     return new AddLootTableCommand(CustomDBInterface.GetAddLootTableCommandDef(commandId));
            // case CommandType.UpdateWaitAndFireOnce:
            //     return new UpdateWaitAndFireOnceCommand(SDBInterface.GetUpdateWaitAndFireOnceCommandDef(commandId));
            case CommandType.RequireZoneType:
                return new RequireZoneTypeCommand(SDBInterface.GetRequireZoneTypeCommandDef(commandId));
            // case CommandType.SetInteractionType:
            //     return new SetInteractionTypeCommand(CustomDBInterface.GetSetInteractionTypeCommandDef(commandId));
            // case CommandType.UnpackItem:
            //     return new UnpackItemCommand(CustomDBInterface.GetUnpackItemCommandDef(commandId));
            // case CommandType.TargetOwnedDeployables:
            //     return new TargetOwnedDeployablesCommand(CustomDBInterface.GetTargetOwnedDeployablesCommandDef(commandId));
            // case CommandType.RemovePermanentEffect:
            //     return new RemovePermanentEffectCommand(CustomDBInterface.GetRemovePermanentEffectCommandDef(commandId));
            // case CommandType.RequireLootStore:
            //     return new RequireLootStoreCommand(CustomDBInterface.GetRequireLootStoreCommandDef(commandId));
            // case CommandType.TargetBySinVulnerable:
            //     return new TargetBySinVulnerableCommand(CustomDBInterface.GetTargetBySinVulnerableCommandDef(commandId));
            // case CommandType.ModifyDamageByType:
            //     return new ModifyDamageByTypeCommand(CustomDBInterface.GetModifyDamageByTypeCommandDef(commandId));
            // case CommandType.ModifyDamageByFaction:
            //     return new ModifyDamageByFactionCommand(CustomDBInterface.GetModifyDamageByFactionCommandDef(commandId));
            // case CommandType.ModifyDamageByHeadshot:
            //     return new ModifyDamageByHeadshotCommand(CustomDBInterface.GetModifyDamageByHeadshotCommandDef(commandId));
            // case CommandType.UnlockHeadAccessories:
            //     return new UnlockHeadAccessoriesCommand(CustomDBInterface.GetUnlockHeadAccessoriesCommandDef(commandId));
            // case CommandType.RequireDamageType:
            //     return new RequireDamageTypeCommand(SDBInterface.GetRequireDamageTypeCommandDef(commandId));
            case CommandType.RequireWeaponArmed:
                return new RequireWeaponArmedCommand(SDBInterface.GetRequireWeaponArmedCommandDef(commandId));
            // case CommandType.ModifyDamageByTarget:
            //     return new ModifyDamageByTargetCommand(CustomDBInterface.GetModifyDamageByTargetCommandDef(commandId));
            // case CommandType.ModifyDamageByMyHealth:
            //     return new ModifyDamageByMyHealthCommand(CustomDBInterface.GetModifyDamageByMyHealthCommandDef(commandId));
            // case CommandType.ModifyDamageByTargetHealth:
            //     return new ModifyDamageByTargetHealthCommand(CustomDBInterface.GetModifyDamageByTargetHealthCommandDef(commandId));
            // case CommandType.ModifyDamageByTargetDamageResponse:
            //     return new ModifyDamageByTargetDamageResponseCommand(CustomDBInterface.GetModifyDamageByTargetDamageResponseCommandDef(commandId));
            // case CommandType.AddAccountGroup:
            //     return new AddAccountGroupCommand(CustomDBInterface.GetAddAccountGroupCommandDef(commandId));
            // case CommandType.RequireInitiatorExists:
            //     return new RequireInitiatorExistsCommand(CustomDBInterface.GetRequireInitiatorExistsCommandDef(commandId));
            // case CommandType.RegisterTimedTrigger:
            //     return new RegisterTimedTriggerCommand(CustomDBInterface.GetRegisterTimedTriggerCommandDef(commandId));
            // case CommandType.Taunt:
            //     return new TauntCommand(CustomDBInterface.GetTauntCommandDef(commandId));
            // case CommandType.StartArc:
            //     return new StartArcCommand(CustomDBInterface.GetStartArcCommandDef(commandId));
            // case CommandType.RequestArcJobs:
            //     return new RequestArcJobsCommand(CustomDBInterface.GetRequestArcJobsCommandDef(commandId));
            // case CommandType.UnlockBattleframes:
            //     return new UnlockBattleframesCommand(CustomDBInterface.GetUnlockBattleframesCommandDef(commandId));
            // case CommandType.AddAppendageHealthPool:
            //     return new AddAppendageHealthPoolCommand(CustomDBInterface.GetAddAppendageHealthPoolCommandDef(commandId));
            // case CommandType.RequireSquadLeader:
            //     return new RequireSquadLeaderCommand(SDBInterface.GetRequireSquadLeaderCommandDef(commandId));
            // case CommandType.RequireHasCertificate:
            //     return new RequireHasCertificateCommand(SDBInterface.GetRequireHasCertificateCommandDef(commandId));
            // case CommandType.DropAllCarryable:
            //     return new DropAllCarryableCommand(CustomDBInterface.GetDropAllCarryableCommandDef(commandId));
            // case CommandType.RemoteAbilityCall:
            //     return new RemoteAbilityCallCommand(CustomDBInterface.GetRemoteAbilityCallCommandDef(commandId));
            // case CommandType.RequireInCombat:
            //     return new RequireInCombatCommand(SDBInterface.GetRequireInCombatCommandDef(commandId));
            // case CommandType.RequireHasItem:
            //     return new RequireHasItemCommand(SDBInterface.GetRequireHasItemCommandDef(commandId));
            // case CommandType.MountVehicle:
            //     return new MountVehicleCommand(CustomDBInterface.GetMountVehicleCommandDef(commandId));
            // case CommandType.RequireIsNPC:
            //     return new RequireIsNPCCommand(SDBInterface.GetRequireIsNPCCommandDef(commandId));
            // case CommandType.TargetAiTarget:
            //     return new TargetAiTargetCommand(CustomDBInterface.GetTargetAiTargetCommandDef(commandId));
            // case CommandType.ModifyDamageForInflict:
            //     return new ModifyDamageForInflictCommand(CustomDBInterface.GetModifyDamageForInflictCommandDef(commandId));
            // case CommandType.ApplyAmmoRider:
            //     return new ApplyAmmoRiderCommand(SDBInterface.GetApplyAmmoRiderCommandDef(commandId));
            // case CommandType.TargetFilterByRange:
            //     return new TargetFilterByRangeCommand(SDBInterface.GetTargetFilterByRangeCommandDef(commandId));
            // case CommandType.OverrideCollision:
            //     return new OverrideCollisionCommand(SDBInterface.GetOverrideCollisionCommandDef(commandId));
            // case CommandType.RequireHasUnlock:
            //     return new RequireHasUnlockCommand(SDBInterface.GetRequireHasUnlockCommandDef(commandId));
            // case CommandType.UnlockContent:
            //     return new UnlockContentCommand(CustomDBInterface.GetUnlockContentCommandDef(commandId));
            // case CommandType.ShowRewardScreen:
            //     return new ShowRewardScreenCommand(CustomDBInterface.GetShowRewardScreenCommandDef(commandId));
            // case CommandType.RegisterLoadScale:
            //     return new RegisterLoadScaleCommand(SDBInterface.GetRegisterLoadScaleCommandDef(commandId));
            // case CommandType.EnableInteraction:
            //     return new EnableInteractionCommand(CustomDBInterface.GetEnableInteractionCommandDef(commandId));
            // case CommandType.ReputationModifier:
            //     return new ReputationModifierCommand(CustomDBInterface.GetReputationModifierCommandDef(commandId));
            // case CommandType.HostilityOverride:
            //     return new HostilityOverrideCommand(CustomDBInterface.GetHostilityOverrideCommandDef(commandId));
            // case CommandType.AddFactionReputation:
            //     return new AddFactionReputationCommand(CustomDBInterface.GetAddFactionReputationCommandDef(commandId));
            // case CommandType.MovementFacing:
            //     return new MovementFacingCommand(SDBInterface.GetMovementFacingCommandDef(commandId));
            // case CommandType.RequireFriends:
            //     return new RequireFriendsCommand(SDBInterface.GetRequireFriendsCommandDef(commandId));
            // case CommandType.TargetFilterBySinAcquired:
            //     return new TargetFilterBySinAcquiredCommand(SDBInterface.GetTargetFilterBySinAcquiredCommandDef(commandId));
            // case CommandType.RequireMovementFlags:
            //     return new RequireMovementFlagsCommand(SDBInterface.GetRequireMovementFlagsCommandDef(commandId));
            // case CommandType.ItemAttributeModifier:
            //     return new ItemAttributeModifierCommand(CustomDBInterface.GetItemAttributeModifierCommandDef(commandId));
            // case CommandType.MovementTether:
            //     return new MovementTetherCommand(SDBInterface.GetMovementTetherCommandDef(commandId));
            // case CommandType.DamageItemSlot:
            //     return new DamageItemSlotCommand(CustomDBInterface.GetDamageItemSlotCommandDef(commandId));
            // case CommandType.TargetMyTinyObjects:
            //     return new TargetMyTinyObjectsCommand(CustomDBInterface.GetTargetMyTinyObjectsCommandDef(commandId));
            // case CommandType.RegisterLoadFromWeapon:
            //     return new RegisterLoadFromWeaponCommand(SDBInterface.GetRegisterLoadFromWeaponCommandDef(commandId));
            // case CommandType.ApplyClientStatusEffect:
            //     return new ApplyClientStatusEffectCommand(SDBInterface.GetApplyClientStatusEffectCommandDef(commandId));
            // case CommandType.RemoveClientStatusEffect:
            //     return new RemoveClientStatusEffectCommand(SDBInterface.GetRemoveClientStatusEffectCommandDef(commandId));
            // case CommandType.RequireItemDurability:
            //     return new RequireItemDurabilityCommand(SDBInterface.GetRequireItemDurabilityCommandDef(commandId));
            // case CommandType.RequireEliteLevel:
            //     return new RequireEliteLevelCommand(SDBInterface.GetRequireEliteLevelCommandDef(commandId));
            // case CommandType.RequireCAISState:
            //     return new RequireCAISStateCommand(SDBInterface.GetRequireCAISStateCommandDef(commandId));
            // case CommandType.InflictHitFeedback:
            //     return new InflictHitFeedbackCommand(CustomDBInterface.GetInflictHitFeedbackCommandDef(commandId));
            // case CommandType.RepositionClones:
            //     return new RepositionClonesCommand(CustomDBInterface.GetRepositionClonesCommandDef(commandId));
            // case CommandType.ApplyUnlock:
            //     return new ApplyUnlockCommand(CustomDBInterface.GetApplyUnlockCommandDef(commandId));
            // case CommandType.RequireAppliedUnlock:
            //     return new RequireAppliedUnlockCommand(CustomDBInterface.GetRequireAppliedUnlockCommandDef(commandId));
            // case CommandType.RequireHeadshot:
            //     return new RequireHeadshotCommand(SDBInterface.GetRequireHeadshotCommandDef(commandId));
            // case CommandType.DisableChatBubble:
            //     return new DisableChatBubbleCommand(SDBInterface.GetDisableChatBubbleCommandDef(commandId));
            // case CommandType.DisableHealthAndIcon:
            //     return new DisableHealthAndIconCommand(SDBInterface.GetDisableHealthAndIconCommandDef(commandId));
            // case CommandType.AddInitiatorToStatusEffect:
                // env `both`, but no such table in SDB?
            // case CommandType.RemoveInitiatorFromStatusEffect:
                // env `both`, but no such table in SDB?
            // case CommandType.ForceRespawn:
            //     return new ForceRespawnCommand(CustomDBInterface.GetForceRespawnCommandDef(commandId));
            // case CommandType.ReduceCooldowns:
            //     return new ReduceCooldownsCommand(CustomDBInterface.GetReduceCooldownsCommandDef(commandId));
            // case CommandType.RequireArcActive:
                // Zero instances in BaseCommandDef
            case CommandType.AttemptToCalldownVehicle:
                return new AttemptToCalldownVehicleCommand(SDBInterface.GetAttemptToCalldownVehicleCommandDef(commandId));
            default:
                break;
        }

        return new CustomPlaceholderCommand(commandType.ToString(), commandId);
    }
}