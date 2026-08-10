using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Numerics;
using System.Threading;
using GameServer.Entities.Character;
using GameServer.Enums;
using GameServer.Physics;
using GameServer.StaticDB.Records.dbitems;

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

    public void FireProjectile(CharacterEntity entity, uint trace, Vector3 origin, Vector3 direction, Ammo ammo, float range, float projectileSpeed, float impactRadius, float maxRadius)
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
            IsDrunk = isDrunk
        };

        _activeProjectiles.TryAdd((entity.EntityId, trace), projectile);
        _logger.Debug("Spawned {Type} projectile trace={Trace}, speed={Speed}, range={Range}, lifetime={Lifetime}ms, impactRadius={ImpactRadius}, maxRadius={MaxRadius}", ammoFlags.Simulation, trace, projectileSpeed, range, lifetimeMs, impactRadius, maxRadius);
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
                        _shard.Physics.HandleProjectileImpactDebug(source, projectile.TraceId, hit);
                    }
                }
            }

            if (elapsedMs >= projectile.LifetimeMs)
            {
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
