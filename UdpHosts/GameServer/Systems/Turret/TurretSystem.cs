using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using AeroMessages.GSS.Turret.Event;
using GameServer.Entities.Character;
using GameServer.Entities.Deployable;
using GameServer.Entities.Turret;
using GameServer.Physics;
using GameServer.StaticDB;
using GameServer.StaticDB.Records.dbitems;
using GameServer.Systems.Combat;
using Serilog;

namespace GameServer.Systems.Turret;

/// <summary>
///     Drives NPC-piloted Engineer turrets. The deployable's construction ability spawns a hidden gunner NPC
///     (NPCSpawn) and binds it to the turret with status effect 1368 (TurretControl); from then on this system slews
///     the turret's gun onto hostiles and fires its <c>dbcharacter::TurretWeapon</c>, the same way a manned turret is
///     driven by its player's PoseUpdate plus fire input.
/// </summary>
public sealed class TurretSystem
{
    // The gun only starts firing once it has slewed to within this angle of the target.
    private const float _aimToleranceDegrees = 4f;

    // TurretWeapon.physical_origin is (0,0,0) for the FX_Muzzle turrets, meaning the real muzzle comes out of the
    // visual's hardpoint. Fall back to a chest-height offset so shots clear the deployable's own collision.
    private const float _defaultMuzzleHeight = 1.5f;

    private const float _targetChestHeight = 1.0f;

    // Slew rate used when dbcharacter::Turret.max_rotation_rate is unset (degrees per second).
    private const float _defaultRotationRateDeg = 120f;

    // Idle scan, measured from the 1962 replays: an unengaged turret holds one yaw anchor for a few seconds,
    // slews to the next one 60 degrees away, holds it, and comes back. The anchors are the rest heading and a
    // side, and the turret keeps returning through rest - the 0 <-> -60 and 0 <-> +60 pairs dominate every
    // transition count. Median dwell is 3.5s at the sweep anchor and 4.2s at rest, slew runs at 100-180 deg/s.
    private const float _scanSweepDegrees = 60f;
    private const float _scanDwellMs = 3500f;
    private const float _scanDwellJitterMs = 900f;

    // The gun's rest direction is the turret's +Y axis, and every scanning turret holds its gun 3.67 degrees down at
    // the anchors (a turret parked at rest carries the pose q_x(3.67deg) * q_z(0)).
    private const float _scanRestPitchDegrees = -3.67f;

    private const double _tickIntervalMs = PhysicsEngine.TimeStepMs;

    private readonly IShard _shard;
    private readonly ILogger _logger;
    private readonly HostilityResolver _hostility;
    private readonly Dictionary<uint, TurretWeaponProfile> _profiles;
    private readonly Dictionary<uint, bool> _profileFailures;
    private readonly Dictionary<ulong, TurretGunnerState> _states;
    private double _accumulator;

    public TurretSystem(IShard shard)
    {
        _shard = shard;
        _logger = shard.Logger.ForContext<TurretSystem>();
        _hostility = shard.Hostility;
        _profiles = [];
        _profileFailures = [];
        _states = [];
    }

    public void Tick(double deltaTimeMs, ulong currentTime, CancellationToken ct)
    {
        _accumulator += deltaTimeMs;
        if (_accumulator < _tickIntervalMs)
        {
            return;
        }

        _accumulator = Math.Min(_accumulator - _tickIntervalMs, _tickIntervalMs);
        var dt = (float)(_tickIntervalMs * 0.001);

        foreach (var entity in _shard.Entities.Values)
        {
            if (entity is not TurretEntity { IsNpcControlled: true } turret)
            {
                continue;
            }

            if (turret.Parent is DeployableEntity { IsDead: true } || turret.NpcGunner is not { IsAlive: true })
            {
                continue;
            }

            TickTurret(turret, dt);
        }
    }

    private static Vector3 MuzzlePosition(TurretEntity turret, TurretWeaponProfile profile)
    {
        var offset = new Vector3(profile.MuzzleX, profile.MuzzleY, profile.MuzzleZ);

        if (offset == Vector3.Zero)
        {
            return turret.Position + new Vector3(0, 0, _defaultMuzzleHeight);
        }

        return turret.Position + Vector3.Transform(offset, turret.Orientation);
    }

