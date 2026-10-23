using GameServer.Systems.Progression;

namespace GameServer.Systems.Admin.Commands;

[ServerCommand("Level up your current battleframe without paying xp", "levelup [levels]", "levelup", "level_up")]
public class LevelUpServerCommand : ProgressionServerCommand
{
    public override void Execute(string[] parameters, ServerCommandContext context)
    {
        var (success, player) = ResolveSourcePlayer(context);
        if (!success)
        {
            return;
        }

        uint levels = parameters.Length > 0 ? ParseUIntParameter(parameters[0]) : 1;
        if (levels == 0)
        {
            SourceFeedback("The number of levels has to be at least 1", context);
            return;
        }

        if (context.Shard.Progression.LevelUp(player, levels) is not ProgressionUpdate update)
        {
            SourceFeedback("Could not level up", context);
            return;
        }

        if (update.LevelsGained == 0)
        {
            SourceFeedback(FormatProgression("Already at the top of the xp curve", update), context);
            return;
        }

        SourceFeedback(FormatProgression($"Advanced {update.LevelsGained} level(s)", update), context);
    }
}
