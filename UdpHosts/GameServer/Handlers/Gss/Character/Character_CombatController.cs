using System;
using System.Linq;
using System.Numerics;
using Aero.Protocol;
using AeroMessages.GSS.Character;
using AeroMessages.GSS.Character.Command;
using AeroMessages.GSS.Character.Event;
using GameServer.Entities.Character;
using GameServer.Extensions;
using GameServer.Protocol;
using GameServer.StaticDB;
using GameServer.Systems.Aptitude;
using GameServer.Systems.WeaponSim;
using Serilog;

namespace GameServer.Handlers.Gss.Character;

[Typecode(GssCharacterView.CombatController)]
public static class Character_CombatController
{
    private static readonly ILogger _logger = Log.ForContext(typeof(Character_CombatController));

    [MessageID(GssCharacterCommand.FireInputIgnored)]
    public static void FireInputIgnored(NetworkClient client, IShard shard, IPlayer player, ulong entityId, GamePacket packet)
    {
        var query = packet.Unpack<FireInputIgnored>();
        var serverTime = shard.CurrentTime;
        _logger.Debug("[FireInputIgnored] server={ServerTime} command={QueryTime} ignored={QueryValue}", serverTime, query.Time, query.Ignored);
        var character = player.CharacterEntity;
        character.Shard.WeaponSim.EnqueueFireEvent(character, new WeaponFireEvent
        {
            Time = query.Time,
            Type = WeaponFireEventType.FireInputIgnored,
            Value = (byte)query.Ignored
        });
    }

    [MessageID(GssCharacterCommand.FireBurst)]
    public static void FireBurst(NetworkClient client, IShard shard, IPlayer player, ulong entityId, GamePacket packet)
    {
        var query = packet.Unpack<FireBurst>();
        var serverTime = shard.CurrentTime;
        _logger.Debug("[FireBurst] server={ServerTime} command={QueryTime}", serverTime, query.Time);
        var character = player.CharacterEntity;
        character.SetFireBurst(query.Time);
        character.Shard.WeaponSim.EnqueueFireEvent(character, new WeaponFireEvent
        {
            Time = query.Time,
            Type = WeaponFireEventType.FireBurst
        });
    }

    [MessageID(GssCharacterCommand.FireWeaponProjectile)]
    public static void FireWeaponProjectile(NetworkClient client, IShard shard, IPlayer player, ulong entityId, GamePacket packet)
    {
        var query = packet.Unpack<FireWeaponProjectile>();

        var serverTime = shard.CurrentTime;
        _logger.Debug("[FireWeaponProjectile] server={ServerTime} command={QueryTime}", serverTime, query.Time);

        var character = player.CharacterEntity;

        Vector3? shooterVelocity = query.HaveShooterVelocity == 1 ? query.ShooterVelocity : null;

        // Both branches below come from a client FireWeaponProjectile, so the aim it carries is the most
        // recent thing the client told us. Keep it on the character so ability chains that run a moment
        // later can aim with what was actually reported instead of the live aim.
        character.RecordAimSample(query.Time, query.AimDirection);

        // Ground truth for the aim: the client sends this world-space vector itself, so putting it next to
        // the aim rebuilt from the movement pose pins down any heading convention error.
        _logger.Debug("[AimCheck] {Entity} fireAim={FireAim} poseAim={PoseAim} heading={Heading}",
            character.EntityId,
            query.AimDirection,
            character.AimDirection,
            character.Heading);

        // The WeaponSim owns projectile spawning and broadcasts WeaponProjectileFired / AbilityProjectileFired to every
        // player who has the shooter scoped in (WeaponSim.Broadcast*ProjectileFired), including the firing client, so
        // the controller does not need an echo of its own.
        var pendingAbility = character.TryConsumeAbilityProjectile(query.Time);
        if (pendingAbility.HasValue)
        {
            character.Shard.WeaponSim.OnFireAbilityProjectile(character, pendingAbility.Value, query.Time, query.AimDirection, shooterVelocity);
        }
        else
        {
            character.Shard.WeaponSim.OnFireWeaponProjectile(character, query.Time, query.AimDirection, shooterVelocity);
        }
    }

    [MessageID(GssCharacterCommand.FireEnd)]
    public static void FireEnd(NetworkClient client, IShard shard, IPlayer player, ulong entityId, GamePacket packet)
    {
        var query = packet.Unpack<FireEnd>();
        var serverTime = shard.CurrentTime;
        _logger.Debug("[FireEnd] server={ServerTime} command={QueryTime}", serverTime, query.Time);
        var character = player.CharacterEntity;
        character.SetFireEnd(query.Time);
        character.Shard.WeaponSim.EnqueueFireEvent(character, new WeaponFireEvent
        {
            Time = query.Time,
            Type = WeaponFireEventType.FireEnd
        });
    }

