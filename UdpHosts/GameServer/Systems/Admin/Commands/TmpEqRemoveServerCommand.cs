using GameServer.Data;

namespace GameServer.Systems.Admin.Commands;

[ServerCommand("Remove a temporary equipment override", "tmpeq_remove <slot>", "tmpeq_remove", "tmpeqremove")]
public class TmpEqRemoveServerCommand : TmpEqServerCommand
{
    public override void Execute(string[] parameters, ServerCommandContext context)
    {
        if (parameters.Length != 1)
        {
            SourceFeedback("Usage: tmpeq_remove <slot>", context);
            return;
        }

        if (!CharacterLoadout.TryParseSlot(parameters[0], out var slot))
        {
            SourceFeedback($"Unknown slot: {parameters[0]}", context);
            return;
        }

        var (success, character) = ResolveTargetCharacter(context);
        if (!success)
        {
            return;
        }

        if (context.Service.RemoveEquipmentOverride(character.Player, slot))
        {
            character.ApplyLoadout(character.CurrentLoadout);
            SourceFeedback($"Removed {slot} override for {character} ({FormatOverrides(context, character.Player)})", context);
        }
        else
        {
            SourceFeedback($"No {slot} override set for {character}", context);
        }
    }
}
