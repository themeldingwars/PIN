namespace GameServer.Admin;

[ServerCommand("Clear target for server commands", "clear", "clear")]
public class ClearTargetServerCommand : ServerCommand
{
    public override void Execute(string[] parameters, ServerCommandContext context)
    {
        if (context.SourcePlayer == null)
        {
            SourceFeedback("Cannot clear target without a player", context);
            return;
        }

        context.Service.ClearTarget(context.SourcePlayer);

        SourceFeedback("Cleared target", context);
    }
}