    [MessageID(GssCharacterCommand.FireCancel)]
    public static void FireCancel(NetworkClient client, IShard shard, IPlayer player, ulong entityId, GamePacket packet)
    {
        var query = packet.Unpack<FireCancel>();
        var serverTime = shard.CurrentTime;
        _logger.Debug("[FireCancel] server={ServerTime} command={QueryTime}", serverTime, query.Time);
        var character = player.CharacterEntity;
        character.SetFireCancel(query.Time);
        character.Shard.WeaponSim.EnqueueFireEvent(character, new WeaponFireEvent
        {
            Time = query.Time,
            Type = WeaponFireEventType.FireCancel
        });
    }

    [MessageID(GssCharacterCommand.UseScope)]
    public static void UseScope(NetworkClient client, IShard shard, IPlayer player, ulong entityId, GamePacket packet)
    {
        var query = packet.Unpack<UseScope>();

        var serverTime = shard.CurrentTime;
        _logger.Debug("[UseScope] server={ServerTime} command={QueryTime} inscope={InScope}", serverTime, query.Time, query.InScope);
        var character = player.CharacterEntity;
        character.SetFireMode(1, new FireModeData
        {
           Mode = (byte)query.InScope,
           Time = query.Time,
        });
        character.Shard.WeaponSim.EnqueueFireEvent(character, new WeaponFireEvent
        {
            Time = query.Time,
            Type = WeaponFireEventType.UseScope,
            Value = (byte)query.InScope
        });
    }

    [MessageID(GssCharacterCommand.SelectWeapon)]
    public static void SelectWeapon(NetworkClient client, IShard shard, IPlayer player, ulong entityId, GamePacket packet)
    {
        var query = packet.Unpack<SelectWeapon>();

        var serverTime = shard.CurrentTime;
        _logger.Debug("[SelectWeapon] server={ServerTime} command={QueryTime} index={SelectedWeaponIndex}, previous={PreviousWeaponIndex}", serverTime, query.Time, query.SelectedWeaponIndex, query.PreviousWeaponIndex);
        var character = player.CharacterEntity;
        character.SetWeaponIndex(new WeaponIndexData
        {
            Index = query.SelectedWeaponIndex,
            PreviousIndex = query.PreviousWeaponIndex,
            PlaySwapAnimation = 0,
            Time = query.Time,
        });
        character.Shard.WeaponSim.EnqueueFireEvent(character, new WeaponFireEvent
        {
            Time = query.Time,
            Type = WeaponFireEventType.SelectWeapon,
            Value = query.SelectedWeaponIndex
        });
    }

    [MessageID(GssCharacterCommand.SelectFireMode)]
    public static void SelectFireMode(NetworkClient client, IShard shard, IPlayer player, ulong entityId, GamePacket packet)
    {
        var query = packet.Unpack<SelectFireMode>();
        var serverTime = shard.CurrentTime;
        _logger.Debug("[SelectFireMode] server={ServerTime} command={QueryTime} fireMode={FireMode}", serverTime, query.Time, query.FireMode);

        var character = player.CharacterEntity;
        character.SetFireMode(0, new FireModeData
        {
           Mode = query.FireMode,
           Time = query.Time,
        });
        character.Shard.WeaponSim.EnqueueFireEvent(character, new WeaponFireEvent
        {
            Time = query.Time,
            Type = WeaponFireEventType.SelectFireMode,
            Value = query.FireMode
        });
    }

    [MessageID(GssCharacterCommand.ReloadWeapon)]
    public static void ReloadWeapon(NetworkClient client, IShard shard, IPlayer player, ulong entityId, GamePacket packet)
    {
        var query = packet.Unpack<ReloadWeapon>();
        var serverTime = shard.CurrentTime;
        _logger.Debug("[ReloadWeapon] server={ServerTime} command={QueryTime}", serverTime, query.Time);
        var character = player.CharacterEntity;
        character.SetWeaponReloaded(query.Time);
        character.Shard.WeaponSim.EnqueueFireEvent(character, new WeaponFireEvent
        {
            Time = query.Time,
            Type = WeaponFireEventType.ReloadWeapon
        });
    }

    [MessageID(GssCharacterCommand.CancelReload)]
    public static void CancelReload(NetworkClient client, IShard shard, IPlayer player, ulong entityId, GamePacket packet)
    {
        var query = packet.Unpack<CancelReload>();
        var serverTime = shard.CurrentTime;
        _logger.Debug("[CancelReload] server={ServerTime} command={QueryTime}", serverTime, query.Time);
        var character = player.CharacterEntity;
        character.SetWeaponReloadCancelled(query.Time);
        character.Shard.WeaponSim.EnqueueFireEvent(character, new WeaponFireEvent
        {
            Time = query.Time,
            Type = WeaponFireEventType.CancelReload
        });
    }