    private static Vector3 TargetCenter(CharacterEntity target)
    {
        return new Vector3(target.Position.X, target.Position.Y, target.Position.Z + _targetChestHeight);
    }

    /// <summary>
    ///     PIN's yaw convention: an entity facing the world direction <c>dir</c> carries
    ///     <c>q_z(atan2(dir.X, dir.Y))</c>, so the angle is measured off +Y toward +X rather than the other way
    ///     around. <see cref="NpcMovementSystem"/> uses it for NPC facing, and the turret poses are in the same space.
    /// </summary>
    /// <param name="direction">The world direction to measure the yaw of.</param>
    /// <returns>The yaw angle in PIN's facing convention (off +Y toward +X).</returns>
    private static float FacingAngle(Vector3 direction)
    {
        return MathF.Atan2(direction.X, direction.Y);
    }

    /// <summary>
    ///     The facing angle a deployable's orientation quaternion encodes, in <see cref="FacingAngle"/>'s space.
    ///     Deployables stay upright, so only the Z component carries anything.
    /// </summary>
    /// <param name="orientation">The deployable's orientation quaternion.</param>
    /// <returns>The facing angle the orientation encodes, in <see cref="FacingAngle"/>'s space.</returns>
    private static float EntityFacing(Quaternion orientation)
    {
        return 2f * MathF.Atan2(orientation.Z, orientation.W);
    }

    /// <summary>
    ///     Builds <c>Turret::ObserverView.CurrentPose</c>: the turret's rotation in its own frame, a yaw about Z with
    ///     a rotation about X on top of it. The capture's idle anchors are exactly q_x(3.67deg) * q_z(0),
    ///     q_x(3.67deg) * q_z(-60deg) and q_x(3.67deg) * q_z(+60deg). The X rotation runs opposite to the gun's
    ///     elevation, hence the negation.
    /// </summary>
    /// <param name="yaw">The gun's yaw about Z.</param>
    /// <param name="pitch">The gun's elevation (negated into the X rotation).</param>
    /// <returns>The turret's current-pose rotation in its own frame.</returns>
    private static Quaternion PoseRotation(float yaw, float pitch)
    {
        return Quaternion.CreateFromAxisAngle(Vector3.UnitX, -pitch) *
               Quaternion.CreateFromAxisAngle(Vector3.UnitZ, yaw);
    }

    /// <summary>
    ///     Where the gun points in the world for a given pose. The capture shows the pose yaw running opposite to the
    ///     world aim heading - as the pose yaw fell from 15.9deg to 3.65deg the aim heading rose from 101deg to
    ///     113deg - so the pose adds to the facing angle rather than subtracting from it.
    /// </summary>
    /// <param name="yaw">The pose yaw.</param>
    /// <param name="pitch">The pose pitch (elevation).</param>
    /// <param name="facing">The turret's own facing angle.</param>
    /// <returns>The world-space direction the gun points for the given pose.</returns>
    private static Vector3 PoseAim(float yaw, float pitch, float facing)
    {
        var angle = facing + yaw;
        var cosPitch = MathF.Cos(pitch);
        return new Vector3(MathF.Sin(angle) * cosPitch, MathF.Cos(angle) * cosPitch, MathF.Sin(pitch));
    }

    /// <summary>
    ///     The pose angles that point the gun along a world aim direction: the pose yaw is the aim's facing angle
    ///     relative to the turret's own, the pitch is the elevation above the horizontal.
    /// </summary>
    /// <param name="aim">The world aim direction to point along.</param>
    /// <param name="facing">The turret's own facing angle.</param>
    /// <returns>The pose (yaw, pitch) that points the gun along the aim.</returns>
    private static (float Yaw, float Pitch) PoseAngles(Vector3 aim, float facing)
    {
        var yaw = WrapAngle(FacingAngle(aim) - facing);
        var pitch = MathF.Atan2(aim.Z, MathF.Sqrt((aim.X * aim.X) + (aim.Y * aim.Y)));
        return (yaw, pitch);
    }

