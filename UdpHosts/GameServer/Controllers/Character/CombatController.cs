using AeroMessages.Common;
using AeroMessages.GSS.V66.Character;
using AeroMessages.GSS.V66.Character.Command;
using AeroMessages.GSS.V66.Character.Event;
using GameServer.Enums.GSS.Character;
using GameServer.Extensions;
using GameServer.Packets;
using Serilog;
using System;

namespace GameServer.Controllers.Character;

[ControllerID(Enums.GSS.Controllers.Character_CombatController)]
public class CombatController : Base
{
    ILogger _logger;
    public override void Init(INetworkClient client, IPlayer player, IShard shard, ILogger logger)
    {
        // TODO: Implement
    }

    [MessageID((byte)Commands.FireInputIgnored)]
    public void FireInputIgnored(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        // TODO: Implement
    }

    [MessageID((byte)Commands.UseScope)]
    public void UseScope(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        // TODO: Implement
    }

    [MessageID((byte)Commands.SelectFireMode)]
    public void SelectFireMode(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        // TODO: Implement
    }
    

    [MessageID((byte)Commands.ActivateAbility)]
    public void ActivateAbility(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        var activateAbility = packet.Unpack<ActivateAbility>();

        if (activateAbility == null) { return; }

        var abilitySlot = activateAbility.AbilitySlotIndex;

        if (abilitySlot == 0) { }  // Ability1        - Default button 1
        if (abilitySlot == 1) { }  // Ability2        - Default button 2
        if (abilitySlot == 2) { }  // Ability3        - Default button 3
        if (abilitySlot == 3) { }  // AbilityHKM      - Default button 4
        if (abilitySlot == 4) { }  // AbilityInteract - Default button E
        if (abilitySlot == 5) { }  // Auxiliary       - Default button G
        if (abilitySlot == 6) { }  // AbilityMedical  - Default button Q
        if (abilitySlot == 13) { } // AbilitySIN      - Default button F
        if (abilitySlot == 16) { } // Vehicle         - Default button V
        if (abilitySlot == 17) { } // Auxiliary       - Default button T

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