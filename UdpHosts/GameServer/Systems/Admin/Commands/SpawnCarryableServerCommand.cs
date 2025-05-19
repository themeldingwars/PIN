using System.Numerics;
using GameServer.Data.SDB;

namespace GameServer.Admin;

[ServerCommand("Spawn a carryable by carryableTypeId, optionally at a location.", "carryable <characterTypeId> [<x> <y> <z>]", "carryable", "spawn_carryable")]
public class SpawnCarryableServerCommand : ServerCommand
{
    public override void Execute(string[] parameters, ServerCommandContext context)
    {
        if (parameters.Length != 1 && parameters.Length != 4)
        {
            SourceFeedback("Invalid number of parameters for spawn carryable command", context);
            return;
        }

        if (context.SourcePlayer?.CharacterEntity == null && parameters.Length != 4)
        {
            SourceFeedback("Must provide position if player character is not available", context);
            return;
        }

        uint typeId = ParseUIntParameter(parameters[0]);
        if (SDBInterface.GetCarryableObject(typeId) == null)
        {
            SourceFeedback("No carryable data for this typeId", context);
            return;
        }
        
        if (parameters.Length == 4)
        {
            Vector3? paramPosition = ParseVector3Parameters(parameters, 1);
            if (paramPosition != null)
            {
                context.Shard.EntityMan.SpawnCarryable(typeId, (Vector3)paramPosition);
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
            context.Shard.EntityMan.SpawnCarryable(typeId, position);
        }
    }
}