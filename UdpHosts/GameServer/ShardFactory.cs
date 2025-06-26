using System;
using GameServer;
using Serilog;
using Shared.Udp;

public class ShardFactory : IShardFactory
{
    private readonly GameServerSettings _settings;
    private readonly ILogger _logger;
    private readonly Lazy<IPacketSender> _packetSender;

    public ShardFactory(GameServerSettings settings, ILogger logger, Lazy<IPacketSender> packetSender)
    {
        _settings = settings;
        _logger = logger;
        _packetSender = packetSender;
    }

    public Shard Create(ulong instanceId)
    {
        return new Shard(instanceId, _settings, _packetSender.Value, _logger);
    }
}
