using Aero.Protocol;
using AeroMessages.GSS.Vehicle.Command;
using AeroMessages.GSS.Vehicle.Event;
using GameServer.Entities.Vehicle;
using GameServer.Extensions;
using GameServer.Protocol;
using GameServer.Systems.Aptitude;

namespace GameServer.Handlers.Gss.Vehicle;

/// <summary>
///     Handlers for the vehicle combat controller view.
/// </summary>
[Typecode(GssVehicleView.CombatController)]
public static class Vehicle_CombatController
{
    [MessageID(GssVehicleCommand.ActivateAbility)]
    public static void ActivateAbility(NetworkClient client, IShard shard, IPlayer player, ulong entityId, GamePacket packet)
    {
        var activateAbility = packet.Unpack<ActivateAbility>();

        var vehicle = shard.Entities[entityId & 0xffffffffffffff00] as VehicleEntity;

        var abilityId = vehicle.Abilities[(byte)activateAbility.AbilitySlotIndex];

        var character = player.CharacterEntity;

        if (character.IsPlayerControlled)
        {
            var message = new AbilityActivated() { AbilityId = abilityId, Time = activateAbility.Time };

            character.Player.NetChannels[ChannelType.ReliableGss].SendMessage(message, character.EntityId);
        }

        shard.Abilities.HandleActivateAbility(shard, vehicle, abilityId, activateAbility.Time, new AptitudeTargets(vehicle));
    }

    [MessageID(GssVehicleCommand.DeactivateAbility)]
    public static void DeactivateAbility(NetworkClient client, IShard shard, IPlayer player, ulong entityId, GamePacket packet)
    {
        // TODO: implement - the DeactivateAbility packet is accepted but not unpacked or handled yet.
    }
}
