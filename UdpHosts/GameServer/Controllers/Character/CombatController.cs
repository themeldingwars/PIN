using AeroMessages.GSS.V66.Character.Command;
using AeroMessages.GSS.V66.Character.Event;
using GameServer.Enums.GSS.Character;
using GameServer.Extensions;
using GameServer.Packets;
using Serilog;

namespace GameServer.Controllers.Character;

[ControllerID(Enums.GSS.Controllers.Character_CombatController)]
public class CombatController : Base
{
    public override void Init(INetworkClient client, IPlayer player, IShard shard, ILogger logger)
    {
        // TODO: Implement
    }

    [MessageID((byte)Commands.FireInputIgnored)]
    public void FireInputIgnored(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        // TODO: Implement
    }

    [MessageID((byte)Commands.FireBurst)]
    public void FireBurst(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        // TODO: Implement
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

        client.NetChannels[ChannelType.ReliableGss].SendIAero(fireWeaponProjectile, player.CharacterEntity.EntityId);
    }

    [MessageID((byte)Commands.FireEnd)]
    public void FireEnd(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        // TODO: Implement
    }

    [MessageID((byte)Commands.UseScope)]
    public void UseScope(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        // TODO: Implement
    }

    [MessageID((byte)Commands.SelectWeapon)]
    public void SelectWeapon(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        // TODO: Implement
    }

    [MessageID((byte)Commands.SelectFireMode)]
    public void SelectFireMode(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        // TODO: Implement
    }

    [MessageID((byte)Commands.ReloadWeapon)]
    public void ReloadWeapon(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        // TODO: Implement
    }

    [MessageID((byte)Commands.ActivateAbility)]
    public void ActivateAbility(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        var activateAbility = packet.Unpack<ActivateAbility>();

        if (activateAbility == null)
        {
            return;
        }

        var abilitySlot = activateAbility.AbilitySlotIndex;

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
        }
        
        // Vehicle - Default button V
        if (abilitySlot == 16)
        {
        }
        
        // Auxiliary - Default button T
        if (abilitySlot == 17)
        {
        }

        /*var resp = new AbilityActivated
        {
            ActivatedAbilityId = activateAbility.AbilitySlotIndex,  // ??
            ActivatedTime = activateAbility.Time,
            AbilityCooldownsData = new AbilityCooldownsData
            {
                ActiveCooldowns_Group1 = Array.Empty<ActiveCooldown>(),
                ActiveCooldowns_Group2 = Array.Empty<ActiveCooldown>(),
                GlobalCooldown_Activated_Time = activateAbility.Time,
                GlobalCooldown_ReadyAgain_Time = activateAbility.Time+119,
                Unk = 0x00
            }
        };*/
    }
}