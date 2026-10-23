namespace GameServer.Systems.Admin.Commands;

[ServerCommand("List the loot containers and slots the server is tracking", "lootdump")]
public class LootDumpServerCommand : ServerCommand
{
    public override void Execute(string[] parameters, ServerCommandContext context)
    {
        var character = context.SourcePlayer?.CharacterEntity;

        if (character == null)
        {
            SourceFeedback("Need a character to dump loot for", context);
            return;
        }

        SourceFeedback($"Loot: {context.Shard.Loot.DumpLoot(character.Position)}", context);
    }
}
