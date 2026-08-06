namespace GameServer.Systems.Admin.Commands;

[ServerCommand("Down your character instantly", "downme", "downme", "down", "bleed", "bleedout")]
public class DownMeCommand : ServerCommand
{
    public override void Execute(string[] parameters, ServerCommandContext context)
    {
        if (context.SourcePlayer == null || context.SourcePlayer.CharacterEntity == null)
        {
            SourceFeedback("Cannot commit suicide without a valid player character", context);
            return;
        }

        context.Shard.CharacterLifecycle.ForceBleedout(context.SourcePlayer.CharacterEntity);
        SourceFeedback("Down!", context);
    }
}
