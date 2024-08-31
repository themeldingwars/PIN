using AeroMessages.GSS.V66.Vehicle.Command;
using AeroMessages.GSS.V66.Vehicle.Event;
using GameServer.Aptitude;
using GameServer.Entities.Vehicle;
using GameServer.Enums.GSS.Vehicle;
using GameServer.Extensions;
using GameServer.Packets;
using Serilog;

namespace GameServer.Controllers.Vehicle;

[ControllerID(Enums.GSS.Controllers.Vehicle_CombatController)]
public class CombatController : Base
{
    public override void Init(INetworkClient client, IPlayer player, IShard shard, ILogger logger)
    {
        // TODO: Implement
    }

    [MessageID((byte)Commands.ActivateAbility)]
    public void ActivateAbility(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        var activateAbility = packet.Unpack<ActivateAbility>();

        var vehicle = client.AssignedShard.Entities[entityId & 0xffffffffffffff00] as VehicleEntity;

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

    [MessageID((byte)Commands.DeactivateAbility)]
    public void DeactivateAbility(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        // todo?
        // var deactivateAbility = packet.Unpack<DeactivateAbility>();
    }
}
