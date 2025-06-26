using GameServer;

public interface IShardFactory
{
    Shard Create(ulong instanceId);
}