    private static float ApproachAngle(float current, float target, float maxDelta)
    {
        var delta = WrapAngle(target - current);
        return Math.Abs(delta) <= maxDelta ? target : current + (Math.Sign(delta) * maxDelta);
    }

    private static bool AnglesClose(TurretGunnerState state, float yaw, float pitch)
    {
        var tolerance = DegreesToRadians(_aimToleranceDegrees);
        return Math.Abs(WrapAngle(yaw - state.Yaw)) <= tolerance && Math.Abs(pitch - state.Pitch) <= tolerance;
    }

    private static float WrapAngle(float angle)
    {
        return ((angle + MathF.PI) % (2f * MathF.PI)) - MathF.PI;
    }

    private static float DegreesToRadians(float degrees)
    {
        return degrees * (MathF.PI / 180f);
    }

    private static float ResolveRotationRate(uint turretTypeId)
    {
        var turret = SDBInterface.GetTurret(turretTypeId);
        return turret is { MaxRotationRate: > 0 } ? turret.MaxRotationRate : _defaultRotationRateDeg;
    }

    private void TickTurret(TurretEntity turret, float dt)
    {
        var profile = ResolveProfile(turret.Type);
        if (profile == null)
        {
            return;
        }

        var state = GetState(turret, profile);

        var muzzle = MuzzlePosition(turret, profile);
        var target = FindTarget(turret, muzzle, profile.Weapon.Range);
        if (target == null)
        {
            var lostTarget = state.Target != null;
            state.Target = null;
            TickIdleScan(turret, state, dt, lostTarget);
            return;
        }

        state.Target = target;
        state.ScanDwellUntil = 0;

        // The pose is the turret's rotation relative to how it was placed, so both angles are compared in the
        // facing-angle space the turret's own orientation is expressed in.
        var aim = Vector3.Normalize(TargetCenter(target) - muzzle);
        var facing = EntityFacing(turret.Orientation);
        var (desiredYaw, desiredPitch) = PoseAngles(aim, facing);

        // Slew toward the target instead of snapping, so the gun traverses on the client like a manned turret's.
        var slew = DegreesToRadians(ResolveRotationRate(turret.Type) * dt);
        state.Yaw = ApproachAngle(state.Yaw, desiredYaw, slew);
        state.Pitch = ApproachAngle(state.Pitch, desiredPitch, slew);

        var aligned = AnglesClose(state, desiredYaw, desiredPitch);
        turret.SetAimRotation(PoseRotation(state.Yaw, state.Pitch));

        if (!aligned || !HasLineOfSight(turret, muzzle, target))
        {
            return;
        }

        if (_shard.CurrentTimeLong < state.NextBurstTime)
        {
            return;
        }

        var rounds = (byte)Math.Max(1, (int)profile.Weapon.RoundsPerBurst);
        if (state.Ammo < rounds)
        {
            // Turrets carry an effectively unlimited reserve: an empty clip only costs the reload.
            state.NextBurstTime = _shard.CurrentTimeLong + Math.Max(1u, profile.Weapon.ReloadTime);
            state.Ammo = profile.ClipSize;
            turret.SetAmmo([state.Ammo]);
            _logger.Debug("Turret {Turret} reloaded {Rounds} rounds", turret.EntityId, state.Ammo);
            return;
        }

        FireBurst(turret, state, profile, muzzle, PoseAim(state.Yaw, state.Pitch, facing), rounds);

        state.NextBurstTime = _shard.CurrentTimeLong + Math.Max(1u, profile.Weapon.MsPerBurst);
        state.Ammo -= rounds;
        turret.SetAmmo([state.Ammo]);
    }

