#nullable enable
using GameServer.Entities;
using GameServer.Entities.Character;
using GameServer.Systems.Ai;

namespace GameServer.Systems.Admin.Commands;

[ServerCommand(
    "Make an NPC follow a target entity within a leash radius, or 'clear' to drop it.",
    "follow [entityId] <targetId> [radius] | follow [entityId] clear",
    "follow",
    "follow_npc",
    "chase")]
public class FollowNpcServerCommand : ServerCommand
{
    public override void Execute(string[] parameters, ServerCommandContext context)
    {
        // NPC: command target first, else an explicit <entityId> argument (mirrors the move command).
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
            SourceFeedback("Usage: follow [entityId] <targetId> [radius] | follow [entityId] clear", context);
            return;
        }

        if (string.Equals(parameters[argStart], "clear", System.StringComparison.OrdinalIgnoreCase))
        {
            context.Shard.AI.ClearTarget(npc);
            SourceFeedback($"Cleared follow target for {npc.AeroEntityId}", context);
            return;
        }

        var targetId = ParseULongParameter(parameters[argStart]);
        if (targetId == 0)
        {
            SourceFeedback("Invalid <targetId>.", context);
            return;
        }

        var radius = NpcMovementSystem.FollowDefaultRadius;
        if (argStart + 1 < parameters.Length && float.TryParse(parameters[argStart + 1], out var parsedRadius))
        {
            radius = parsedRadius;
        }

        context.Shard.AI.Follow(npc, targetId, radius);

        var present = context.Shard.Entities.ContainsKey(targetId);
        var note = present ? string.Empty : " (target not present yet; will resume when it appears)";
        SourceFeedback($"{npc.AeroEntityId} now following {targetId} within {radius:0.##}{note}", context);
    }
}
