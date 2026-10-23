#nullable enable
using System.Numerics;
using GameServer.Entities;
using GameServer.Entities.Character;

namespace GameServer.Systems.Admin.Commands;

[ServerCommand("Move an NPC (command target, or <entityId>) to coords, or to your position.", "move [entityId] [<x> <y> <z>]", "move", "move_npc", "npc_move", "walk")]
public class MoveNpcServerCommand : ServerCommand
{
    public override void Execute(string[] parameters, ServerCommandContext context)
    {
        var player = context.SourcePlayer?.CharacterEntity;
        if (player == null)
        {
            SourceFeedback("Cannot move without a player", context);
            return;
        }

        // NPC: command target first, else an explicit <entityId> argument.
        CharacterEntity? npc = null;
        var coordStart = 0;
        if (context.Target is CharacterEntity { IsNpc: true } targetChar && targetChar.NpcMovement != null)
        {
            npc = targetChar;
        }
        else if (parameters.Length > 0 && ulong.TryParse(parameters[0], out var id) &&
                 context.Shard.Entities.TryGetValue(id, out var e) && e is CharacterEntity { IsNpc: true } idNpc && idNpc.NpcMovement != null)
        {
            npc = idNpc;
            coordStart = 1;
        }

        if (npc == null)
        {
            SourceFeedback("No NPC to move. Set a target (target <id>) or pass an <entityId>.", context);
            return;
        }

        // Destination: explicit coords, else the player's current position.
        var dest = ParseVector3Parameters(parameters, coordStart) ?? player.Position;
        context.Shard.AI.MoveTo(npc, dest);
        SourceFeedback($"Moving {npc.AeroEntityId} to {dest}", context);
    }
}