    /// <summary>
    ///     What the original server does with a turret that has nothing to shoot at: it parks the gun on a yaw
    ///     anchor, waits a few seconds, then slews it to the anchor 60 degrees away and waits again. The turret
    ///     always comes back through rest on its way to the other side, which is the scanning sweep the turrets
    ///     visibly do.
    /// </summary>
    /// <param name="turret">The turret being parked/scanned.</param>
    /// <param name="state">The gunner state (holds the current yaw anchor and timers).</param>
    /// <param name="dt">The timestep, in seconds.</param>
    /// <param name="lostTarget">True on the tick the turret just lost its target.</param>
    private void TickIdleScan(TurretEntity turret, TurretGunnerState state, float dt, bool lostTarget)
    {
        if (lostTarget)
        {
            // A turret that just lost its target walks back onto its rest heading before starting to scan again.
            state.ScanYaw = 0f;
            state.ScanDwellUntil = _shard.CurrentTimeLong + (ulong)_scanDwellMs;
        }

        var onAnchor = Math.Abs(WrapAngle(state.Yaw - state.ScanYaw)) <= DegreesToRadians(_aimToleranceDegrees);
        if (onAnchor && _shard.CurrentTimeLong >= state.ScanDwellUntil)
        {
            if (Math.Abs(state.ScanYaw) < 0.001f)
            {
                // Leaving rest: sweep to this excursion's side, and queue the other side for the next one, so the
                // gun walks rest -> left -> rest -> right -> rest instead of beating on one side.
                state.ScanYaw = state.ScanSide;
                state.ScanSide = -state.ScanSide;
            }
            else
            {
                state.ScanYaw = 0f;
            }

            var jitter = state.ScanCycle++ % 2 == 0 ? -_scanDwellJitterMs : _scanDwellJitterMs;
            state.ScanDwellUntil = _shard.CurrentTimeLong + (ulong)(_scanDwellMs + jitter);
        }

        var slew = DegreesToRadians(ResolveRotationRate(turret.Type) * dt);
        state.Yaw = ApproachAngle(state.Yaw, state.ScanYaw, slew);
        state.Pitch = ApproachAngle(state.Pitch, DegreesToRadians(_scanRestPitchDegrees), slew);
        turret.SetAimRotation(PoseRotation(state.Yaw, state.Pitch));
    }

    private void FireBurst(TurretEntity turret, TurretGunnerState state, TurretWeaponProfile profile, Vector3 origin, Vector3 aim, byte rounds)
    {
        var gunner = turret.NpcGunner;
        var weapon = profile.Weapon;
        var ammo = profile.Ammo;
        var time = _shard.CurrentTime;

        // The ObserverView burst pair is the client's "this turret started shooting" signal, mirroring the CombatView
        // stamp WeaponSim writes for an NPC character.
        turret.SetFireBurst(time);
        turret.SetFireEnd(time + Math.Max(1u, weapon.MsBurstDuration));

        var aimForward = Vector3.Normalize(aim);
        var aimRight = Vector3.Normalize(Vector3.Cross(aimForward, Vector3.UnitZ));
        var aimUp = Vector3.Normalize(Vector3.Cross(aimRight, aimForward));

        // The aim sample lets clients place the tracer/muzzle flash on the turret's gun.
        _shard.EntityMan.SendToScopedReliable(turret, new WeaponProjectileFired
        {
            Time = time,
            Aim = aimForward,
            HaveShooterVelocity = 0,
        });

        for (byte round = 0; round < rounds; round++)
        {
            PRNG.PRNG.Spread(time, 0, round, aimForward, aimRight, aimUp, weapon.StartingSpread, Vector3.Zero, 0, out var direction);
            var trace = PRNG.PRNG.Trace(time, round);
            _shard.ProjectileSim.FireProjectile(
                gunner,
                trace,
                origin,
                direction,
                ammo,
                weapon.Range,
                ammo.ProjectileSpeed,
                ammo.ImpactRadius,
                ammo.MaxRadius,
                weapon.DamagePerRound);
        }

        _logger.Debug(
            "Turret {Turret} fired {Rounds} round(s) at {Target} (dist {Distance})",
            turret.EntityId,
            rounds,
            state.Target?.EntityId,
            Vector3.Distance(origin, state.Target?.Position ?? origin));
    }

