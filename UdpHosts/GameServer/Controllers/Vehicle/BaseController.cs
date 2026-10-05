using Aero.Protocol;
using AeroMessages.GSS.Vehicle.Command;
using GameServer.Entities;
using GameServer.Extensions;
using GameServer.Packets;
using Serilog;

namespace GameServer.Controllers.Vehicle;

[Typecode(GssVehicleView.BaseController)]
public class BaseController : Base
{
    public override void Init(INetworkClient client, IPlayer player, IShard shard, ILogger logger)
    {
    }

    [MessageID(GssVehicleCommand.MovementInput)]
    public void MovementInput(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        var movementInput = packet.Unpack<MovementInput>();
        client.AssignedShard.Entities.TryGetValue(entityId & 0xffffffffffffff00, out IEntity entity);
        if (entity == null)
        {
            return;
        }

        var vehicle = entity as Entities.Vehicle.VehicleEntity;
        if (vehicle.ControllingPlayer == player)
        {
            client.AssignedShard.Movement.VehicleMovementInput(client, vehicle, movementInput);
        }
    }

    [MessageID(GssVehicleCommand.SetWaterLevelAndDesc)]
    public void SetWaterLevelAndDesc(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        var query = packet.Unpack<SetWaterLevelAndDesc>();
        client.AssignedShard.Entities.TryGetValue(entityId & 0xffffffffffffff00, out IEntity entity);
        if (entity == null)
        {
            return;
        }

        var vehicle = entity as Entities.Vehicle.VehicleEntity;
        vehicle.SetWaterLevelAndDesc(query.Value);
    }

    [MessageID(GssVehicleCommand.SetEffectsFlag)]
    public void SetEffectsFlag(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        var query = packet.Unpack<SetEffectsFlag>();
        client.AssignedShard.Entities.TryGetValue(entityId & 0xffffffffffffff00, out IEntity entity);
        if (entity == null)
        {
            return;
        }

        var vehicle = entity as Entities.Vehicle.VehicleEntity;
        vehicle.SetEffectsFlags(query.Headlights);
    }
}