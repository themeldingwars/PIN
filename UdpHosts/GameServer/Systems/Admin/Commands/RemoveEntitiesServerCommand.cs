using GameServer.Entities.Character;

namespace GameServer.Admin;

[ServerCommand("Remove all entities except for player characters", "rment", "rment", "killall")]
public class RemoveEntitiesServerCommand : ServerCommand
{
    public override void Execute(string[] parameters, ServerCommandContext context)
    {
        foreach (var entity in context.Shard.Entities.Values)
        {
            if (entity is CharacterEntity character)
            {
                if (character.IsPlayerControlled)
                {
                    character.ClearAttachedTo();
                    continue;
                }
            }

            context.Shard.EntityMan.Remove(entity);
        }

        SourceFeedback($"Removing entities", context);
    }
}
