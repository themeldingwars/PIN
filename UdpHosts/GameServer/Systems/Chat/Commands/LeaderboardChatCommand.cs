using System;

namespace GameServer.Systems.Chat.Commands;

[ChatCommand("Opens leaderboard", "leaderboard [<id> | list]", "leaderboard", "scoreboard")]
public class LeaderboardChatCommand : ChatCommand
{
    public override void Execute(string[] parameters, ChatCommandContext context)
    {
        if (parameters.Length != 1)
        {
            SourceFeedback("Provide id of the leaderboard you'd like to open", context);
            return;
        }

        uint terminalId = parameters[0].Equals("list", StringComparison.OrdinalIgnoreCase) || parameters[0] == "0"
            ? int.MaxValue
            : ParseUIntParameter(parameters[0]);

        context.SourcePlayer.CharacterEntity.OpenLeaderboard(terminalId);
    }
}