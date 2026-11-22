using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using AeroMessages.GSS.Character.Event;
using BepuPhysics;
using BepuPhysics.Collidables;
using GameServer.Entities.Character;
using GameServer.Enums;
using GameServer.Physics;
using GameServer.StaticDB.Records.dbitems;
using GameServer.Systems.Aptitude;

namespace GameServer.Systems.ProjectileSim;

public class ProjectileSim
{
    private const float ThinRadius = 0.1f;
    private const uint _maxCatchUpMs = 1000;

    private readonly Shard _shard;
    private readonly Serilog.ILogger _logger;
    private readonly DebugProjectileHitCallbacks? _debugCallbacks;
    private readonly ConcurrentDictionary<(ulong EntityId, uint TraceId), ActiveProjectile> _activeProjectiles;

    public ProjectileSim(Shard shard, DebugProjectileHitCallbacks? debugCallbacks = null)
    {
        _shard = shard;
        _debugCallbacks = debugCallbacks;
        _logger = shard.Logger.ForContext<ProjectileSim>();
        _activeProjectiles = new ConcurrentDictionary<(ulong EntityId, uint TraceId), ActiveProjectile>();
    }

    public void FireProjectile(CharacterEntity entity, uint trace, Vector3 origin, Vector3 direction, Ammo ammo, float range, float projectileSpeed, float impactRadius, float maxRadius, float damage, bool isAbilityProjectile = false, Guid? sourceExecutionId = null, uint? fireTime = null)
    {
        uint startTime = GetStartTime(fireTime);
        var ammoFlags = new AmmoFlags(ammo.Flags);
        bool isDrunk = DrunkMissile.IsActive(ammo);

        var velocity = projectileSpeed * direction;
        float actualSpeed = velocity.Length();
        var lifetimeMs = ammo.ConstLifetime > 0
            ? ammo.ConstLifetime
            : ComputeDefaultLifetimeMs(range, actualSpeed);
        if (ammo.ConstLifetime == 0 && actualSpeed * lifetimeMs / 1000f > range * 1.01f && actualSpeed > 0f)
        {
            projectileSpeed = range * 1000f / lifetimeMs;
            velocity = velocity / actualSpeed * projectileSpeed;
            actualSpeed = projectileSpeed;
        }

        var endPosition = origin + (velocity * (lifetimeMs / 1000f));

        var projectile = new ActiveProjectile
        {
            EntityId = entity.EntityId,
            TraceId = trace,
            Type = ammoFlags.Simulation,
            Ammo = ammo,
            Origin = origin,
            Direction = direction,
            Velocity = velocity,
            InitialVelocity = velocity,
            StartPosition = origin,
            EndPosition = endPosition,
            CurrentPosition = origin,
            PreviousPosition = origin,
            StartTime = startTime,
            LifetimeMs = lifetimeMs,
            Range = range,
            BouncesRemaining = ammo.MaxBounces,
            HitsRemaining = (byte)ammo.MaxHits,
            IsAlive = true,
            HasHit = false,
            TargetEntityId = 0,
            HitEntityId = 0,
            HitPosition = Vector3.Zero,
            HitNormal = Vector3.Zero,
            AccumulatedDt = 0f,
            ImpactRadius = impactRadius,
            MaxRadius = maxRadius,
            Damage = damage,
            DamageType = ammo.Damagetype,
            DamageResponse = ammo.DamageResponse,
            IsDrunk = isDrunk,
            IsAbilityProjectile = isAbilityProjectile,
            NextPeriodAbilityTime = ammo.PeriodAbilityMs > 0 ? startTime + ammo.PeriodAbilityMs : 0,
            SourceExecutionId = sourceExecutionId
        };

        _activeProjectiles.TryAdd((entity.EntityId, trace), projectile);
        _logger.Debug("Spawned {Type} projectile trace={Trace}, speed={Speed}, range={Range}, lifetime={Lifetime}ms, impactRadius={ImpactRadius}, maxRadius={MaxRadius}, damage={Damage}, catchUp={CatchUp}ms", ammoFlags.Simulation, trace, projectileSpeed, range, lifetimeMs, impactRadius, maxRadius, damage, _shard.CurrentTime - startTime);
        SendDebugSpawn(entity, trace, origin, direction, projectileSpeed);
    }

