using System;

namespace GameServer.Systems.Admin.Commands;

[ServerCommand("Toggle NPC aggression: pacify the admin target, or every live NPC with 'all'.", "pacify <on|off> [all]", "pacify", "passive", "calm")]
public class PacifyServerCommand : ServerCommand
{
    public override void Execute(string[] parameters, ServerCommandContext context)
    {
        if (parameters.Length is < 1 or > 2)
        {
            SourceFeedback("Usage: pacify <on|off> [all]", context);
            return;
        }

        bool passive;
        if (string.Equals(parameters[0], "on", StringComparison.OrdinalIgnoreCase))
        {
            passive = true;
        }
        else if (string.Equals(parameters[0], "off", StringComparison.OrdinalIgnoreCase))
        {
            passive = false;
        }
        else
        {
            SourceFeedback("Usage: pacify <on|off> [all]", context);
            return;
        }

        if (parameters.Length == 2 && string.Equals(parameters[1], "all", StringComparison.OrdinalIgnoreCase))
        {
            var count = context.Shard.AI.SetAllPassive(passive);
            SourceFeedback($"Pacify {(passive ? "on" : "off")} for {count} NPC brains", context);
            return;
        }

        if (context.Target == null)
        {
            SourceFeedback("No admin target set - use 'target <entityId/name>' first, or 'pacify <on|off> all'", context);
            return;
        }

        context.Shard.AI.SetPassive(context.Target.EntityId, passive);
        SourceFeedback($"Pacify {(passive ? "on" : "off")} for 0x{context.Target.EntityId:x16}", context);
    }
}
