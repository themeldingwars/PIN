using System.Collections.Generic;

namespace GameServer;

public interface IShardManager
{
    /// <summary>
    ///     The shard for the configured default zone, which new connections join until they log in
    /// </summary>
    IShard DefaultShard { get; }

    IEnumerable<IShard> Shards { get; }

    /// <summary>
    ///     Returns the shard hosting a zone, creating it (and loading its collision) on first use
    /// </summary>
    IShard GetOrCreateShard(uint zoneId);
}
