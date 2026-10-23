using Aero.Protocol;
using AeroMessages.GSS.Vehicle.Command;
using GameServer.Entities;
using GameServer.Extensions;
using GameServer.Protocol;

namespace GameServer.Handlers.Gss.Vehicle;

/// <summary>
///     Handlers for the vehicle base controller view.
/// </summary>
[Typecode(GssVehicleView.BaseController)]
public static class Vehicle_BaseController
{
    [MessageID(GssVehicleCommand.MovementInput)]
    public static void MovementInput(NetworkClient client, IShard shard, IPlayer player, ulong entityId, GamePacket packet)
    {
        var movementInput = packet.Unpack<MovementInput>();
        shard.Entities.TryGetValue(entityId & 0xffffffffffffff00, out IEntity entity);
        if (entity == null)
        {
            return;
        }

        var vehicle = entity as Entities.Vehicle.VehicleEntity;
        if (vehicle.ControllingPlayer == player)
        {
            shard.Movement.VehicleMovementInput(client, vehicle, movementInput);
        }
    }

    [MessageID(GssVehicleCommand.SetWaterLevelAndDesc)]
    public static void SetWaterLevelAndDesc(NetworkClient client, IShard shard, IPlayer player, ulong entityId, GamePacket packet)
    {
        var query = packet.Unpack<SetWaterLevelAndDesc>();
        shard.Entities.TryGetValue(entityId & 0xffffffffffffff00, out IEntity entity);
        if (entity == null)
        {
            return;
        }

        var vehicle = entity as Entities.Vehicle.VehicleEntity;
        vehicle.SetWaterLevelAndDesc(query.Value);
    }

    [MessageID(GssVehicleCommand.SetEffectsFlag)]
    public static void SetEffectsFlag(NetworkClient client, IShard shard, IPlayer player, ulong entityId, GamePacket packet)
    {
        var query = packet.Unpack<SetEffectsFlag>();
        shard.Entities.TryGetValue(entityId & 0xffffffffffffff00, out IEntity entity);
        if (entity == null)
        {
            return;
        }

        var vehicle = entity as Entities.Vehicle.VehicleEntity;
        vehicle.SetEffectsFlags(query.Headlights);
    }
}
