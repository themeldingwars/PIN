using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Aero.Protocol;
using AeroMessages.GSS.Character;
using AeroMessages.GSS.Character.Command;
using AeroMessages.GSS.Character.Event;
using GameServer.Entities.Character;
using GameServer.Extensions;
using GameServer.Packets;
using GameServer.StaticDB;
using GameServer.Systems.Aptitude;
using Serilog;

namespace GameServer.Controllers.Character;

[Typecode(GssCharacterView.CombatController)]
public class CombatController : Base
{
    private ILogger _logger;

    public override void Init(INetworkClient client, IPlayer player, IShard shard, ILogger logger)
    {
        _logger = logger.ForContext<CharacterEntity>();
    }

    [MessageID(GssCharacterCommand.FireInputIgnored)]
    public void FireInputIgnored(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        // TODO: Implement
    }

    [MessageID(GssCharacterCommand.FireBurst)]
    public void FireBurst(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        var query = packet.Unpack<FireBurst>();
        player.CharacterEntity.SetFireBurst(query.Time);
    }

    [MessageID(GssCharacterCommand.FireWeaponProjectile)]
    public void FireWeaponProjectile(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        var fireWeaponProjectile = packet.Unpack<FireWeaponProjectile>();

        Vector3? shooterVelocity = fireWeaponProjectile.HaveShooterVelocity == 1 ? fireWeaponProjectile.ShooterVelocity : null;
        player.HandleFireWeaponProjectile(fireWeaponProjectile.Time, fireWeaponProjectile.AimDirection, shooterVelocity);

        var weaponProjectileFired = new WeaponProjectileFired
        {
            ShortTime = (ushort)fireWeaponProjectile.Time,
            Aim = fireWeaponProjectile.AimDirection,
            HaveShooterVelocity = fireWeaponProjectile.HaveShooterVelocity,
            ShooterVelocity = fireWeaponProjectile.ShooterVelocity
        };

        client.NetChannels[ChannelType.ReliableGss].SendMessage(weaponProjectileFired, player.CharacterEntity.EntityId);
    }

    [MessageID(GssCharacterCommand.FireEnd)]
    public void FireEnd(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        var query = packet.Unpack<FireEnd>();
        player.CharacterEntity.SetFireEnd(query.Time);
    }

    [MessageID(GssCharacterCommand.FireCancel)]
    public void FireCancel(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        var query = packet.Unpack<FireCancel>();
        player.CharacterEntity.SetFireCancel(query.Time);
    }

    [MessageID(GssCharacterCommand.UseScope)]
    public void UseScope(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        var query = packet.Unpack<UseScope>();

        // InScope is a signed byte, any non-zero value means "scoped in".
        // Casting it straight to byte would turn -1 into 255 and make the client disagree with the
        // server about the fire mode, which plays the scope-in animation and then snaps back to hip
        // fire: the field has to carry the same 0/1 the client's own scoped state uses.
        bool inScope = query.InScope != 0;

        _logger.Debug(
            "[Scope] UseScope InScope={InScope} Time={ClientTime} FireMode_1={Mode} ServerTime={ServerTime}",
            query.InScope, query.Time, inScope, player.CharacterEntity.Shard.CurrentTime);

        player.CharacterEntity.SetFireMode(1, new FireModeData
        {
           Mode = (byte)(inScope ? 1 : 0),
           Time = query.Time,
        });

        // The scope's own status effect is what the client uses for the zoomed view, and it is the server that
        // has to take it away again: see CharacterEntity.SetScopedState. The effect carries tfRequireServerConfirmed
        // in its duration chain, which the client executes against its own prediction, so the server's copy has
        // to wear the client's timestamp from the message instead of a fresh server one.
        player.CharacterEntity.SetScopedState(inScope, query.Time);
    }

    [MessageID(GssCharacterCommand.SelectWeapon)]
    public void SelectWeapon(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        var query = packet.Unpack<SelectWeapon>();

        // Switching weapons (or fire modes) never keeps the sights of the previous one: without this, a scope
        // effect whose "scope out" message got lost would hold the zoom over the whole next weapon.
        player.CharacterEntity.SetScopedState(false);

        player.CharacterEntity.SetWeaponIndex(new WeaponIndexData
        {
            Index = query.SelectedWeaponIndex,
            Unk1 = query.Unk3,
            Unk2 = 0,
            Time = query.Time,
        });
    }

