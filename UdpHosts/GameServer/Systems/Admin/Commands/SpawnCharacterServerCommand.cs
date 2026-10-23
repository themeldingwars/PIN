using System.Numerics;
using GameServer.Entities.Character;
using GameServer.StaticDB;

namespace GameServer.Systems.Admin.Commands;

[ServerCommand("Spawn a character by characterTypeId, optionally at a location and level.", "npc <characterTypeId> [<level>] | [<x> <y> <z> [<level>]]", "npc", "character", "monster", "spawn_npc", "spawn_character", "spawn_monster")]
public class SpawnCharacterServerCommand : ServerCommand
{
    public override void Execute(string[] parameters, ServerCommandContext context)
    {
        // Accepted forms: <typeId> | <typeId> <level> | <typeId> <x> <y> <z> | <typeId> <x> <y> <z> <level>
        bool valid = parameters.Length == 1 || parameters.Length == 2 || parameters.Length == 4 || parameters.Length == 5;
        if (!valid)
        {
            SourceFeedback("Invalid number of parameters for spawn character command", context);
            return;
        }

        bool hasPosition = parameters.Length == 4 || parameters.Length == 5;
        byte level = CharacterEntity.DefaultLevel;
        if (parameters.Length == 2)
        {
            level = (byte)ParseUIntParameter(parameters[1]);
        }
        else if (parameters.Length == 5)
        {
            level = (byte)ParseUIntParameter(parameters[4]);
        }

        if (context.SourcePlayer?.CharacterEntity == null && !hasPosition)
        {
            SourceFeedback("Must provide position if player character is not available", context);
            return;
        }

        uint typeId = ParseUIntParameter(parameters[0]);
        if (SDBInterface.GetMonster(typeId) == null)
        {
            SourceFeedback("No monster data for this typeId", context);
            return;
        }

        const float minNormalZ = 0.5f; // surface must be within ~60 deg of vertical to count as walkable
        const float surfaceOffset = 0.5f; // lift the spawn point off the surface so the body is not embedded

        Vector3 spawnPosition;
        if (hasPosition)
        {
            Vector3? paramPosition = ParseVector3Parameters(parameters, 1);
            if (paramPosition == null)
            {
                SourceFeedback("Failed to parse position", context);
                return;
            }

            spawnPosition = (Vector3)paramPosition;

            // Explicit coords: drop the body onto the walkable surface directly below the point.
            if (context.Shard.Physics.TryFindSurfaceSpawn(spawnPosition, Vector3.UnitZ * -1f, 8f, minNormalZ, surfaceOffset, 0, out var grounded))
            {
                spawnPosition = grounded;
            }
        }
        else
        {
            var player = context.SourcePlayer.CharacterEntity;

            // Player: spawn on a flat surface in front of the player, along where they are aiming, rather than at
            // the player's own position (which can sit at/below the surface and embed the dynamic body).
            var aimOrigin = player.GetProjectileOrigin();
            if (context.Shard.Physics.TryFindSurfaceSpawn(aimOrigin, player.AimDirection, 30f, minNormalZ, surfaceOffset, player.EntityId, out var aimSpawn))
            {
                spawnPosition = aimSpawn;
            }
            else
            {
                spawnPosition = player.Position;
            }
        }

        var spawned = context.Shard.EntityMan.SpawnCharacter(typeId, spawnPosition, level: level);
        SourceFeedback($"Spawned NPC {typeId} (entityId {spawned.EntityId}) at {spawnPosition} (level {level}).", context);
    }
}