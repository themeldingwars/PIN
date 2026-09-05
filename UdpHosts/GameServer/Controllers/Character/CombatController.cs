using System;
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
        var character = player.CharacterEntity;

        Vector3? shooterVelocity = fireWeaponProjectile.HaveShooterVelocity == 1 ? fireWeaponProjectile.ShooterVelocity : null;

        var pendingAbility = character.TryConsumeAbilityProjectile(fireWeaponProjectile.Time);
        if (pendingAbility.HasValue)
        {
            var pending = pendingAbility.Value;
            character.Shard.WeaponSim.OnFireAbilityProjectile(character, pending, fireWeaponProjectile.Time, fireWeaponProjectile.AimDirection, shooterVelocity);

            var abilityProjectileFired = new AbilityProjectileFired
            {
                ShortTime = (ushort)fireWeaponProjectile.Time,
                MaybeHalfs = default,
                Aim = fireWeaponProjectile.AimDirection,
                AmmoType = (ushort)pending.AmmoType,
                Range = pending.Range,
                Unk1 = 0,
                Unk2 = pending.BurstCount,
                Unk3 = 0,
                Unk4 = 0,
                Unk5 = 0,
                Hardpoint = pending.Hardpoint,
                UnkFlag = 0,
                UnkFlaggedEntity = 0,
            };

            client.NetChannels[ChannelType.ReliableGss].SendMessage(abilityProjectileFired, character.EntityId);
        }
        else
        {
            player.HandleFireWeaponProjectile(fireWeaponProjectile.Time, fireWeaponProjectile.AimDirection, shooterVelocity);

            var weaponProjectileFired = new WeaponProjectileFired
            {
                ShortTime = (ushort)fireWeaponProjectile.Time,
                Aim = fireWeaponProjectile.AimDirection,
                HaveShooterVelocity = fireWeaponProjectile.HaveShooterVelocity,
                ShooterVelocity = fireWeaponProjectile.ShooterVelocity
            };

            client.NetChannels[ChannelType.ReliableGss].SendMessage(weaponProjectileFired, character.EntityId);
        }
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
        player.CharacterEntity.SetFireMode(1, new FireModeData
        {
           Mode = (byte)query.InScope,
           Time = query.Time,
        });
    }

    [MessageID(GssCharacterCommand.SelectWeapon)]
    public void SelectWeapon(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        var query = packet.Unpack<SelectWeapon>();
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
            if (character.IsPlayerControlled)
            {
                var message = new AbilityActivated
                {
                    ActivatedAbilityId = abilityId,
                    ActivatedTime = activationTime,
                    AbilityCooldownsData = new AbilityCooldownsData
                    {
                        ActiveCooldowns_Group1 = Array.Empty<ActiveCooldown>(),
                        ActiveCooldowns_Group2 = Array.Empty<ActiveCooldown>(),
                        Unk = 0,
                        GlobalCooldown_Activated_Time = activationTime,
                        GlobalCooldown_ReadyAgain_Time = activationTime + 300,
                    }
                };
                _logger.ForContext<AbilitySystem>()
                       .Information("ActivateAbility {ActivatedAbilityId} at {ActivatedTime}", message.ActivatedAbilityId, message.ActivatedTime);
                character.Player.NetChannels[ChannelType.ReliableGss].SendMessage(message, character.EntityId);
            }

            var initiator = character as IAptitudeTarget;
            var shard = player.CharacterEntity.Shard;
            var targets = new AptitudeTargets();
            shard.Abilities.HandleActivateAbility(shard, initiator, abilityId, activationTime, targets);
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
                        ActiveCooldowns_Group1 = Array.Empty<ActiveCooldown>(),
                        ActiveCooldowns_Group2 = Array.Empty<ActiveCooldown>(),
                        Unk = 0,
                        GlobalCooldown_Activated_Time = activationTime,
                        GlobalCooldown_ReadyAgain_Time = activationTime + 300,
                    }
                };
                _logger.ForContext<AbilitySystem>()
                       .Information("ActivateAbility {ActivatedAbilityId} at {ActivatedTime}", message.ActivatedAbilityId, message.ActivatedTime);
                character.Player.NetChannels[ChannelType.ReliableGss].SendMessage(message, character.EntityId);
            }

            var initiator = character as IAptitudeTarget;
            var shard = player.CharacterEntity.Shard;
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

            shard.Abilities.HandleActivateAbility(shard, initiator, abilityId, activationTime, new AptitudeTargets(targets));
        }
    }
}