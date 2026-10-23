using GameServer.Entities.Character;
using GameServer.StaticDB;

namespace GameServer.Systems.Admin.Commands;

[ServerCommand("Roll a loot table and drop it at your feet", "lootdrop <lootTableId> [level]", "lootdrop", "loot")]
public class LootDropServerCommand : ServerCommand
{
    public override void Execute(string[] parameters, ServerCommandContext context)
    {
        if (context.SourcePlayer?.CharacterEntity == null)
        {
            SourceFeedback("Need a character to drop loot at", context);
            return;
        }

        if (parameters.Length == 0)
        {
            SourceFeedback("No lootTableId was provided to lootdrop command", context);
            return;
        }

        uint lootTableId = ParseUIntParameter(parameters[0]);
        var lootTable = SDBInterface.GetLootTable(lootTableId);

        if (lootTable == null)
        {
            SourceFeedback($"No loot table with id {lootTableId}", context);
            return;
        }

        int level = parameters.Length > 1 ? (int)ParseUIntParameter(parameters[1]) : CharacterEntity.DefaultLevel;
        var character = context.SourcePlayer.CharacterEntity;

        context.Shard.Loot.DropLoot(lootTableId, 0, level, character.Position, character);

        SourceFeedback($"Dropped loot table {lootTableId} ({lootTable.Name?.Trim()}) at level {level}", context);
    }
}
