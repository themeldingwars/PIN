using GameServer.Entities;
using GameServer.Systems.SystemEvents;
using Serilog;

namespace GameServer.Systems.Combat;

public class CombatSim
{
    private static readonly ILogger _logger = Log.ForContext<CombatSim>();

    private readonly Shard _shard; // Time in HitFeedback, EntityManager doesnt have Entities
    private readonly IEventBus _eventBus;
    private readonly EntityManager.EntityManager _entityMan;
    private readonly DamageSystem _damage;
    private readonly HitFeedback _feedback;

    public CombatSim(IEventBus eventBus, EntityManager.EntityManager entityMan, DamageSystem damage, Shard shard)
    {
        _eventBus = eventBus;
        _entityMan = entityMan;
        _damage = damage;
        _shard = shard;

        _feedback = new HitFeedback(_shard);

        _eventBus.Subscribe<ProjectileHitEvent>(OnProjectileHit);
    }

    private void OnProjectileHit(ProjectileHitEvent evt)
    {
        // TODO: Validate that target is damageable
        // TODO: Validate hostility
        // TODO: Damage defense calcs and stuff
        _shard.Entities.TryGetValue(evt.TargetId, out IEntity target);
        _shard.Entities.TryGetValue(evt.SourceId, out IEntity source);

        if (target == null)
        {
            _logger.Warning("Dropping ProjectileHitEvent hit because could not get target {targetId}", evt.TargetId);
        }

        if (source == null)
        {
            _logger.Warning("Dropping ProjectileHitEvent hit because could not get source {targetId}", evt.SourceId);
        }

        var dmg = evt.DamageAmount;
        _damage.ApplyDamage(target, dmg, source);
        _feedback.TookDebugHit(target, source, dmg, evt.HeadShot, evt.Crit);
    }
}