    public void Tick(double deltaTime, ulong currentTime, CancellationToken ct)
    {
        var keys = _activeProjectiles.Keys.ToArray();

        foreach (var key in keys)
        {
            if (!_activeProjectiles.TryGetValue(key, out var projectile))
            {
                continue;
            }

            if (ct.IsCancellationRequested)
            {
                break;
            }

            uint elapsedMs = (uint)(currentTime - projectile.StartTime);

            projectile.PreviousPosition = projectile.CurrentPosition;

            Vector3 basePosition;
            switch (projectile.Type)
            {
                case AmmoFlags.SimulationMode.Basic:
                    basePosition = ComputeLinearPosition(projectile, elapsedMs);
                    break;

                case AmmoFlags.SimulationMode.Linear:
                    basePosition = ComputeLinearPosition(projectile, elapsedMs);
                    break;

                case AmmoFlags.SimulationMode.Parabolic:
                    basePosition = ComputeParabolicPosition(ref projectile, elapsedMs);
                    break;

                case AmmoFlags.SimulationMode.Homing:
                    basePosition = UpdateHoming(ref projectile, elapsedMs, projectile.CurrentPosition - projectile.DrunkOffset);
                    break;

                default:
                    basePosition = projectile.CurrentPosition;
                    break;
            }

            if (projectile.IsDrunk)
            {
                float elapsedSec = elapsedMs / 1000.0f;
                float progress = projectile.LifetimeMs == 0 ? 0f : elapsedMs / (float)projectile.LifetimeMs;
                projectile.DrunkOffset = DrunkMissile.ComputeOffset(projectile.InitialVelocity, projectile.Ammo, elapsedSec, progress, projectile.TraceId);
            }
            else
            {
                projectile.DrunkOffset = Vector3.Zero;
            }

            projectile.CurrentPosition = basePosition + projectile.DrunkOffset;

            if (projectile.Ammo.PeriodAbilityId != 0 && projectile.Ammo.PeriodAbilityMs > 0 && currentTime >= projectile.NextPeriodAbilityTime)
            {
                var periodSource = GetSourceEntity(projectile);
                if (periodSource != null)
                {
                    ActivateProjectileAbility(periodSource, projectile, ProjectileHitReason.Period, projectile.Ammo.PeriodAbilityId, (uint)currentTime, new AptitudeTargets(), projectile.CurrentPosition);
                    projectile.NextPeriodAbilityTime += projectile.Ammo.PeriodAbilityMs;
                }
            }

            if (projectile.DetonateReason != ProjectileHitReason.None && currentTime >= projectile.DetonateTime)
            {
                Detonate(projectile, projectile.DetonateReason, (uint)currentTime);
                _activeProjectiles.TryRemove(key, out _);
                _logger.Debug("Projectile trace={Trace} detonated ({Reason}) at {Elapsed}ms", projectile.TraceId, projectile.DetonateReason, elapsedMs);
                SendDebugTimeout(projectile, projectile.CurrentPosition);
                continue;
            }

            var hit = _shard.Physics.SegmentRayCast(projectile.PreviousPosition, projectile.CurrentPosition, projectile.EntityId);

            if (hit.Hit)
            {
                projectile.HasHit = true;
                projectile.HitEntityId = hit.HitEntityId;
                projectile.HitPosition = hit.HitPosition;
                projectile.HitNormal = hit.Normal;

                if (TryBounce(ref projectile, hit))
                {
                    projectile.PreviousPosition = hit.HitPosition;
                    _logger.Debug("Projectile trace={Trace} bounced off entity={Entity} at {Pos}", projectile.TraceId, hit.HitEntityId, hit.HitPosition);
                    SendDebugBounce(projectile, hit.HitPosition, hit.Normal);
                }
                else
                {
                    projectile.IsAlive = false;
                    projectile.HitsRemaining = 0;
                    _logger.Debug("Projectile trace={Trace} impact entity={Entity} at {Pos}", projectile.TraceId, hit.HitEntityId, hit.HitPosition);
                    var source = GetSourceEntity(projectile);
                    if (source != null)
                    {
                        if (source is CharacterEntity shooter)
                        {
                            SendProjectileHitReported(shooter, projectile.TraceId, (uint)currentTime);
                        }

                        _shard.Physics.HandleProjectileImpact(source, projectile.TraceId, hit, projectile.ImpactRadius, projectile.Damage, projectile.DamageType, projectile.IsAbilityProjectile, dealDamage: projectile.Ammo.AbilityId == 0);

                        if (projectile.Ammo.AbilityId != 0)
                        {
                            ActivateProjectileAbility(source, projectile, ProjectileHitReason.Impact, projectile.Ammo.AbilityId, (uint)currentTime, TargetsFor(hit.HitEntityId), hit.HitPosition, _shard.Physics.IsCriticalHit(hit));
                        }
                    }
                }
            }

            float radius = CurrentRadius(projectile, elapsedMs);
            if (radius >= ThinRadius && !new AmmoFlags(projectile.Ammo.Flags).Thin && projectile.DetonateReason == ProjectileHitReason.None)
            {
                ProcessOverlaps(ref projectile, radius, (uint)currentTime, elapsedMs);
            }

            if (elapsedMs >= projectile.LifetimeMs && projectile.DetonateReason == ProjectileHitReason.None)
            {
                if (projectile.IsAlive && new AmmoFlags(projectile.Ammo.Flags).Airburst)
                {
                    Detonate(projectile, ProjectileHitReason.Airburst, (uint)currentTime);
                }

                _activeProjectiles.TryRemove(key, out _);
                _logger.Debug("Projectile trace={Trace} expired at {Elapsed}/{Lifetime}ms", projectile.TraceId, elapsedMs, projectile.LifetimeMs);
                SendDebugTimeout(projectile, projectile.CurrentPosition);
                continue;
            }

            if (!projectile.IsAlive)
            {
                _activeProjectiles.TryRemove(key, out _);
            }
            else
            {
                _activeProjectiles[key] = projectile;
            }
        }
    }

