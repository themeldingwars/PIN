namespace GameServer.Admin;

[ServerCommand("Send a chat message to all clients as the server", "say <message>", "say")]
public class SayServerCommand : ServerCommand
{
    public override void Execute(string[] parameters, ServerCommandContext context)
    {
        if (parameters.Length == 0)
        {
            SourceFeedback("No message was provided to say command", context);
            return;
        }

        var chat = context.Shard.Chat;
        var message = string.Join(" ", parameters);
        chat.SendToAll(message, Enums.ChatChannel.Admin, null);
    }
}