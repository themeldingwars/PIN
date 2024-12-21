namespace GameServer.Systems.Chat;

public class ChatCommandContext
{
    public ChatCommandService Service { get; set; }
    public IShard Shard { get; set; }
    public INetworkPlayer SourcePlayer { get; set; }
}