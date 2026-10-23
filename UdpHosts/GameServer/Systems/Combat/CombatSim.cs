using GameServer.Entities;
using GameServer.Entities.Character;
using GameServer.StaticDB;
using GameServer.Systems.SystemEvents;
using Serilog;

namespace GameServer.Systems.Combat;

public class CombatSim
{
    private static readonly ILogger _logger = Log.ForContext<CombatSim>();

    private readonly Shard _shard;
    private readonly IEventBus _eventBus;
    private readonly EntityManager.EntityManager _entityMan;
    private readonly DamageSystem _damage;
    private readonly HitFeedback _feedback;
    private readonly HostilityResolver _hostility;

    public CombatSim(IEventBus eventBus, EntityManager.EntityManager entityMan, DamageSystem damage, Shard shard)
    {
        _eventBus = eventBus;
        _entityMan = entityMan;
        _damage = damage;
        _shard = shard;
        _hostility = shard.Hostility;

        _feedback = new HitFeedback(_shard);

        _eventBus.Subscribe<ProjectileHitEvent>(OnProjectileHit);
        _eventBus.Subscribe<InflictDamageEvent>(OnInflictDamage);
    }

    private void OnInflictDamage(InflictDamageEvent evt)
    {
        // Deliberately no hostility check on this route. An ability decides who it hits inside its own chain
        // (TargetHostiles, TargetFriendlies, the volume queries), so a hit that reaches here has already been
        // accepted as a legal target by the ability that dealt it.
        _shard.Entities.TryGetValue(evt.TargetId, out IEntity target);
        _shard.Entities.TryGetValue(evt.SourceId, out IEntity source);

        if (target == null)
        {
            _logger.Warning("Dropping InflictDamageEvent hit because could not get target {targetId}", evt.TargetId);
            return;
        }

        if (source == null)
        {
            _logger.Warning("Dropping InflictDamageEvent hit because could not get source {sourceId}", evt.SourceId);
            return;
        }

        var damageResponse = target.DamageResponse;
        var multiplier = SDBInterface.GetDamageResponseDamageType(damageResponse, evt.DamageType) ?? 1f;
        int damageAmount = (int)(evt.DamageAmount * multiplier);
        if (!_damage.ApplyDamage(target, damageAmount, source, new DamageInfo(damageAmount, evt.DamageType, false, evt.Origin)))
        {
            return;
        }

        _feedback.SendHitFeedback(target, source, damageAmount, evt.DamageType, false, false);

        _logger.Debug("Applied {evtOrigin} damage {damageAmount} to {evtTargetId} from {evtSourceId}, ability {evtAbilityId} command {evtCommandId}", evt.Origin, damageAmount, evt.TargetId, evt.SourceId, evt.AbilityId, evt.CommandId);
    }

    private void OnProjectileHit(ProjectileHitEvent evt)
    {
        // TODO: Validate that target is damageable
        // TODO: Damage defense calcs and stuff
        _shard.Entities.TryGetValue(evt.TargetId, out IEntity target);
        _shard.Entities.TryGetValue(evt.SourceId, out IEntity source);

        if (target == null)
        {
            _logger.Warning("Dropping ProjectileHitEvent hit because could not get target {targetId}", evt.TargetId);
            return;
        }

        if (source == null)
        {
            _logger.Warning("Dropping ProjectileHitEvent hit because could not get source {sourceId}", evt.SourceId);
            return;
        }

        if (!_hostility.CanDamage(source, target))
        {
            _logger.Debug(
                "Dropping ProjectileHitEvent hit on {targetId} from {sourceId}, stance {stance}",
                evt.TargetId,
                evt.SourceId,
                _hostility.GetStance(source, target));
            return;
        }

        // A projectile spawned by an ability chain is ability damage even though it arrives as an impact.
        DamageOrigin origin = evt.IsAbilityProjectile ? DamageOrigin.Ability : DamageOrigin.Weapon;
        var dmg = evt.DamageAmount;
        if (!_damage.ApplyDamage(target, dmg, source, new DamageInfo(dmg, evt.DamageType, evt.HeadShot || evt.Crit, origin)))
        {
            return;
        }

        _feedback.SendHitFeedback(target, source, dmg, evt.DamageType, evt.HeadShot, evt.Crit);
        if (source is CharacterEntity sourceChar && sourceChar.IsPlayerControlled && sourceChar.Player.Preferences.DebugWeapon != 0)
        {
            _eventBus.Enqueue(new DebugChatDirectMessageEvent($"You hit {evt.ShapeName} of {target}", sourceChar.Player));
        }
    }
}