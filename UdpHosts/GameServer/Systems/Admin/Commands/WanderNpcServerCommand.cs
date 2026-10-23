#nullable enable
using GameServer.Entities;
using GameServer.Entities.Character;
using GameServer.Systems.Ai;

namespace GameServer.Systems.Admin.Commands;

[ServerCommand(
    "Make an NPC wander around its current position (optional radius), or 'clear' to stop.",
    "wander [entityId] [radius] | wander [entityId] clear",
    "wander",
    "wander_npc",
    "roam")]
public class WanderNpcServerCommand : ServerCommand
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

        if (argStart < parameters.Length && string.Equals(parameters[argStart], "clear", System.StringComparison.OrdinalIgnoreCase))
        {
            context.Shard.AI.ClearWander(npc);
            SourceFeedback($"Cleared wander for {npc.AeroEntityId}", context);
            return;
        }

        // Wander area is centered on the NPC's current position; radius is optional.
        var radius = NpcMovementSystem.WanderDefaultRadius;
        if (argStart < parameters.Length && float.TryParse(parameters[argStart], out var parsedRadius))
        {
            radius = parsedRadius;
        }

        context.Shard.AI.Wander(npc, npc.Position, radius);
        SourceFeedback($"{npc.AeroEntityId} wandering within {radius:0.##} of its position", context);
    }
}
