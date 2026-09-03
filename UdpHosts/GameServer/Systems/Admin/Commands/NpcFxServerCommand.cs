using System;
using System.Globalization;
using System.Numerics;

namespace GameServer.Systems.Admin.Commands;

[ServerCommand("Live-tune NPC projectile FX: muzzle height fraction of the rig capsule, client hardpoint id, and the AbilityProjectileFired half-vector field.", "npcfx show | npcfx muzzle <fraction> | npcfx hardpoint <id> | npcfx offset <x> <y> <z>", "npcfx", "npc_fx")]
public class NpcFxServerCommand : ServerCommand
{
    public override void Execute(string[] parameters, ServerCommandContext context)
    {
        if (parameters.Length == 0 || string.Equals(parameters[0], "show", StringComparison.OrdinalIgnoreCase))
        {
            var aimPoint = AIEngine.NpcAimPointOverride?.ToString() ?? "off";
            SourceFeedback($"npcfx: muzzle={AIEngine.NpcMuzzleFraction.ToString(CultureInfo.InvariantCulture)} hardpoint={AIEngine.NpcFxHardpoint} offset={AIEngine.NpcFxOffset} aimpoint={aimPoint}", context);
            return;
        }

        switch (parameters[0].ToLowerInvariant())
        {
            case "muzzle" when parameters.Length == 2 && float.TryParse(parameters[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var fraction):
                AIEngine.NpcMuzzleFraction = Math.Clamp(fraction, 0f, 3f);
                SourceFeedback($"npcfx muzzle fraction = {AIEngine.NpcMuzzleFraction}", context);
                break;
            case "hardpoint" when parameters.Length == 2 && uint.TryParse(parameters[1], out var hardpoint):
                AIEngine.NpcFxHardpoint = hardpoint;
                SourceFeedback($"npcfx hardpoint = {hardpoint}", context);
                break;
            case "resetprofiles":
                var cleared = context.Shard.AI.ClearProfiles();
                SourceFeedback($"npcfx: cleared {cleared} cached AI profiles (live brains rebuild next tick)", context);
                break;
            case "aimpoint" when parameters.Length == 2 && string.Equals(parameters[1], "clear", StringComparison.OrdinalIgnoreCase):
                AIEngine.NpcAimPointOverride = null;
                SourceFeedback("npcfx aimpoint cleared - NPCs hunt live targets again", context);
                break;
            case "aimpoint" when parameters.Length == 2 && string.Equals(parameters[1], "here", StringComparison.OrdinalIgnoreCase):
                if (context.SourcePlayer?.CharacterEntity == null)
                {
                    SourceFeedback("No player character to take a position from", context);
                    return;
                }

                AIEngine.NpcAimPointOverride = context.SourcePlayer.CharacterEntity.Position;
                SourceFeedback($"npcfx aimpoint = {AIEngine.NpcAimPointOverride} (your current spot - now step aside)", context);
                break;
            case "aimpoint" when parameters.Length == 4:
                Vector3? point = ParseVector3Parameters(parameters, 1);
                if (point == null)
                {
                    SourceFeedback("Failed to parse aim point", context);
                    return;
                }

                AIEngine.NpcAimPointOverride = point;
                SourceFeedback($"npcfx aimpoint = {point}", context);
                break;
            case "offset" when parameters.Length == 4:
                Vector3? offset = ParseVector3Parameters(parameters, 1);
                if (offset == null)
                {
                    SourceFeedback("Failed to parse offset vector", context);
                    return;
                }

                AIEngine.NpcFxOffset = (Vector3)offset;
                SourceFeedback($"npcfx offset = {AIEngine.NpcFxOffset}", context);
                break;
            default:
                SourceFeedback("Usage: npcfx show | npcfx muzzle <fraction> | npcfx hardpoint <id> | npcfx offset <x> <y> <z>", context);
                break;
        }
    }
}