    [MessageID(GssCharacterCommand.ActivateConsumable)]
    public static void ActivateConsumable(NetworkClient client, IShard shard, IPlayer player, ulong entityId, GamePacket packet)
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
            if (character.IsPlayerControlled)
            {
                var message = new AbilityActivated
                {
                    ActivatedAbilityId = abilityId,
                    ActivatedTime = activationTime,
                    AbilityCooldownsData = new AbilityCooldownsData
                    {
                        ActiveCooldowns_Group1 = [],
                        ActiveCooldowns_Group2 = [],
                        GlobalCooldown_Activated_Time = activationTime,
                        GlobalCooldown_ReadyAgain_Time = activationTime + 300,
                        GlobalCooldown_Flags = 0,
                    }
                };
                _logger.ForContext<AbilitySystem>()
                       .Information("ActivateAbility {ActivatedAbilityId} at {ActivatedTime}", message.ActivatedAbilityId, message.ActivatedTime);
                character.Player.NetChannels[ChannelType.ReliableGss].SendMessage(message, character.EntityId);
            }

            var initiator = character as IAptitudeTarget;
            var targets = new AptitudeTargets();
            shard.Abilities.HandleActivateAbility(shard, initiator, abilityId, activationTime, targets);
        }
    }

    [MessageID(GssCharacterCommand.ActivateAbility)]
    public static void ActivateAbility(NetworkClient client, IShard shard, IPlayer player, ulong entityId, GamePacket packet)
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
        uint abilityId = ResolveAbilityIdBySlot(character, abilitySlot);

        if (abilityId != 0)
        {
            var activationTime = activateAbility.Time;
            if (character.IsPlayerControlled)
            {
                var message = new AbilityActivated
                {
                    ActivatedAbilityId = abilityId,
                    ActivatedTime = activationTime,
                    AbilityCooldownsData = new AbilityCooldownsData
                    {
                        ActiveCooldowns_Group1 = [],
                        ActiveCooldowns_Group2 = [],
                        GlobalCooldown_Activated_Time = activationTime,
                        GlobalCooldown_ReadyAgain_Time = activationTime + 300,
                        GlobalCooldown_Flags = 0,
                    }
                };
                _logger.ForContext<AbilitySystem>()
                       .Information("ActivateAbility {ActivatedAbilityId} at {ActivatedTime}", message.ActivatedAbilityId, message.ActivatedTime);
                character.Player.NetChannels[ChannelType.ReliableGss].SendMessage(message, character.EntityId);
            }

            var initiator = character as IAptitudeTarget;
            var targets = activateAbility.Targets
            .Where(entityId =>
            {
                try
                {
                    return shard.Entities[entityId.Backing & 0xffffffffffffff00] != null;
                }
                catch
                {
                    return false;
                }
            })
            .Select(entityId => (IAptitudeTarget)shard.Entities[entityId.Backing & 0xffffffffffffff00])
            .ToArray();

            var executionId = Guid.NewGuid();
            character.SetAbilityInput(abilityId, executionId, activationTime);
            shard.Abilities.HandleActivateAbility(shard, initiator, abilityId, activationTime, new AptitudeTargets(targets), executionId);
        }
    }

    [MessageID(GssCharacterCommand.DeactivateAbility)]
    public static void DeactivateAbility(NetworkClient client, IShard shard, IPlayer player, ulong entityId, GamePacket packet)
    {
        var query = packet.Unpack<DeactivateAbility>();
        var serverTime = shard.CurrentTime;
        var character = player.CharacterEntity;

        // The release only carries the slot; resolve it to the same ability id the activation stored its input
        // under, then end that execution.
        uint abilityId = ResolveAbilityIdBySlot(character, query.AbilitySlotIndex);
        var input = character.ClearAbilityInput(abilityId);
        if (input == null)
        {
            _logger.Debug("[DeactivateAbility] server={ServerTime} slot={Slot} ability={AbilityId} had no matching active input", serverTime, query.AbilitySlotIndex, abilityId);
            return;
        }

        _logger.Debug("[DeactivateAbility] server={ServerTime} slot={Slot} ability={AbilityId} exec={ExecId}", serverTime, query.AbilitySlotIndex, input.AbilityId, input.ExecutionId);
        character.Shard.Abilities.HandleDeactivateAbility(character, input.ExecutionId);
    }

    /// <summary>
    /// Resolves the ability id bound to an ability slot, from the equipped loadout, falling back to the
    /// hardcoded default abilities for slots that have no slotted module.
    /// </summary>
    /// <param name="character">The character whose equipped loadout provides the slotted abilities.</param>
    /// <param name="abilitySlot">The ability slot index to resolve.</param>
    /// <returns>The ability id bound to the slot, or 0 when nothing is slotted or defaulted for it.</returns>
    private static uint ResolveAbilityIdBySlot(CharacterEntity character, byte abilitySlot)
    {
        uint abilityId = 0;

        // Using the local data until we can get the loadout remotely
        if (character.CurrentLoadout != null)
        {
            var moduleId = character.CurrentLoadout.GetAbilityModuleIdBySlotIndex(abilitySlot);
            if (moduleId != 0)
            {
                var abilityModule = SDBInterface.GetAbilityModule(moduleId);
                if (abilityModule != null)
                {
                    abilityId = abilityModule.AbilityChainId;
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

        return abilityId;
    }
}