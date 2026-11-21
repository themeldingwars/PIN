namespace GameServer.Systems.Admin.Commands;

[ServerCommand("Review your character from downed/bleedout stage", "revive", "revive", "reviveme")]
public class ReviveMeCommand : ServerCommand
{
    public override void Execute(string[] parameters, ServerCommandContext context)
    {
        if (context.SourcePlayer == null || context.SourcePlayer.CharacterEntity == null)
        {
            SourceFeedback("Cannot revive without a valid player character", context);
            return;
        }

        var character = context.SourcePlayer.CharacterEntity;
        if (!context.Shard.CharacterLifecycle.TryRevive(character, fullHealth: true))
        {
            SourceFeedback("You are not downed", context);
            return;
        }

        SourceFeedback($"Be whole! Health: {character.CurrentHealth}/{character.MaxHealth.Value}", context);
    }
}
