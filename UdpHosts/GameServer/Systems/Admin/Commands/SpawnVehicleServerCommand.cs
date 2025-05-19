using System.Numerics;
using GameServer.Data.SDB;

namespace GameServer.Admin;

[ServerCommand("Spawn a vehicle by vehicleTypeId, optionally at a location.", "vehicle <vehicleTypeId> [<x> <y> <z>]", "vehicle", "spawn_vehicle")]
public class SpawnVehicleServerCommand : ServerCommand
{
    public override void Execute(string[] parameters, ServerCommandContext context)
    {
        if (parameters.Length != 1 && parameters.Length != 4)
        {
            SourceFeedback("Invalid number of parameters for spawn vehicle command", context);
            return;
        }

        if (context.SourcePlayer?.CharacterEntity == null && parameters.Length != 4)
        {
            SourceFeedback("Must provide position if player character is not available", context);
            return;
        }

        ushort typeId = (ushort)ParseUIntParameter(parameters[0]);
        if (SDBInterface.GetVehicleInfo(typeId) == null)
        {
            SourceFeedback("No vehicle data for this typeId", context);
            return;
        }
        
        var orientation = context.SourcePlayer.CharacterEntity.Rotation;

        if (parameters.Length == 4)
        {
            Vector3? paramPosition = ParseVector3Parameters(parameters, 1);
            if (paramPosition != null)
            {
                context.Shard.EntityMan.SpawnVehicle(typeId, (Vector3)paramPosition, orientation, null, false);
            }
            else
            {
                SourceFeedback("Failed to parse position", context);
                return;
            }
        }
        else
        {
            var position = context.SourcePlayer.CharacterEntity.Position;
            context.Shard.EntityMan.SpawnVehicle(typeId, position, orientation, null, false);
        }
    }
}