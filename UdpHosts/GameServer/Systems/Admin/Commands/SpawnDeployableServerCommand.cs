using System.Numerics;
using GameServer.Data.SDB;

namespace GameServer.Admin;

[ServerCommand("Spawn a deployable by deployableTypeId, optionally at a location.", "deployable <deployableTypeId> [<x> <y> <z>]", "deployable", "spawn_deployable")]
public class SpawnDeployableServerCommand : ServerCommand
{
    public override void Execute(string[] parameters, ServerCommandContext context)
    {
        if (parameters.Length != 1 && parameters.Length != 4)
        {
            SourceFeedback("Invalid number of parameters for spawn deployable command", context);
            return;
        }

        if (context.SourcePlayer?.CharacterEntity == null && parameters.Length != 4)
        {
            SourceFeedback("Must provide position if player character is not available", context);
            return;
        }

        uint typeId = ParseUIntParameter(parameters[0]);
        if (SDBInterface.GetDeployable(typeId) == null)
        {
            SourceFeedback("No deployable data for this typeId", context);
            return;
        }
        
        var orientation = context.SourcePlayer.CharacterEntity.Rotation;

        if (parameters.Length == 4)
        {
            Vector3? paramPosition = ParseVector3Parameters(parameters, 1);
            if (paramPosition != null)
            {
                context.Shard.EntityMan.SpawnDeployable(typeId, (Vector3)paramPosition, orientation);
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
            context.Shard.EntityMan.SpawnDeployable(typeId, position, orientation);
        }
    }
}