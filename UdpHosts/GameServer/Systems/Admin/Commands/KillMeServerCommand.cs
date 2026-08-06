namespace GameServer.Systems.Admin.Commands;

[ServerCommand("Kill your character instantly", "killme", "killme", "suicide", "die")]
public class KillMeCommand : ServerCommand
{
    public override void Execute(string[] parameters, ServerCommandContext context)
    {
        if (context.SourcePlayer == null || context.SourcePlayer.CharacterEntity == null)
        {
            SourceFeedback("Cannot commit suicide without a valid player character", context);
            return;
        }

        context.Shard.CharacterLifecycle.ForceDeath(context.SourcePlayer.CharacterEntity);
        SourceFeedback("Die!", context);
    }
}