    private static uint ComputeDefaultLifetimeMs(float range, float speed)
    {
        float computedMs = ((range / Math.Max(speed, 0.001f)) * 1000f) + 0.5f;
        uint ms = (uint)Math.Max(0f, computedMs);
        return ms == 0 ? 1u : Math.Min(ms, 5000u);
    }

    private static Vector3 ComputeLinearPosition(ActiveProjectile proj, uint elapsedMs)
    {
        float t = Math.Min(1f, (float)elapsedMs / proj.LifetimeMs);
        return Vector3.Lerp(proj.StartPosition, proj.EndPosition, t);
    }

    private static Vector3 ComputeParabolicPosition(ref ActiveProjectile proj, uint elapsedMs)
    {
        uint clampedElapsedMs = Math.Min(elapsedMs, proj.LifetimeMs);
        float t = clampedElapsedMs / 1000.0f;
        var gravityAccel = new Vector3(0f, 0f, -proj.Ammo.Gravity);
        proj.Velocity = proj.InitialVelocity + (gravityAccel * t);
        return proj.StartPosition + (proj.InitialVelocity * t) + (gravityAccel * (0.5f * t * t));
    }

    private static float CurrentRadius(ActiveProjectile proj, uint elapsedMs)
    {
        float radius = proj.Ammo.InitialRadius + (proj.Ammo.RadiusDt * (elapsedMs / 1000f));
        if (proj.MaxRadius > 0.001f && proj.MaxRadius <= radius)
        {
            radius = proj.MaxRadius;
        }

        return radius;
    }

    private static uint AbilityFor(Ammo ammo, ProjectileHitReason reason)
    {
        return reason switch
        {
            ProjectileHitReason.Impact => ammo.AbilityId,
            ProjectileHitReason.Touch => ammo.TouchAbilityId,
            ProjectileHitReason.Airburst => ammo.AirburstAbilityId,
            ProjectileHitReason.Period => ammo.PeriodAbilityId,
            _ => 0,
        };
    }

    private static Vector3 SegmentDirection(ActiveProjectile proj)
    {
        Vector3 segment = proj.CurrentPosition - proj.PreviousPosition;
        if (segment.LengthSquared() > 1e-8f)
        {
            return Vector3.Normalize(segment);
        }

        return proj.Direction.LengthSquared() > 1e-8f ? Vector3.Normalize(proj.Direction) : -Vector3.UnitZ;
    }

    private uint GetStartTime(uint? fireTime)
    {
        uint now = _shard.CurrentTime;
        if (fireTime is not { } time)
        {
            return now;
        }

        int behind = (int)(now - time);
        if (behind <= 0)
        {
            return now;
        }

        return behind > _maxCatchUpMs ? now - _maxCatchUpMs : time;
    }

