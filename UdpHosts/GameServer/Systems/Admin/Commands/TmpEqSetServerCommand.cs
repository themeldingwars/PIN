using GameServer.Data;
using GameServer.Enums;
using GameServer.StaticDB;

namespace GameServer.Systems.Admin.Commands;

[ServerCommand("Set a temporary equipment override", "tmpeq_set <slot> <itemId>", "tmpeq_set", "tmpeqset")]
public class TmpEqSetServerCommand : TmpEqServerCommand
{
    public override void Execute(string[] parameters, ServerCommandContext context)
    {
        if (parameters.Length != 2)
        {
            SourceFeedback("Usage: tmpeq_set <slot> <itemId>", context);
            return;
        }

        if (!CharacterLoadout.TryParseSlot(parameters[0], out var slot))
        {
            SourceFeedback($"Unknown slot: {parameters[0]}", context);
            return;
        }

        var itemId = ParseUIntParameter(parameters[1]);
        var itemInfo = SDBInterface.GetRootItem(itemId);
        if (itemInfo == null)
        {
            SourceFeedback($"No item data for id {parameters[1]}", context);
            return;
        }

        var itemType = (ItemType)itemInfo.Type;
        var acceptedType = CharacterLoadout.GetAcceptedItemType(slot);
        if (acceptedType.HasValue && itemType != acceptedType.Value)
        {
            SourceFeedback($"Item {itemId} is of type {itemType}, but slot {slot} only accepts {acceptedType.Value} items", context);
            return;
        }

        var (success, character) = ResolveTargetCharacter(context);
        if (!success)
        {
            return;
        }

        context.Service.SetEquipmentOverride(character.Player, slot, itemId);
        character.ApplyLoadout(character.CurrentLoadout);
        SourceFeedback($"Set {slot} override to {itemId} for {character} ({FormatOverrides(context, character.Player)})", context);
    }
}
