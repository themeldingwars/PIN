using Aero.Protocol;
using AeroMessages.GSS.Vehicle.Command;
using AeroMessages.GSS.Vehicle.Event;
using GameServer.Entities.Vehicle;
using GameServer.Extensions;
using GameServer.Packets;
using GameServer.Systems.Aptitude;
using Serilog;

namespace GameServer.Controllers.Vehicle;

[Typecode(GssVehicleView.CombatController)]
public class CombatController : Base
{
    public override void Init(INetworkClient client, IPlayer player, IShard shard, ILogger logger)
    {
        // TODO: Implement
    }

    [MessageID(GssVehicleCommand.ActivateAbility)]
    public void ActivateAbility(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        var activateAbility = packet.Unpack<ActivateAbility>();

        if (!client.AssignedShard.Entities.TryGetValue(entityId & 0xffffffffffffff00, out var entity) || entity is not VehicleEntity vehicle)
        {
            return;
        }

        var abilityId = vehicle.Abilities[(byte)activateAbility.AbilitySlotIndex];

        var character = player.CharacterEntity;
        var shard = character.Shard;

        if (character.IsPlayerControlled)
        {
            var message = new AbilityActivated() { AbilityId = abilityId, Time = activateAbility.Time };

            character.Player.NetChannels[ChannelType.ReliableGss].SendMessage(message, character.EntityId);
        }

        shard.Abilities.HandleActivateAbility(shard, vehicle, abilityId, activateAbility.Time, new AptitudeTargets(vehicle));
    }

    [MessageID(GssVehicleCommand.DeactivateAbility)]
    public void DeactivateAbility(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        // todo?
        // var deactivateAbility = packet.Unpack<DeactivateAbility>();
    }
}