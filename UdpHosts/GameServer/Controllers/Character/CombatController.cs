using AeroMessages.Common;
using AeroMessages.GSS.V66.Character.Command;
using AeroMessages.GSS.V66.Character.Event;
using VController = AeroMessages.GSS.V66.Vehicle.Controller;
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
        //Console.WriteLine("DEBUG: " + abilitySlot);

        if (abilitySlot == 0) { }  // Ability1        - Default button 1
        if (abilitySlot == 1) { }  // Ability2        - Default button 2
        if (abilitySlot == 2) { }  // Ability3        - Default button 3
        if (abilitySlot == 3) { }  // AbilityHKM      - Default button 4
        if (abilitySlot == 4) { }  // AbilityInteract - Default button E
        if (abilitySlot == 5) { }  // Auxiliary       - Default button G
        if (abilitySlot == 6) { }  // AbilityMedical  - Default button Q
        if (abilitySlot == 13) { } // AbilitySIN      - Default button F
        if (abilitySlot == 16)     // Vehicle         - Default button V
        {
            var resp = ActivateAbilityVehicle(player);
            client.NetChannels[ChannelType.ReliableGss].SendIAero(resp, player.EntityId);
            // client.NetChannels[ChannelType.ReliableGss].SendIAeroChanges(resp, player.EntityId);
        }
        if (abilitySlot == 17) { } // Auxiliary       - Default button T
    }

    public VController.CombatController ActivateAbilityVehicle(IPlayer player)
    {
        return new VController.CombatController
        {
            SlottedAbility_0 = 0,
            SlottedAbility_1 = 0,
            SlottedAbility_2 = 0,
            SlottedAbility_3 = 0,
            SlottedAbility_4 = 0,
            SlottedAbility_5 = 30770,
            SlottedAbility_6 = 0,
            SlottedAbility_7 = 0,
            SlottedAbility_8 = 43,

            SlottedAbility_0Prop = 0,
            SlottedAbility_1Prop = 0,
            SlottedAbility_2Prop = 0,
            SlottedAbility_3Prop = 0,
            SlottedAbility_4Prop = 0,
            SlottedAbility_5Prop = 30770,
            SlottedAbility_6Prop = 0,
            SlottedAbility_7Prop = 0,
            SlottedAbility_8Prop = 43,

            StatusEffectsChangeTime_0Prop = 11465,
            StatusEffectsChangeTime_1Prop = 3139,
            StatusEffectsChangeTime_2Prop = 2132,
            StatusEffectsChangeTime_3Prop = 5763,
            StatusEffectsChangeTime_4Prop = 29801,
            StatusEffectsChangeTime_5Prop = 28521,
            StatusEffectsChangeTime_6Prop = 8302,
            StatusEffectsChangeTime_7Prop = 25970,
            StatusEffectsChangeTime_8Prop = 27760,
            StatusEffectsChangeTime_9Prop = 25441,
            StatusEffectsChangeTime_10Prop = 25701,
            StatusEffectsChangeTime_11Prop = 24864,
            StatusEffectsChangeTime_12Prop = 8308,
            StatusEffectsChangeTime_13Prop = 27749,
            StatusEffectsChangeTime_14Prop = 28005,
            StatusEffectsChangeTime_15Prop = 28261,
            StatusEffectsChangeTime_16Prop = 8308,
            StatusEffectsChangeTime_17Prop = 2609,
            StatusEffectsChangeTime_18Prop = 14641,
            StatusEffectsChangeTime_19Prop = 12602,
            StatusEffectsChangeTime_20Prop = 14902,
            StatusEffectsChangeTime_21Prop = 14645,
            StatusEffectsChangeTime_22Prop = 20000,
            StatusEffectsChangeTime_23Prop = 30319,
            StatusEffectsChangeTime_24Prop = 12576,
            StatusEffectsChangeTime_25Prop = 8244,
            StatusEffectsChangeTime_26Prop = 12338,
            StatusEffectsChangeTime_27Prop = 13873,
            StatusEffectsChangeTime_28Prop = 11552,
            StatusEffectsChangeTime_29Prop = 28704,
            StatusEffectsChangeTime_30Prop = 29551,
            StatusEffectsChangeTime_31Prop = 29801,

            StatusEffects_0Prop = null,
            StatusEffects_1Prop = null,
            StatusEffects_2Prop = null,
            StatusEffects_3Prop = null,
            StatusEffects_4Prop = null,
            StatusEffects_5Prop = null,
            StatusEffects_6Prop = null,
            StatusEffects_7Prop = null,
            StatusEffects_8Prop = null,
            StatusEffects_9Prop = null,
            StatusEffects_10Prop = null,
            StatusEffects_11Prop = null,
            StatusEffects_12Prop = null,
            StatusEffects_13Prop = null,
            StatusEffects_14Prop = null,
            StatusEffects_15Prop = null,
            StatusEffects_16Prop = null,
            StatusEffects_17Prop = null,
            StatusEffects_18Prop = null,
            StatusEffects_19Prop = null,
            StatusEffects_20Prop = null,
            StatusEffects_21Prop = null,
            StatusEffects_22Prop = null,
            StatusEffects_23Prop = null,
            StatusEffects_24Prop = null,
            StatusEffects_25Prop = null,
            StatusEffects_26Prop = null,
            StatusEffects_27Prop = null,
            StatusEffects_28Prop = null,
            StatusEffects_29Prop = null,
            StatusEffects_30Prop = null,
            StatusEffects_31Prop = null
        };
    }
}