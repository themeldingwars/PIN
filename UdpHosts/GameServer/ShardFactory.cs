using System;
using Autofac;
using GameServer;
using GameServer.Aptitude;
using GameServer.Physics;
using GameServer.Systems.Chat;
using GameServer.Systems.Encounters;
using Serilog;
using Shared.Udp;

public class ShardFactory : IShardFactory
{
    private readonly IComponentContext _context;
    private readonly GameServerSettings _settings;
    private readonly ILogger _logger;
    private readonly Lazy<IPacketSender> _packetSender;

    public ShardFactory(IComponentContext context, GameServerSettings settings, ILogger logger, Lazy<IPacketSender> packetSender)
    {
        _context = context;
        _settings = settings;
        _logger = logger;
        _packetSender = packetSender;
    }

    public Shard Create(ulong instanceId)
    {
        var shard = new Shard(instanceId, _settings, _packetSender.Value, _logger);

        var physics = _context.Resolve<PhysicsEngine>(
            new TypedParameter(typeof(IShard), shard));
        var movement = _context.Resolve<MovementRelay>(
            new TypedParameter(typeof(IShard), shard));
        var abilities = _context.Resolve<AbilitySystem>(
            new TypedParameter(typeof(IShard), shard));
        var entityMan = _context.Resolve<EntityManager>(
            new TypedParameter(typeof(IShard), shard));
        var encounterMan = _context.Resolve<EncounterManager>(
            new TypedParameter(typeof(IShard), shard));
        var weaponSim = _context.Resolve<WeaponSim>(
            new TypedParameter(typeof(IShard), shard));
        var projectileSim = _context.Resolve<ProjectileSim>(
            new TypedParameter(typeof(IShard), shard));
        var chat = _context.Resolve<ChatService>(
            new TypedParameter(typeof(IShard), shard));
        var admin = _context.Resolve<AdminService>(
            new TypedParameter(typeof(IShard), shard));

        shard.SetSystems(physics, movement, abilities, entityMan, encounterMan, weaponSim, projectileSim, chat, admin);

        return shard;
    }
}
