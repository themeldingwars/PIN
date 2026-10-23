using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Numerics;
using System.Threading;
using AeroMessages.GSS.Character.Event;
using GameServer.Entities.Character;
using GameServer.Enums;
using GameServer.Physics;
using GameServer.StaticDB.Records.dbitems;
using GameServer.Systems.Aptitude;

namespace GameServer.Systems.ProjectileSim;

public class ProjectileSim
{
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

    public void FireProjectile(CharacterEntity entity, uint trace, Vector3 origin, Vector3 direction, Ammo ammo, float range, float projectileSpeed, float impactRadius, float maxRadius, float damage, bool isAbilityProjectile = false, Guid? sourceExecutionId = null)
    {
        var ammoFlags = new AmmoFlags(ammo.Flags);
        bool isDrunk = DrunkMissile.IsActive(ammo);

        var velocity = projectileSpeed * direction;
        float actualSpeed = velocity.Length();
        var lifetimeMs = ammo.ConstLifetime > 0
            ? ammo.ConstLifetime
            : ComputeDefaultLifetimeMs(range, actualSpeed);
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
            StartTime = _shard.CurrentTime,
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
            NextPeriodAbilityTime = ammo.PeriodAbilityMs > 0 ? _shard.CurrentTime + ammo.PeriodAbilityMs : 0,
            SourceExecutionId = sourceExecutionId
        };

        _activeProjectiles.TryAdd((entity.EntityId, trace), projectile);
        _logger.Debug("Spawned {Type} projectile trace={Trace}, speed={Speed}, range={Range}, lifetime={Lifetime}ms, impactRadius={ImpactRadius}, maxRadius={MaxRadius}, damage={Damage}", ammoFlags.Simulation, trace, projectileSpeed, range, lifetimeMs, impactRadius, maxRadius, damage);
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
                    _shard.Abilities.HandleActivateAbility(_shard, periodSource, projectile.Ammo.PeriodAbilityId, (uint)currentTime, new AptitudeTargets(), projectile.SourceExecutionId, projectile.CurrentPosition);
                    projectile.NextPeriodAbilityTime += projectile.Ammo.PeriodAbilityMs;
                }
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

                        _shard.Physics.HandleProjectileImpact(source, projectile.TraceId, hit, projectile.ImpactRadius, projectile.Damage, projectile.DamageType, projectile.IsAbilityProjectile);

                        if (projectile.Ammo.AbilityId != 0)
                        {
                            var targets = new AptitudeTargets();
                            if (hit.HitEntityId != 0 && _shard.Entities.TryGetValue(hit.HitEntityId, out var hitEntity) && hitEntity is IAptitudeTarget hitTarget)
                            {
                                targets.Push(hitTarget);
                            }

                            _shard.Abilities.HandleActivateAbility(_shard, source, projectile.Ammo.AbilityId, (uint)currentTime, targets, projectile.SourceExecutionId, hit.HitPosition);
                        }
                    }
                }
            }

            if (elapsedMs >= projectile.LifetimeMs)
            {
                if (projectile.Ammo.AirburstAbilityId != 0)
                {
                    var airburstSource = GetSourceEntity(projectile);
                    if (airburstSource != null)
                    {
                        _shard.Abilities.HandleActivateAbility(_shard, airburstSource, projectile.Ammo.AirburstAbilityId, (uint)currentTime, new AptitudeTargets(), projectile.SourceExecutionId, projectile.CurrentPosition);
                    }
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
