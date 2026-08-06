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

        context.Shard.CharacterLifecycle.TryRevive(context.SourcePlayer.CharacterEntity);
        SourceFeedback("Be whole!", context);
    }
}