    private Vector3 UpdateHoming(ref ActiveProjectile proj, uint elapsedMs, Vector3 basePosition)
    {
        if (proj.TargetEntityId != 0 && _shard.Entities.TryGetValue(proj.TargetEntityId, out var target))
        {
            var targetPos = target.Position;
            var toTarget = Vector3.Normalize(targetPos - basePosition);
            float homingStrength = proj.Ammo.HomingStrength * 0.01f;
            proj.Velocity = Vector3.Lerp(proj.Velocity, homingStrength * toTarget, 0.1f);
            basePosition += proj.Velocity * 0.05f;
        }
        else
        {
            float t = Math.Min(1f, (float)elapsedMs / proj.LifetimeMs);
            basePosition = Vector3.Lerp(proj.StartPosition, proj.EndPosition, t);
        }

        return basePosition;
    }

    private bool TryBounce(ref ActiveProjectile proj, SegmentRaycastHit hit)
    {
        if (proj.BouncesRemaining <= 0)
        {
            return false;
        }

        var velocityDir = Vector3.Normalize(proj.Velocity);
        var bounceAngle = Vector3.Dot(-velocityDir, hit.Normal);

        float threshold = proj.Ammo.BounceCos;
        if (hit.Normal.Z < 1f)
        {
            threshold = proj.Ammo.SlopeBounceCos;
        }

        if (bounceAngle < threshold)
        {
            return false;
        }

        var reflected = Vector3.Reflect(velocityDir, hit.Normal);

        // BounceFriction=1 means no friction (full speed preserved)
        // TODO: If bounces feel wrong, try: elasticity * friction (treats as direct multiplier)
        var speedScale = proj.Ammo.BounceElasticity * (2f - proj.Ammo.BounceFriction);
        proj.Velocity = reflected * proj.Ammo.ProjectileSpeed * Math.Max(speedScale, 0f);
        proj.CurrentPosition = hit.HitPosition;
        proj.BouncesRemaining--;

        return true;
    }

    private AptitudeTargets TargetsFor(ulong entityId)
    {
        var targets = new AptitudeTargets();
        if (entityId != 0 && _shard.Entities.TryGetValue(entityId, out var entity) && entity is IAptitudeTarget target)
        {
            targets.Push(target);
        }

        return targets;
    }

    private void ProcessOverlaps(ref ActiveProjectile proj, float radius, uint time, uint elapsedMs)
    {
        var overlaps = _shard.Physics.QueryEntityIdsInSphere(proj.CurrentPosition, radius, proj.EntityId);
        if (overlaps.Count == 0)
        {
            return;
        }

        if (proj.Ammo.FlakDurationMax >= 0)
        {
            if (proj.IsAlive && elapsedMs >= proj.Ammo.FlakDurationArming)
            {
                int min = proj.Ammo.FlakDurationMin;
                int max = proj.Ammo.FlakDurationMax;
                int delay = min < max ? min + Random.Shared.Next(max - min) : min;
                proj.DetonateTime = time + (uint)Math.Max(0, delay);
                proj.DetonateReason = ProjectileHitReason.Airburst;
                _logger.Debug("Projectile trace={Trace} flak armed by {Count} overlaps, detonating in {Delay}ms", proj.TraceId, overlaps.Count, delay);
            }

            return;
        }

        var source = GetSourceEntity(proj);
        if (source == null)
        {
            return;
        }

        proj.LastTouchTimes ??= new Dictionary<ulong, uint>();
        uint bucket = proj.Ammo.PeriodDamageMs != 0 ? time - ((time - proj.StartTime) % proj.Ammo.PeriodDamageMs) : time;
        bool stop = false;
        foreach (ulong entityId in overlaps)
        {
            if (proj.LastTouchTimes.TryGetValue(entityId, out uint last) && (int)(last - bucket) >= 0)
            {
                continue;
            }

            if (!_shard.Entities.TryGetValue(entityId, out var entity) || entity is not IAptitudeTarget)
            {
                continue;
            }

            proj.HitCount++;
            if (proj.Ammo.MaxHits != 0 && proj.HitCount >= proj.Ammo.MaxHits)
            {
                stop = true;
            }

            proj.LastTouchTimes[entityId] = bucket;
            ApplyHit(source, proj, ProjectileHitReason.Touch, time, entityId, proj.CurrentPosition);
        }

        if (stop)
        {
            proj.IsAlive = false;
            proj.HitsRemaining = 0;
            _logger.Debug("Projectile trace={Trace} spent after {Hits} touches", proj.TraceId, proj.HitCount);
        }
    }

