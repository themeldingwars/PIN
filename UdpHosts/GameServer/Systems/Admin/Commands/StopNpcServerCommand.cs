#nullable enable
using GameServer.Entities;
using GameServer.Entities.Character;

namespace GameServer.Systems.Admin.Commands;

[ServerCommand("Stop an NPC in place (command target, or <entityId>).", "stop [entityId]", "stop", "stop_npc", "npc_stop")]
public class StopNpcServerCommand : ServerCommand
{
    public override void Execute(string[] parameters, ServerCommandContext context)
    {
        CharacterEntity? npc = null;
        if (context.Target is CharacterEntity { IsNpc: true } targetChar && targetChar.NpcMovement != null)
        {
            npc = targetChar;
        }
        else if (parameters.Length > 0 && ulong.TryParse(parameters[0], out var id) &&
                 context.Shard.Entities.TryGetValue(id, out var e) && e is CharacterEntity { IsNpc: true } idNpc && idNpc.NpcMovement != null)
        {
            npc = idNpc;
        }

        if (npc == null)
        {
            SourceFeedback("No NPC to stop. Set a target (target <id>) or pass an <entityId>.", context);
            return;
        }

        context.Shard.AI.Stop(npc);
        SourceFeedback($"Stopped {npc.AeroEntityId}", context);
    }
}