    [MessageID(GssCharacterCommand.SelectFireMode)]
    public void SelectFireMode(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        var query = packet.Unpack<SelectFireMode>();

        player.CharacterEntity.SetScopedState(false);

        player.CharacterEntity.SetFireMode(0, new FireModeData
        {
           Mode = query.FireMode,
           Time = query.Time,
        });
    }

    [MessageID(GssCharacterCommand.ReloadWeapon)]
    public void ReloadWeapon(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        var query = packet.Unpack<ReloadWeapon>();
        player.CharacterEntity.SetWeaponReloaded(query.Time);
    }

    [MessageID(GssCharacterCommand.CancelReload)]
    public void CancelReload(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        var query = packet.Unpack<CancelReload>();
        player.CharacterEntity.SetWeaponReloadCancelled(query.Time);
    }

    [MessageID(GssCharacterCommand.ActivateConsumable)]
    public void ActivateConsumable(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        var query = packet.Unpack<ActivateConsumable>();
        _logger.Information("ActivateConsumable {ItemSdbId}", query?.ItemSdbId);
        if (query == null)
        {
            return;
        }

        var abilityModule = SDBInterface.GetAbilityModule(query.ItemSdbId);
        if (abilityModule == null)
        {
            return;
        }

        uint abilityId = abilityModule.AbilityChainId;
        if (abilityId != 0)
        {
            var character = player.CharacterEntity;
            var activationTime = query.Time;
            var shard = character.Shard;
            var initiator = character as IAptitudeTarget;
            var targets = new AptitudeTargets();

            bool success = shard.Abilities.HandleActivateAbility(shard, initiator, abilityId, activationTime, targets, abilityModuleId: query.ItemSdbId);
            if (character.IsPlayerControlled)
            {
                SendAbilityActivationResponse(character, abilityId, activationTime, success);
            }
        }
    }

    [MessageID(GssCharacterCommand.ActivateAbility)]
    public void ActivateAbility(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        var activateAbility = packet.Unpack<ActivateAbility>();
        _logger.Information("ActivateAbility Slot {AbilitySlotIndex}", activateAbility?.AbilitySlotIndex);
        if (activateAbility == null)
        {
            return;
        }

        // Get the ability id based on the slotted ability
        var abilitySlot = activateAbility.AbilitySlotIndex;
        var character = player.CharacterEntity;
        uint abilityId = 0;
        uint moduleId = 0;

        byte abilityCategory = 0;

        // Using the local data until we can get the loadout remotely
        if (character.CurrentLoadout != null)
        {
            moduleId = character.CurrentLoadout.GetAbilityModuleIdBySlotIndex(abilitySlot);
            if (moduleId != 0)
            {
                var abilityModule = SDBInterface.GetAbilityModule(moduleId);
                if (abilityModule != null)
                {
                    abilityId = abilityModule.AbilityChainId;
                    abilityCategory = abilityModule.UiCategory;
                }
                else
                {
                    _logger.Warning("ActivateAbility slot {AbilitySlotIndex}: module {ModuleId} has no AbilityModule SDB record", abilitySlot, moduleId);
                }
            }
        }

        // Defaults if we failed
        if (abilityId == 0)
        {
            // Ability1 - Default button 1
            if (abilitySlot == 0)
            {
            }

            // Ability2 - Default button 2
            if (abilitySlot == 1)
            {
            }

            // Ability3 - Default button 3
            if (abilitySlot == 2)
            {
            }

            // AbilityHKM - Default button 4
            if (abilitySlot == 3)
            {
            }

            // AbilityInteract - Default button E
            if (abilitySlot == 4)
            {
                abilityId = 187; // Interact
            }

            // Auxiliary - Default button G
            if (abilitySlot == 5)
            {
            }

            // AbilityMedical - Default button Q
            if (abilitySlot == 6)
            {
            }

            // AbilitySIN - Default button F
            if (abilitySlot == 13)
            {
                abilityId = 43; // 40? SIN Targetting
            }

            // Vehicle - Default button V
            if (abilitySlot == 16)
            {
            }

            // Auxiliary - Default button T
            if (abilitySlot == 17)
            {
            }
        }

        if (abilityId == 0 && moduleId != 0)
        {
            _logger.Warning("ActivateAbility slot {AbilitySlotIndex}: module {ModuleId} did not resolve to an ability id (AbilityChainId is 0 or missing)", abilitySlot, moduleId);
        }

        _logger.Information("ActivateAbility slot {AbilitySlotIndex}: module {ModuleId} resolved to ability {AbilityId} (category {AbilityCategory})", abilitySlot, moduleId, abilityId, abilityCategory);

        if (abilityId != 0)
        {
            var activationTime = activateAbility.Time;
            var shard = character.Shard;
            var initiator = character as IAptitudeTarget;

            // Server-side cooldown gate: while the ability is cooling down,
            // reject the activation instead of running the chain again.
            if (!shard.Abilities.IsAbilityReady(initiator, abilityId, abilityCategory, activationTime, out uint readyAgainTime))
            {
                _logger.Information("ActivateAbility slot {AbilitySlotIndex} (ability {AbilityId}) rejected: cooling down until {ReadyAgainTime}", abilitySlot, abilityId, readyAgainTime);
                if (character.IsPlayerControlled)
                {
                    SendAbilityActivationResponse(character, abilityId, activationTime, activated: false);
                }

                return;
            }

            var targets = activateAbility.Targets
            .Select(entityId => 
            {
                shard.Entities.TryGetValue(entityId.Backing & 0xffffffffffffff00, out var target);
                return target as IAptitudeTarget;
            })
            .Where(target => target != null)
            .ToArray();

            bool success = shard.Abilities.HandleActivateAbility(shard, initiator, abilityId, activationTime, new AptitudeTargets(targets), abilityModuleId: moduleId);
            if (character.IsPlayerControlled)
            {
                SendAbilityActivationResponse(character, abilityId, activationTime, success);
            }
        }
    }

