using GameServer.Entities;

namespace GameServer.Admin;

public class ServerCommandContext
{
    public AdminService Service { get; set; }
    public IShard Shard { get; set; }
    public INetworkPlayer SourcePlayer { get; set; }
    public IEntity Target { get; set; }
}