    private void Detonate(ActiveProjectile proj, ProjectileHitReason reason, uint time)
    {
        var source = GetSourceEntity(proj);
        if (source != null)
        {
            ApplyHit(source, proj, reason, time, 0, proj.CurrentPosition);
        }
    }

    private void ApplyHit(CharacterEntity source, ActiveProjectile proj, ProjectileHitReason reason, uint time, ulong entityId, Vector3 position)
    {
        uint abilityId = AbilityFor(proj.Ammo, reason);
        if (abilityId != 0)
        {
            ActivateProjectileAbility(source, proj, reason, abilityId, time, TargetsFor(entityId), position);
            return;
        }

        var hit = new SegmentRaycastHit
        {
            Hit = true,
            HitPosition = position,
            Normal = -SegmentDirection(proj),
            HitEntityId = entityId,
            Collidable = new CollidableReference(new StaticHandle(0)),
        };

        float splashRadius = reason == ProjectileHitReason.Touch ? 0f : proj.ImpactRadius;
        _logger.Debug("Projectile trace={Trace} {Reason} has no ability, applying {Damage} damage to entity={Entity} splash={Splash}", proj.TraceId, reason, proj.Damage, entityId, splashRadius);
        _shard.Physics.HandleProjectileImpact(source, proj.TraceId, hit, splashRadius, proj.Damage, proj.DamageType, proj.IsAbilityProjectile);
    }

    private void ActivateProjectileAbility(CharacterEntity source, ActiveProjectile proj, ProjectileHitReason reason, uint abilityId, uint time, AptitudeTargets targets, Vector3 position, bool critical = false)
    {
        _logger.Debug("Projectile trace={Trace} {Reason} activating ability {AbilityId} for {Source} at {Position} with {TargetCount} targets, critical={Critical}", proj.TraceId, reason, abilityId, source, position, targets.Count, critical);

        int damagePoints = (int)MathF.Round(proj.Damage, MidpointRounding.AwayFromZero);
        byte damageType = proj.DamageType;
        float scaleMultiplier = proj.ImpactRadius;
        Vector3 direction = SegmentDirection(proj);

        _shard.Abilities.HandleActivateAbility(
            _shard,
            source,
            abilityId,
            time,
            targets,
            proj.SourceExecutionId,
            position,
            initiate: context =>
            {
                context.InitTime = time;
                context.DamagePoints = damagePoints;
                context.DamageType = damageType;
                context.ScaleMultiplier = scaleMultiplier;
                context.Critical = critical;
                context.InitBodyPosition = position;
                context.InitAimOrigin = position;
                context.InitAimDirection = direction;
            });
    }

    private CharacterEntity GetSourceEntity(ActiveProjectile proj)
    {
        if (_shard.Entities.TryGetValue(proj.EntityId, out var entity))
        {
            return entity as CharacterEntity;
        }

        return null;
    }

    // Original-server monster attacks write a ProjectileHitReported message on the shooter's CombatView for each shot.
    // The two trailing bytes are still unknown; zero is the observed default for ordinary single hits.
    private void SendProjectileHitReported(CharacterEntity shooter, uint traceId, uint time)
    {
        var message = new ProjectileHitReported
        {
            TraceRef = (ushort)traceId,
            ShortTime = (ushort)time,
            SegmentFraction = 0,
            Unk3 = 0,
        };

        _shard.EntityMan.SendToScopedReliable(shooter, message);
    }

    private void SendDebugSpawn(CharacterEntity entity, uint traceId, Vector3 origin, Vector3 direction, float speed)
    {
        _debugCallbacks?.SendDebugProjectileSpawn(entity, traceId, origin, direction, speed);
    }

    private void SendDebugBounce(ActiveProjectile proj, Vector3 position, Vector3 normal)
    {
        var source = GetSourceEntity(proj);
        if (source != null)
        {
            _debugCallbacks?.SendDebugProjectileBounce(source, proj.TraceId, position, normal);
        }
    }

    private void SendDebugTimeout(ActiveProjectile proj, Vector3 position)
    {
        var source = GetSourceEntity(proj);
        if (source != null)
        {
            var timeoutDirection = Vector3.Normalize(proj.Direction);
            _debugCallbacks?.SendDebugProjectileTimeout(source, proj.TraceId, position, timeoutDirection);
        }
    }
}