    private static AbilityCooldownsData BuildAbilityCooldownsData(IShard shard, AbilityState state, uint activationTime)
    {
        uint now = shard.CurrentTime;

        var group1 = new List<ActiveCooldown>();
        var group2 = new List<ActiveCooldown>();

        // The cooldown entries are kept in shard time, so the global cooldown
        // window has to be expressed in shard time too - mixing in the
        // client-supplied activation time makes the client timer jump.
        uint globalReadyAgain = now + 300;
        foreach (var entry in state.Cooldowns)
        {
            if (!entry.IsActive(now))
            {
                continue;
            }

            switch (entry.Kind)
            {
                case AbilityCooldownKind.Local:
                    group1.Add(entry.ToActiveCooldown());
                    break;
                case AbilityCooldownKind.Category:
                    group2.Add(entry.ToActiveCooldown());
                    break;
                case AbilityCooldownKind.Global:
                    globalReadyAgain = Math.Max(globalReadyAgain, entry.ReadyAgainTime);
                    break;
                default:
                    break;
            }
        }

        return new AbilityCooldownsData
        {
            ActiveCooldowns_Group1 = group1.ToArray(),
            ActiveCooldowns_Group2 = group2.ToArray(),
            Unk = 0,
            GlobalCooldown_Activated_Time = now,
            GlobalCooldown_ReadyAgain_Time = globalReadyAgain,
        };
    }

    /// <summary>
    /// Acknowledges an ability activation (or its failure) with the cooldown
    /// payload the client uses to show the ability timer and gate re-use.
    /// </summary>
    private void SendAbilityActivationResponse(CharacterEntity character, uint abilityId, uint activationTime, bool activated)
    {
        var shard = character.Shard;
        var state = shard.Abilities.GetOrAddState(character);
        var cooldownsData = BuildAbilityCooldownsData(shard, state, activationTime);

        if (activated)
        {
            var message = new AbilityActivated
            {
                ActivatedAbilityId = abilityId,
                ActivatedTime = activationTime,
                AbilityCooldownsData = cooldownsData,
            };
            _logger.ForContext<AbilitySystem>()
                   .Information("AbilityActivated {ActivatedAbilityId} at {ActivatedTime}", message.ActivatedAbilityId, message.ActivatedTime);
            character.Player.NetChannels[ChannelType.ReliableGss].SendMessage(message, character.EntityId);
        }
        else
        {
            var message = new AbilityFailed
            {
                FailedAbilityId = abilityId,
                Unk2 = 0, // 0 in captures
                AbilityCooldownsData = cooldownsData,
            };
            _logger.ForContext<AbilitySystem>()
                   .Information("AbilityFailed {FailedAbilityId} at {ActivationTime}", message.FailedAbilityId, activationTime);
            character.Player.NetChannels[ChannelType.ReliableGss].SendMessage(message, character.EntityId);
        }
    }
}