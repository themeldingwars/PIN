using GameServer.Entities;
using GameServer.Entities.Character;
using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Other;

public class SpawnLootCommand : Command, ICommand
{
    private SpawnLootCommandDef Params;

    public SpawnLootCommand(SpawnLootCommandDef par)
: base(par)
    {
        Params = par;
    }

    /// <summary>
    ///     Drops the loot around the entity running the effect, credited to its initiator. The 1962 SDB only has the
    ///     command ids, which tables a command rolls is guessed in the custom data, commands without one drop nothing.
    ///     Retired items are left out, the 1962 deposits only dropped crystite although their tables still list ores.
    /// </summary>
    public override void Execute(Context context, ref CommandResult result)
    {
        if (Params.LootTableId == 0 && Params.LootTable2Id == 0)
        {
            Logger.Debug("{Command} {CommandId} has no loot table in the custom data, nothing drops", nameof(SpawnLootCommand), Params.Id);
            result.SetPass();
            return;
        }

        var level = context.Initiator is CharacterEntity character ? character.Level : 1;
        context.Shard.Loot.DropLoot(Params.LootTableId, Params.LootTable2Id, level, context.Self.Position, context.Initiator as IEntity, excludeRetired: true);
        result.SetPass();
    }
}
