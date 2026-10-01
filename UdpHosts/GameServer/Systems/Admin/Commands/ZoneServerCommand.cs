namespace GameServer.Systems.Admin.Commands;

[ServerCommand("Travel to another open world zone", "zone <id>", "zone")]
public class ZoneServerCommand : ServerCommand
{
    public override void Execute(string[] parameters, ServerCommandContext context)
    {
        if (parameters.Length != 1)
        {
            SourceFeedback("Invalid number of parameters for zone command", context);
            return;
        }

        if (context.SourcePlayer == null || context.SourcePlayer.CharacterEntity == null)
        {
            SourceFeedback("Cannot change zone without a valid player character", context);
            return;
        }

        uint zoneId = ParseUIntParameter(parameters[0]);

        if (!ZoneTransfer.TryStart(context.SourcePlayer, zoneId, out var refusal))
        {
            SourceFeedback(refusal, context);
        }
    }
}
