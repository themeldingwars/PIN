using AeroMessages.GSS.V66.Character.Controller;

namespace GameServer.Systems.Chat.Commands;

[ChatCommand("Opens leaderboard", "leaderboard [id]", "leaderboard", "scoreboard")]
public class LeaderboardChatCommand : ChatCommand
{
    public override void Execute(string[] parameters, ChatCommandContext context)
    {
        if (parameters.Length != 1)
        {
            SourceFeedback("Provide id of the leaderboard you'd like to open", context);
            return;
        }

        context.SourcePlayer.CharacterEntity.SetAuthorizedTerminal(
             new AuthorizedTerminalData
             {
                 TerminalId = ParseUIntParameter(parameters[0]), TerminalType = 14, TerminalEntityId = 0
             });
    }
}