    private CharacterEntity FindTarget(TurretEntity turret, Vector3 muzzle, float range)
    {
        CharacterEntity best = null;
        var bestDistance = float.MaxValue;

        foreach (var entity in _shard.Entities.Values)
        {
            if (entity is not CharacterEntity { IsAlive: true } candidate)
            {
                continue;
            }

            // Hidden NPCs are internal drivers (other turrets' gunners); they are never shot at.
            if (candidate.IsHidden || !_hostility.CanDamage(turret, candidate))
            {
                continue;
            }

            var distance = Vector3.Distance(muzzle, TargetCenter(candidate));
            if (distance > range || distance >= bestDistance)
            {
                continue;
            }

            bestDistance = distance;
            best = candidate;
        }

        return best;
    }

    private bool HasLineOfSight(TurretEntity turret, Vector3 muzzle, CharacterEntity target)
    {
        var hit = _shard.Physics.SegmentRayCast(muzzle, TargetCenter(target), turret.EntityId, ignoreNpcs: true);
        return !hit.Hit || hit.HitEntityId == target.EntityId;
    }

    private TurretWeaponProfile ResolveProfile(uint turretTypeId)
    {
        if (_profiles.TryGetValue(turretTypeId, out var cached))
        {
            return cached;
        }

        if (_profileFailures.ContainsKey(turretTypeId))
        {
            return null;
        }

        var rows = SDBInterface.GetTurretWeapons(turretTypeId);
        if (rows == null || rows.Count == 0)
        {
            _logger.Warning("Turret type {TurretType} has no dbcharacter::TurretWeapon rows, it cannot fire", turretTypeId);
            _profileFailures[turretTypeId] = true;
            return null;
        }

        var row = rows[0];
        var info = SDBUtils.GetDetailedWeaponInfo(row.WeaponId);
        var ammo = info?.Main == null ? null : SDBInterface.GetAmmo(info.Main.AmmoId);
        if (info?.Main == null || ammo == null)
        {
            _logger.Warning("Turret type {TurretType} weapon {WeaponId} could not be resolved, it cannot fire", turretTypeId, row.WeaponId);
            _profileFailures[turretTypeId] = true;
            return null;
        }

        var profile = new TurretWeaponProfile
        {
            Weapon = info.Main,
            Ammo = ammo,
            MuzzleX = row.PhysicalOrigin.x,
            MuzzleY = row.PhysicalOrigin.y,
            MuzzleZ = row.PhysicalOrigin.z,
            ClipSize = info.Main.BaseClipSize > 0 ? info.Main.BaseClipSize : (ushort)Math.Max(1, (int)info.Main.MaxAmmo),
        };

        _profiles[turretTypeId] = profile;
        return profile;
    }

    private TurretGunnerState GetState(TurretEntity turret, TurretWeaponProfile profile)
    {
        if (_states.TryGetValue(turret.EntityId, out var state))
        {
            return state;
        }

        // The gun starts on the rest heading, holding the pitch every scanning turret in the capture holds at its
        // anchors, and dwells there before the first sweep.
        state = new TurretGunnerState
        {
            Ammo = profile.ClipSize,
            Pitch = DegreesToRadians(_scanRestPitchDegrees),
            ScanYaw = 0f,
            ScanSide = DegreesToRadians((turret.EntityId & 1) == 0 ? -_scanSweepDegrees : _scanSweepDegrees),
            ScanDwellUntil = _shard.CurrentTimeLong + (ulong)_scanDwellMs,
        };

        _states[turret.EntityId] = state;
        turret.SetAmmo([state.Ammo]);
        return state;
    }

    private sealed class TurretWeaponProfile
    {
        public WeaponTemplateResult Weapon;
        public Ammo Ammo;
        public float MuzzleX;
        public float MuzzleY;
        public float MuzzleZ;
        public ushort ClipSize;
    }

    private sealed class TurretGunnerState
    {
        public CharacterEntity Target;
        public ulong NextBurstTime;

        // The gun's pose angles, in the turret's own frame (yaw off its +Y rest axis, pitch above its XY plane).
        public float Yaw;
        public float Pitch;

        // Idle scan: the anchor the gun is parked on, when it may move again, and which side the next excursion
        // sweeps to (it flips each time the turret leaves rest).
        public float ScanYaw;
        public ulong ScanDwellUntil;
        public float ScanSide;
        public int ScanCycle;
        public ushort Ammo;
    }
}
