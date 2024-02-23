namespace GameServer.Admin;

public class ServerCommandContext
{
    public IShard Shard { get; set; }
    public INetworkPlayer SourcePlayer { get; set; }
}