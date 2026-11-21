using System.Linq;
using AeroMessages.GSS.Character;

namespace GameServer.Systems.Admin.Commands;

[ServerCommand("Set a visual override on your character, slot 131 is the battleframe visuals override NPCs get from their visuals group", "visualoverride <visuals group id> [slot] | visualoverride clear [slot] | visualoverride list", "visualoverride", "visual_override")]
public class VisualOverrideServerCommand : ServerCommand
{
    private const byte _battleframeVisualsSlot = 131;

    public override void Execute(string[] parameters, ServerCommandContext context)
    {
        var character = context.SourcePlayer?.CharacterEntity;
        if (character == null)
        {
            SourceFeedback("Needs a player character", context);
            return;
        }

        if (parameters.Length is < 1 or > 2)
        {
            SourceFeedback("Usage: visualoverride <visuals group id> [slot] | visualoverride clear [slot] | visualoverride list", context);
            return;
        }

        var current = character.VisualOverrides.Data ?? [];
        if (parameters[0] == "list")
        {
            SourceFeedback(current.Length == 0 ? "No visual overrides" : string.Join(", ", current.Select(o => $"slot {o.SlotType}: {o.VisualsGroupId}")), context);
            return;
        }

        var slot = _battleframeVisualsSlot;
        if (parameters.Length == 2 && !byte.TryParse(parameters[1], out slot))
        {
            SourceFeedback($"Invalid slot {parameters[1]}", context);
            return;
        }

        var others = current.Where(o => o.SlotType != slot);
        if (parameters[0] == "clear")
        {
            character.SetVisualOverrides(new VisualOverridesField { Data = [.. others] });
            character.ApplyLoadout(character.CurrentLoadout);
            SourceFeedback($"Cleared the visual override in slot {slot}", context);
            return;
        }

        if (!uint.TryParse(parameters[0], out var visualsGroupId))
        {
            SourceFeedback($"Invalid visuals group id {parameters[0]}", context);
            return;
        }

        character.SetVisualOverrides(new VisualOverridesField { Data = [.. others, new VisualOverridesData { SlotType = slot, VisualsGroupId = visualsGroupId }] });
        character.ApplyLoadout(character.CurrentLoadout);
        SourceFeedback($"Set the visual override in slot {slot} to visuals group {visualsGroupId}", context);
    }
}