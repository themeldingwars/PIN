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

    /// <summary>
    ///     Creates another shard of a zone that nobody finds by zone, e.g. a Melding pocket for the players of one
    ///     tornado. Loads the zone's collision, so call it off the shard threads.
    /// </summary>
    IShard CreateInstance(uint zoneId);

    /// <summary>Stops an instance, it has to be empty.</summary>
    void CloseInstance(IShard instance);
}
