#nullable enable
using System.Collections.Generic;
using System.Numerics;
using GameServer.Entities;
using GameServer.Entities.Character;

namespace GameServer.Systems.Admin.Commands;

[ServerCommand(
    "Make an NPC patrol a route of waypoints (x y z triples), or 'clear' to drop it.",
    "patrol [entityId] <x> <y> <z> [<x> <y> <z> ...] | patrol [entityId] clear",
    "patrol",
    "patrol_npc")]
public class PatrolNpcServerCommand : ServerCommand
{
    public override void Execute(string[] parameters, ServerCommandContext context)
    {
        // NPC: command target first, else an explicit <entityId> argument (mirrors the move/follow commands).
        CharacterEntity? npc = null;
        var argStart = 0;
        if (context.Target is CharacterEntity { IsNpc: true } targetChar && targetChar.NpcMovement != null)
        {
            npc = targetChar;
        }
        else if (parameters.Length > 0 && ulong.TryParse(parameters[0], out var npcId) &&
                 context.Shard.Entities.TryGetValue(npcId, out var e) && e is CharacterEntity { IsNpc: true } idNpc && idNpc.NpcMovement != null)
        {
            npc = idNpc;
            argStart = 1;
        }

        if (npc == null)
        {
            SourceFeedback("No NPC to command. Set a target (target <id>) or pass an <entityId>.", context);
            return;
        }

        if (argStart >= parameters.Length)
        {
            SourceFeedback("Usage: patrol [entityId] <x> <y> <z> [<x> <y> <z> ...] | patrol [entityId] clear", context);
            return;
        }

        if (string.Equals(parameters[argStart], "clear", System.StringComparison.OrdinalIgnoreCase))
        {
            context.Shard.AI.ClearPatrol(npc);
            SourceFeedback($"Cleared patrol route for {npc.AeroEntityId}", context);
            return;
        }

        var route = ParseRoute(parameters, argStart);
        if (route == null || route.Count == 0)
        {
            SourceFeedback("Invalid waypoints. Provide one or more <x> <y> <z> triples.", context);
            return;
        }

        context.Shard.AI.Patrol(npc, route);
        SourceFeedback($"{npc.AeroEntityId} patrolling {route.Count} waypoint(s)", context);
    }

    // Parses the remaining parameters as consecutive <x> <y> <z> triples; returns null on any malformed triple.
    private static List<Vector3>? ParseRoute(string[] parameters, int startIndex)
    {
        var route = new List<Vector3>();
        for (var i = startIndex; i + 2 < parameters.Length; i += 3)
        {
            if (!float.TryParse(parameters[i], out var x) ||
                !float.TryParse(parameters[i + 1], out var y) ||
                !float.TryParse(parameters[i + 2], out var z))
            {
                return null;
            }

            route.Add(new Vector3(x, y, z));
        }

        return route;
    }
}
