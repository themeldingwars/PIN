namespace GameServer.Systems.Admin.Commands;

[ServerCommand("Force respawn your character", "respawn", "respawn", "force_respawn")]
public class RespawnServerCommand : ServerCommand
{
    public override void Execute(string[] parameters, ServerCommandContext context)
    {
        if (context.SourcePlayer == null || context.SourcePlayer.CharacterEntity == null)
        {
            SourceFeedback("Cannot respawn without a valid player character", context);
            return;
        }

        context.Shard.PlayerRespawn.ForceRespawn(context.SourcePlayer.CharacterEntity);
        SourceFeedback("Respawning...", context);
    }
}
