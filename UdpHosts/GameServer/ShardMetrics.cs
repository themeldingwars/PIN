using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;

namespace GameServer;

public enum ShardSystem
{
    Posted,
    Physics,
    AI,
    Entities,
    Encounters,
    Abilities,
    Weapons,
    Turrets,
    Projectiles,
    Damage,
    Lifecycle,
    Respawn,
    Loot,
    Borders,
    Events,
    All,
}

/// <summary>
///     The shard's metrics, tagged with its zone. Recording doesn't allocate, so it runs on every tick.
/// </summary>
public sealed class ShardMetrics
{
    public const string MeterName = "PIN.GameServer";

    private static readonly Meter Meter = new(MeterName);
    private static readonly Histogram<double> TickDuration = Meter.CreateHistogram<double>("pin.shard.tick.duration", "ms", "Time per game tick and system, system=all for the whole tick");
    private static readonly Histogram<double> NetworkTickDuration = Meter.CreateHistogram<double>("pin.shard.network_tick.duration", "ms", "Time per network tick, receiving, acking, resending and sending for every client");
    private static readonly Counter<long> BytesReceived = Meter.CreateCounter<long>("pin.shard.bytes.received", "By", "UDP payload received from the shard's clients");
    private static readonly Counter<long> BytesSent = Meter.CreateCounter<long>("pin.shard.bytes.sent", "By", "UDP payload sent to the shard's clients");
    private static readonly ConcurrentQueue<ShardMetrics> Shards = new();

    private readonly KeyValuePair<string, object> _zone;
    private readonly TagList[] _systems;
    private readonly Func<int> _players;
    private readonly Func<int> _entities;

    static ShardMetrics()
    {
        Meter.CreateObservableGauge("pin.shard.players", () => Observe(s => s._players()), "{player}", "Connections on the shard");
        Meter.CreateObservableGauge("pin.shard.entities", () => Observe(s => s._entities()), "{entity}", "Entities in the shard, players and NPCs included");
    }

    public ShardMetrics(uint zoneId, Func<int> players, Func<int> entities)
    {
        _zone = new KeyValuePair<string, object>("zone", zoneId);
        _systems = Enum.GetValues<ShardSystem>()
                       .Select(system => new TagList { _zone, { "system", system.ToString().ToLowerInvariant() } })
                       .ToArray();
        _players = players;
        _entities = entities;
        Shards.Enqueue(this);
    }

    /// <summary>Records the time since <paramref name="since" /> and returns now, for the next system.</summary>
    public long Tick(ShardSystem system, long since)
    {
        var now = Stopwatch.GetTimestamp();
        TickDuration.Record(Stopwatch.GetElapsedTime(since, now).TotalMilliseconds, _systems[(int)system]);
        return now;
    }

    public void NetworkTick(long since) => NetworkTickDuration.Record(Stopwatch.GetElapsedTime(since).TotalMilliseconds, _zone);

    public void Received(int bytes) => BytesReceived.Add(bytes, _zone);

    public void Sent(int bytes) => BytesSent.Add(bytes, _zone);

    private static IEnumerable<Measurement<int>> Observe(Func<ShardMetrics, int> value) =>
        Shards.Select(shard => new Measurement<int>(value(shard), shard._zone));
}
