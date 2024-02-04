using AeroMessages.GSS.V66.Vehicle.Command;
using GameServer.Enums.GSS.Vehicle;
using GameServer.Extensions;
using GameServer.Packets;
using Serilog;

namespace GameServer.Controllers.Vehicle;

[ControllerID(Enums.GSS.Controllers.Vehicle_BaseController)]
public class BaseController : Base
{
    public override void Init(INetworkClient client, IPlayer player, IShard shard, ILogger logger)
    {
        // TODO: Implement
    }

    [MessageID((byte)Commands.MovementInput)]
    public void MovementInput(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        var movementInput = packet.Unpack<MovementInput>();
        var vehicle = client.AssignedShard.Entities[entityId & 0xffffffffffffff00] as Entities.Vehicle.VehicleEntity;
        if (vehicle.ControllingPlayer == player)
        {
            client.AssignedShard.Movement.VehicleMovementInput(client, vehicle, movementInput);
        }
    }

    [MessageID((byte)Commands.SetWaterLevelAndDesc)]
    public void SetWaterLevelAndDesc(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        var query = packet.Unpack<SetWaterLevelAndDesc>();
        var vehicle = client.AssignedShard.Entities[entityId & 0xffffffffffffff00] as Entities.Vehicle.VehicleEntity;
        vehicle.SetWaterLevelAndDesc(query.Value);
    }

    [MessageID((byte)Commands.SetEffectsFlag)]
    public void SetEffectsFlag(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        var query = packet.Unpack<SetEffectsFlag>();
        var vehicle = client.AssignedShard.Entities[entityId & 0xffffffffffffff00] as Entities.Vehicle.VehicleEntity;
        vehicle.SetEffectsFlags(query.UnkByte2_HeadlightEnabled);
    }
}
