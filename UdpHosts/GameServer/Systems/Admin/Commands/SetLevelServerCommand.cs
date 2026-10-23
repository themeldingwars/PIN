using GameServer.Systems.Progression;

namespace GameServer.Systems.Admin.Commands;

[ServerCommand("Set the level of your current battleframe, skipping the xp curve", "setlevel <level>", "setlevel", "set_level")]
public class SetLevelServerCommand : ProgressionServerCommand
{
    public override void Execute(string[] parameters, ServerCommandContext context)
    {
        var (success, player) = ResolveSourcePlayer(context);
        if (!success)
        {
            return;
        }

        if (parameters.Length == 0)
        {
            SourceFeedback($"Usage: setlevel <level>, the level range is 1 to {context.Shard.Progression.MaxFrameLevel}", context);
            return;
        }

        uint level = ParseUIntParameter(parameters[0]);
        if (level == 0)
        {
            SourceFeedback("The level has to be at least 1", context);
            return;
        }

        if (context.Shard.Progression.SetLevel(player, level) is not ProgressionUpdate update)
        {
            SourceFeedback("Could not set the level", context);
            return;
        }

        SourceFeedback(FormatProgression($"Set the level to {update.Level}", update), context);
    }
}
