using System;
using System.Linq;
using AeroMessages.GSS.V66.Character;
using AeroMessages.GSS.V66.Character.Command;
using AeroMessages.GSS.V66.Character.Event;
using GameServer.Aptitude;
using GameServer.Data.SDB;
using GameServer.Entities;
using GameServer.Enums.GSS.Character;
using GameServer.Extensions;
using GameServer.Packets;
using Serilog;

namespace GameServer.Controllers.Character;

[ControllerID(Enums.GSS.Controllers.Character_CombatController)]
public class CombatController : Base
{
    private ILogger _logger;

    public override void Init(INetworkClient client, IPlayer player, IShard shard, ILogger logger)
    {
        _logger = logger;
    }

    [MessageID((byte)Commands.FireInputIgnored)]
    public void FireInputIgnored(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        // TODO: Implement
    }

    [MessageID((byte)Commands.FireBurst)]
    public void FireBurst(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        var query = packet.Unpack<FireBurst>();
        player.CharacterEntity.SetFireBurst(query.Time);
    }

    [MessageID((byte)Commands.FireWeaponProjectile)]
    public void FireWeaponProjectile(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        var fireWeaponProjectile = packet.Unpack<FireWeaponProjectile>();
        
        var weaponProjectileFired = new WeaponProjectileFired
        {
            ShortTime = (ushort)fireWeaponProjectile.Time,
            Aim = fireWeaponProjectile.AimDirection,
            HaveMoreData = fireWeaponProjectile.HaveShooterVelocity,
            MoreData = fireWeaponProjectile.ShooterVelocity
        };

        // TODO: This should be sent remote
        // FIXME: Because WeaponProjectileFired has two AeroMessageId and SendIAero grabs the first one, it tries to send this to the CombatController instead of the CombatView which is invalid
        // client.NetChannels[ChannelType.ReliableGss].SendIAero(weaponProjectileFired, player.CharacterEntity.EntityId);
    }

    [MessageID((byte)Commands.FireEnd)]
    public void FireEnd(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        var query = packet.Unpack<FireEnd>();
        player.CharacterEntity.SetFireEnd(query.Time);
    }

    [MessageID((byte)Commands.FireCancel)]
    public void FireCancel(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        var query = packet.Unpack<FireCancel>();
        player.CharacterEntity.SetFireCancel(query.Time);
    }

    [MessageID((byte)Commands.UseScope)]
    public void UseScope(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        var query = packet.Unpack<UseScope>();
        player.CharacterEntity.SetFireMode(1, new FireModeData
        {
           Mode = query.InScope,
           Time = query.Time,
        });
    }

    [MessageID((byte)Commands.SelectWeapon)]
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

    [MessageID((byte)Commands.SelectFireMode)]
    public void SelectFireMode(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        var query = packet.Unpack<SelectFireMode>();
        player.CharacterEntity.SetFireMode(0, new FireModeData
        {
           Mode = query.FireMode,
           Time = query.Time,
        });
    }

    [MessageID((byte)Commands.ReloadWeapon)]
    public void ReloadWeapon(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        var query = packet.Unpack<ReloadWeapon>();
        player.CharacterEntity.SetWeaponReloaded(query.Time);
    }

    [MessageID((byte)Commands.CancelReload)]
    public void CancelReload(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        var query = packet.Unpack<CancelReload>();
        player.CharacterEntity.SetWeaponReloadCancelled(query.Time);
    }

    [MessageID((byte)Commands.ActivateAbility)]
    public void ActivateAbility(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        var activateAbility = packet.Unpack<ActivateAbility>();
        Console.WriteLine($"ActivateAbility Slot {activateAbility.AbilitySlotIndex}");
        if (activateAbility == null)
        {
            return;
        }

        // Get the ability id based on the slotted ability
        var abilitySlot = activateAbility.AbilitySlotIndex;
        var character = player.CharacterEntity;
        uint abilityId = 0;

        // Using the local data until we can get the loadout remotely
        if (character.CharData != null)
        {
            var loadout = character.CharData.Loadout;
            var module = loadout.BackpackModules.FirstOrDefault((mod) => mod.SlotIDX == abilitySlot);
            if (module != null)
            {
                var itemId = module.SdbID;
                var abilityModule = SDBInterface.GetAbilityModule(itemId);
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
            var initiator = character as IAptitudeTarget;
            var shard = player.CharacterEntity.Shard;
            var activationTime = activateAbility.Time;
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
            .ToHashSet<IAptitudeTarget>();

            shard.Abilities.HandleActivateAbility(shard, initiator, abilityId, activationTime, targets);
        }
    }
}