using GameServer.Systems.Progression;

namespace GameServer.Systems.Admin.Commands;

/// <summary>
/// Shared plumbing for the commands that move battleframe progression around.
/// </summary>
public abstract class ProgressionServerCommand : ServerCommand
{
    protected (bool Success, INetworkPlayer Player) ResolveSourcePlayer(ServerCommandContext context)
    {
        if (context.SourcePlayer?.CharacterEntity == null)
        {
            SourceFeedback("Need a character to change the progression of", context);
            return (false, null);
        }

        return (true, context.SourcePlayer);
    }

    protected string FormatProgression(string action, ProgressionUpdate update)
    {
        var next = update.XpToNextLevel > 0
            ? $"{update.CurrentXp}/{update.XpToNextLevel} xp to level {update.Level + 1}"
            : "at the level cap";

        return $"{action}: frame {update.ChassisId} is level {update.Level}, {next}, {update.LifetimeXp} lifetime xp";
    }
}
