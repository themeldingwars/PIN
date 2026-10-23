using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using System.Threading;
using AeroMessages.GSS.Character.Event;
using AeroMessages.GSS.Generic;
using GameServer.Entities;
using GameServer.Entities.Character;
using GameServer.Enums;
using GameServer.Protocol;
using GameServer.StaticDB;
using GameServer.StaticDB.Records.dbitems;
using GameServer.Systems.Aptitude;
using Serilog;

namespace GameServer.Systems.WeaponSim;

public class WeaponSim
{
    // Charge kept when switching between linked fire modes, unverified.
    private const float _chargeSwitchKeepFraction = 1.0f;

    private readonly Dictionary<ulong, WeaponPlayerSim> _weaponSimState;
    private readonly Shard _shard;
    private readonly ILogger _logger;
    private readonly ulong _updateIntervalMs = 50;
    private ulong _lastUpdate;

    public WeaponSim(Shard shard)
    {
        _shard = shard;
        _weaponSimState = [];
        _logger = shard.Logger.ForContext<WeaponSim>();
    }

    public void Tick(double deltaTime, ulong currentTime, CancellationToken ct)
    {
        if (currentTime > _lastUpdate + _updateIntervalMs)
        {
            _lastUpdate = currentTime;
            var entities = GetWeaponSimPlayersEntities();
            foreach (var entity in entities)
            {
                ProcessEntity(entity as CharacterEntity);
            }
        }
    }

    /// <summary>
    ///     Enqueues a timestamped fire event into the per-weapon fire state machine.
    ///     Called from the GSS handlers and, for NPC-driven firing, from the AI behavior system.
    /// </summary>
    /// <param name="entity">Character that owns the weapon</param>
    /// <param name="evt">The event with its client time</param>
    public void EnqueueFireEvent(CharacterEntity entity, WeaponFireEvent evt)
    {
        var sim = GetOrCreatePlayerSim(entity.EntityId);
        InsertEventSorted(sim.Events, evt);
        if (evt.Time > sim.Fire.LatestClientTime)
        {
            sim.Fire.LatestClientTime = evt.Time;
        }
    }

    /// <summary>
    ///     Records a client FireWeaponProjectile as an aim/velocity sample for the fire state machine.
    ///     The sim itself decides when shots fire; this only feeds their aim/origin data.
    /// </summary>
    /// <param name="entity">Character that fires</param>
    /// <param name="time">Client time of the sample</param>
    /// <param name="localAimDir">The aim direction at the time of the sample</param>
    /// <param name="shooterVelocity">The shooter velocity at the time of the sample</param>
    public void OnFireWeaponProjectile(CharacterEntity entity, uint time, Vector3 localAimDir, Vector3? shooterVelocity = null)
    {
        EnqueueFireEvent(entity, new WeaponFireEvent
        {
            Time = time,
            Type = WeaponFireEventType.FireAimSample,
            Aim = localAimDir,
            Velocity = shooterVelocity
        });
    }

    /// <summary>
    ///     Fires an ability projectile using the aim vector from the client's FireWeaponProjectile.
    ///     Homing runs in the projectile simulation and a ballistic solution is applied for arcing
    ///     ammo; only the body/hardpoint origin offset is still missing (see the muzzle stub below).
    /// </summary>
    /// <param name="entity">Character that fires</param>
    /// <param name="pending">Ability projectile info</param>
    /// <param name="time">Time of firing</param>
    /// <param name="localAimDir">The aim direction at the time of firing</param>
    /// <param name="shooterVelocity">The shooter velocity at the time of firing</param>
    public void OnFireAbilityProjectile(CharacterEntity entity, PendingAbilityProjectile pending, uint time, Vector3 localAimDir, Vector3? shooterVelocity = null)
    {
        var ammo = SDBInterface.GetAmmo(pending.AmmoType);
        if (ammo == null)
        {
            _logger.Warning("Will not fire ability projectile because failed to get ammo {AmmoType} (exec {ExecId})", pending.AmmoType, pending.ExecutionId);
            return;
        }

        // TODO: Should we really load from weapon?
        var attrsDict = entity.GetActiveWeaponDetails()?.Attributes;

        float projectileSpeed = ammo.ProjectileSpeed;
        if (ammo.ProjectileSpeedStat != 0 && attrsDict != null && attrsDict.TryGetValue(ammo.ProjectileSpeedStat, out var speedAttr))
        {
            projectileSpeed = speedAttr;
        }

        float impactRadius = ammo.ImpactRadius;
        if (ammo.ImpactRadiusStat != 0 && attrsDict != null && attrsDict.TryGetValue(ammo.ImpactRadiusStat, out var impactAttr))
        {
            impactRadius = impactAttr;
        }

        float maxRadius = ammo.MaxRadius;
        if (ammo.MaxRadiusStat != 0 && attrsDict != null && attrsDict.TryGetValue(ammo.MaxRadiusStat, out var maxRadiusAttr))
        {
            maxRadius = maxRadiusAttr;
        }

        // Parabolic monster ammo needs a ballistic firing solution; a flat muzzle-to-chest aim drops into the ground.
        var preliminaryOrigin = entity.GetProjectileOrigin(pending.QueueTime, localAimDir, shooterVelocity);
        localAimDir = ResolveBallisticAim(entity, preliminaryOrigin, localAimDir, ammo, projectileSpeed);

        // Stub: spawn from the weapon muzzle origin (body/hardpoint offset not implemented yet).
        var origin = entity.GetProjectileOrigin(pending.QueueTime, localAimDir, shooterVelocity);

        var activeWeaponDetails = entity.GetActiveWeaponDetails();
        float damage = pending.UseWeaponDamage && activeWeaponDetails != null && activeWeaponDetails.Weapon != null
            ? ResolveWeaponDamage(activeWeaponDetails.Weapon, attrsDict)
            : pending.Damage;

        byte roundsToFire = pending.BurstCount > 0 ? pending.BurstCount : (byte)1;
        Vector3 aimForward = Vector3.Normalize(localAimDir);
        Vector3 aimRight = Vector3.Normalize(Vector3.Cross(aimForward, Vector3.UnitZ));
        Vector3 aimUp = Vector3.Normalize(Vector3.Cross(aimRight, aimForward));

        // For an NPC the CombatView burst pair is the client's only "this attack started" signal. Stamp it at the
        // ability's logical fire time (which may be delayed by UpdateWaitAndFireOnce), with the single-shot +1ms end
        // used by original-server monster attacks.
        if (!entity.IsPlayerControlled)
        {
            entity.SetFireBurst(time);
            entity.SetFireEnd(time + 1);
        }

        // The original server writes the aim sample before the ability payload (WeaponProjectileFired then
        // AbilityProjectileFired). Players already predicted the shot, so only NPCs need the aim sample.
        if (!entity.IsPlayerControlled)
        {
            BroadcastWeaponProjectileFired(entity, time, localAimDir, shooterVelocity);
        }

        BroadcastAbilityProjectileFired(entity, time, localAimDir, pending, shooterVelocity, roundsToFire);

        for (byte round = 0; round < roundsToFire; round++)
        {
            PRNG.PRNG.Spread(time, 0, round, aimForward, aimRight, aimUp, pending.Spread, Vector3.Zero, 0, out Vector3 direction);
            uint trace = PRNG.PRNG.Trace(time, round);
            _shard.ProjectileSim.FireProjectile(entity, trace, origin, direction, ammo, pending.Range, projectileSpeed, impactRadius, maxRadius, damage, isAbilityProjectile: true, sourceExecutionId: pending.ExecutionId);
        }

        _logger.Debug("Fired ability projectile ammo={AmmoType} range={Range} burst={Burst} entity={Entity} exec={ExecId}", pending.AmmoType, pending.Range, roundsToFire, entity.EntityId, pending.ExecutionId);
    }

    public void RestockAmmo(CharacterEntity entity)
    {
        var sim = GetOrCreatePlayerSim(entity.EntityId);

        var active = entity.GetActiveWeaponDetails();
        if (active?.Weapon != null)
        {
            EnsureAmmoInitialized(sim, active.Weapon, active.WeaponProfile);
            sim.ClipAmmo = sim.ClipMax;
            sim.ReserveAmmo = sim.ReserveMax;
            sim.ClipEmptyTriggered = false;
        }

        // Weapon slot 1 (primary) maps to ammo slot 0, weapon slot 2 (secondary) to ammo slot 1.
        RestockAmmoSlot(entity, 1, 0);
        RestockAmmoSlot(entity, 2, 1);
    }

    public void ResetAmmoState(CharacterEntity entity)
    {
        _weaponSimState.Remove(entity.EntityId);
        RestockAmmo(entity);
    }

    private static float ResolveWeaponDamage(WeaponTemplateResult weapon, IReadOnlyDictionary<ushort, float> attributes)
    {
        float damage = weapon.DamagePerRound;
        if (attributes != null && attributes.TryGetValue((ushort)ItemAttributeId.WeaponDamage, out var damageAttribute))
        {
            damage = damageAttribute;
        }

        return damage;
    }

    private static bool TrySolveBallisticAim(Vector3 origin, Vector3 target, float speed, float gravity, out Vector3 aim)
    {
        aim = Vector3.Zero;
        if (speed <= 0f || gravity <= 0f)
        {
            return false;
        }

        var delta = target - origin;
        double dx = delta.X;
        double dy = delta.Y;
        double dz = delta.Z;
        double horizontal = Math.Sqrt((dx * dx) + (dy * dy));
        double v = speed;
        double g = gravity;

        if (horizontal < 0.01)
        {
            aim = dz >= 0f ? Vector3.UnitZ : -Vector3.UnitZ;
            return true;
        }

        double dirX = dx / horizontal;
        double dirY = dy / horizontal;

        // Solve z = x*tan(theta) - g*x^2/(2*v^2*cos^2(theta)) for tan(theta), using the low arc.
        double a = (g * horizontal * horizontal) / (2.0 * v * v);
        double c = dz + a;
        double discriminant = (horizontal * horizontal) - (4.0 * a * c);

        double tan;
        if (discriminant < 0.0)
        {
            // Out of reach for this speed/gravity pair: aim at the maximum-range angle toward the target instead of
            // firing flat into the ground.
            tan = (v * v) / (g * horizontal);
        }
        else
        {
            double sqrt = Math.Sqrt(discriminant);
            double denominator = horizontal + sqrt;
            tan = denominator > 1e-6 ? (2.0 * c) / denominator : (horizontal - sqrt) / (2.0 * a);
        }

        if (double.IsNaN(tan) || double.IsInfinity(tan))
        {
            return false;
        }

        aim = Vector3.Normalize(new Vector3((float)dirX, (float)dirY, (float)tan));
        return true;
    }

    /// <summary>
    ///     True when the fire mode fires continuously while the trigger is held (resumes after a reload).
    ///     M2 (bolt action / semi) and M4 (charge) need a fresh trigger press.
    /// </summary>
    /// <param name="weapon">The active weapon template.</param>
    /// <returns>True when a held trigger fires continuously; false when each shot needs a fresh press.</returns>
    private static bool IsContinuousFireMode(WeaponTemplateResult weapon)
    {
        // Bolt-action weapons (the BoltAction flag, which is independent of the low-bit burst mode) and the M2/M4 modes
        // require a fresh trigger press per shot: a held trigger must not auto-repeat. This is the server-side guard that
        // stops a held trigger (e.g. an NPC's) from firing a bolt-action weapon more than once per press.
        if (((WeaponFlags)weapon.WeaponFlags).BoltAction)
        {
            return false;
        }

        byte burstMode = (byte)(weapon.WeaponFlags & 7u);
        return burstMode != 2 && burstMode != 4;
    }

    /// <summary>
    ///     True when the active mode is the M1 chargeup weapon: the first shot of a press is
    ///     gated on the weapon charge reaching the armed threshold.
    /// </summary>
    /// <param name="weapon">The active weapon template.</param>
    /// <returns>True when the mode is the M1 chargeup weapon.</returns>
    private static bool IsM1Chargeup(WeaponTemplateResult weapon)
    {
        return (weapon.WeaponFlags & 7u) == 1u && weapon.MsChargeUp > 0;
    }

    /// <summary>
    ///     True when the active mode can never fire (the "spin-up" alt fire): it has no clip
    ///     ammo and no attack ability, so holding it only charges the weapon. The chargeup
    ///     stats are not required on this template — the charge is weapon-wide (main mode's).
    /// </summary>
    /// <param name="weapon">The active weapon template.</param>
    /// <param name="profile">The active weapon profile (clip/ammo capacities).</param>
    /// <returns>True when the mode can never fire and only charges the weapon.</returns>
    private static bool IsChargeOnlyMode(WeaponTemplateResult weapon, WeaponProfile profile)
    {
        bool canFire = (profile.BaseClipSize > 0 && profile.MaxAmmo > 0) || weapon.AttackAbility != 0;
        return !canFire;
    }

    /// <summary>
    ///     Captures the weapon-wide chargeup caps from the main (mode 0) template, falling back
    ///     to the active mode when the main template has no chargeup (e.g. spin-up-only layouts).
    /// </summary>
    /// <param name="player">The player's weapon simulation to store the caps on.</param>
    /// <param name="entity">The character whose weapon details provide the templates.</param>
    /// <param name="activeDetails">The currently active weapon's details (fallback source).</param>
    private static void CaptureChargeupCaps(WeaponPlayerSim player, CharacterEntity entity, CharacterEntity.ActiveWeaponDetails activeDetails)
    {
        WeaponProfile source;
        var mainDetails = entity.GetWeaponDetails(0);
        if (mainDetails != null && mainDetails.Weapon != null && mainDetails.WeaponProfile.MsChargeUp > 0)
        {
            source = mainDetails.WeaponProfile;
        }
        else if (activeDetails.WeaponProfile.MsChargeUp > 0)
        {
            source = activeDetails.WeaponProfile;
        }
        else
        {
            source = mainDetails != null && mainDetails.Weapon != null ? mainDetails.WeaponProfile : activeDetails.WeaponProfile;
        }

        player.ChargeCap = source.MsChargeUp;
        player.ChargeMin = source.MsChargeUpMin;
    }

    /// <summary>
    ///     The client aim/velocity sample for exactly the shot time; none when nothing matches.
    ///     Exact-time matching is deliberate: the shot time also seeds the spread PRNG, so a sample
    ///     from another time would desync both the aim and the spread.
    /// </summary>
    /// <param name="fire">Fire state holding the aim samples</param>
    /// <param name="shotTime">Shot time to find a sample for</param>
    /// <param name="sample">The sample for the shot time, when one exists</param>
    /// <returns>True when a sample for the exact shot time exists</returns>
    private static bool TryGetAimSample(WeaponFireState fire, uint shotTime, out FireAimSample sample)
    {
        foreach (var s in fire.AimSamples)
        {
            if (s.Time == shotTime)
            {
                sample = s;
                return true;
            }
        }

        sample = default;
        return false;
    }

    private static void TrimAimSamples(WeaponFireState fire, uint time)
    {
        while (fire.AimSamples.Count > 0 && time - fire.AimSamples[0].Time > 1000)
        {
            fire.AimSamples.RemoveAt(0);
        }

        if (fire.AimSamples.Count > 32)
        {
            fire.AimSamples.RemoveRange(0, fire.AimSamples.Count - 32);
        }
    }

    private static void InsertEventSorted(List<WeaponFireEvent> events, WeaponFireEvent evt)
    {
        int lo = 0;
        int hi = events.Count - 1;
        while (lo <= hi)
        {
            int mid = (lo + hi) / 2;
            if (events[mid].Time < evt.Time)
            {
                lo = mid + 1;
            }
            else
            {
                hi = mid - 1;
            }
        }

        events.Insert(lo, evt);
    }

    private static void RestockAmmoSlot(CharacterEntity entity, byte weaponSlot, int ammoSlot)
    {
        var details = entity.GetWeaponDetails(weaponSlot, 0);
        if (details?.Weapon == null)
        {
            return;
        }

        ushort clip = details.WeaponProfile.BaseClipSize;
        ushort reserve = details.WeaponProfile.MaxAmmo;

        var view = entity.Character_CombatView;
        if (view != null)
        {
            if (ammoSlot == 0)
            {
                view.Ammo_0Prop = clip;
                view.AltAmmo_0Prop = reserve;
            }
            else
            {
                view.Ammo_1Prop = clip;
                view.AltAmmo_1Prop = reserve;
            }
        }

        var controller = entity.Character_CombatController;
        if (controller != null)
        {
            if (ammoSlot == 0)
            {
                controller.Ammo_0Prop = clip;
                controller.AltAmmo_0Prop = reserve;
            }
            else
            {
                controller.Ammo_1Prop = clip;
                controller.AltAmmo_1Prop = reserve;
            }
        }
    }

    /// <summary>
    ///     Spawns a single weapon projectile directly from the weapon/ammo templates.
    /// </summary>
    /// <param name="entity">Character that fires</param>
    /// <param name="sim">Per-player weapon sim state</param>
    /// <param name="details">Active weapon details (mode template + attributes)</param>
    /// <param name="modeState">Spread state of the active fire mode</param>
    /// <param name="time">Client time of the shot</param>
    /// <param name="aim">Aim direction at shot time</param>
    /// <param name="velocity">Shooter velocity at shot time</param>
    /// <param name="origin">Projectile origin at shot time</param>
    /// <param name="spreadPct">Spread percent at shot time</param>
    /// <param name="aimSource">How the aim was resolved (sample/interpolated/pose)</param>
    private void FireWeaponDirect(CharacterEntity entity, WeaponPlayerSim sim, CharacterEntity.ActiveWeaponDetails details, WeaponModeState modeState, uint time, Vector3 aim, Vector3? velocity, Vector3 origin, float spreadPct, string aimSource)
    {
        var weapon = details.Weapon;
        var spreadProfile = details.WeaponProfile;

        // Resolve weapon attributes: stats from weapon override template defaults
        var attrsDict = details.Attributes;
        float range = weapon.Range;
        if (attrsDict.TryGetValue((ushort)ItemAttributeId.WeaponRange, out var rangeAttr))
        {
            range = rangeAttr;
        }

        // Ammo
        var ammo = SDBInterface.GetAmmo(weapon.AmmoId); // TODO: Handle ammo overrides

        // Ammo stat properties: stat ID from ammo points to weapon attribute to use
        float projectileSpeed = ammo.ProjectileSpeed;
        if (ammo.ProjectileSpeedStat != 0 && attrsDict.TryGetValue(ammo.ProjectileSpeedStat, out var speedAttr))
        {
            projectileSpeed = speedAttr;
        }

        float impactRadius = ammo.ImpactRadius;
        if (ammo.ImpactRadiusStat != 0 && attrsDict.TryGetValue(ammo.ImpactRadiusStat, out var impactAttr))
        {
            impactRadius = impactAttr;
        }

        float maxRadius = ammo.MaxRadius;
        if (ammo.MaxRadiusStat != 0 && attrsDict.TryGetValue(ammo.MaxRadiusStat, out var maxRadiusAttr))
        {
            maxRadius = maxRadiusAttr;
        }

        float damage = ResolveWeaponDamage(weapon, attrsDict);

        // Parabolic monster ammo needs a ballistic firing solution; a flat muzzle-to-chest aim drops into the ground.
        aim = ResolveBallisticAim(entity, origin, aim, ammo, projectileSpeed);
        origin = entity.GetProjectileOrigin(time, aim, velocity);

        byte numBullets = Math.Max((byte)1, weapon.RoundsPerBurst);

        // NPCs have no FireBurst/FireEnd commands; the CombatView burst pair is the client-side attack signal. Stamp a
        // single-shot burst at the authoritative shot time (the AI now presses/releases in 1ms).
        if (!entity.IsPlayerControlled)
        {
            entity.SetFireBurst(time);
            entity.SetFireEnd(time + 1);
        }

        Vector3 aimForward = aim;
        Vector3 aimRight = Vector3.Normalize(Vector3.Cross(aimForward, Vector3.UnitZ));
        Vector3 aimUp = Vector3.Normalize(Vector3.Cross(aimRight, aimForward));
        for (byte numBulletsFired = 0; numBulletsFired < numBullets; numBulletsFired++)
        {
            PRNG.PRNG.Spread(time, weapon.SlotIndex, numBulletsFired, aimForward, aimRight, aimUp, spreadPct, modeState.LastSpreadDirection, modeState.LastSpreadTime, out Vector3 direction);
            uint trace = PRNG.PRNG.Trace(time, numBulletsFired);
            _logger.Debug("FireWeaponDirect time={Time} trace={Trace} aimSource={AimSource}", time, trace, aimSource);
            _shard.ProjectileSim.FireProjectile(entity, trace, origin, direction, ammo, range, projectileSpeed, impactRadius, maxRadius, damage);
            modeState.LastSpreadDirection = direction;
            modeState.LastSpreadTime = time;

            // Broadcast the aim sample to everyone who can see the shooter so clients spawn the projectile visual on the
            // server's aim. This is the single WeaponProjectileFired path for both players and NPCs (which have no
            // client). Fire *timing* (the client animation) is stamped separately on the CombatView via
            // SetFireBurst/SetFireEnd.
            BroadcastWeaponProjectileFired(entity, time, aim, velocity);
        }

        // Add spread/heat for this round of the current press
        uint totalPressRounds = sim.Fire.TotalRoundsInPress > 0 ? sim.Fire.TotalRoundsInPress : 1;
        WeaponSpreadMath.ApplyBurstSpreadUpdate(spreadProfile, modeState, 1, totalPressRounds);

        modeState.LastBurstTime = time;
    }

    // Sends WeaponProjectileFired (the aim sample) to every player that has the shooter scoped in, so clients spawn the
    // projectile visual on the server's aim. Sent on the reliable channel to the entity's scoped players, which includes
    // the firing player (preserving the echo a client expects) plus everyone else who can see the shot.
    private void BroadcastWeaponProjectileFired(CharacterEntity entity, uint time, Vector3 aim, Vector3? velocity)
    {
        var message = new WeaponProjectileFired
        {
            ShortTime = (ushort)time,
            Aim = aim,
            HaveShooterVelocity = (byte)(velocity.HasValue ? 1 : 0),
        };

        if (velocity.HasValue)
        {
            message.ShooterVelocity = velocity.Value;
        }

        _shard.EntityMan.SendToScopedReliable(entity, message);
    }

    // Sends AbilityProjectileFired to every player that has the shooter scoped in, so clients render the ability
    // projectile on the server's aim/ammo. Mirrors BroadcastWeaponProjectileFired for weapon shots.
    private void BroadcastAbilityProjectileFired(CharacterEntity entity, uint time, Vector3 aim, PendingAbilityProjectile pending, Vector3? velocity, byte burstCount)
    {
        var message = new AbilityProjectileFired
        {
            ShortTime = (ushort)time,
            OriginOffset = Vector3.Zero,
            Aim = aim,
            AmmoType = (ushort)pending.AmmoType,
            Range = pending.Range,
            Damage = (int)Math.Round(pending.Damage),
            BurstCount = burstCount,
            Spread = pending.Spread,
            SpreadSeed = 0,
            Unk5 = 0,
            Hardpoint = pending.Hardpoint,
            HaveHomingTarget = (byte)(pending.UseHomingTarget ? 1 : 0),
            HomingTarget = default,
        };

        _shard.EntityMan.SendToScopedReliable(entity, message);
    }

    // Keeps one spread state per fire mode
    private WeaponPlayerSim GetOrCreatePlayerSim(ulong entityId)
    {
        if (!_weaponSimState.TryGetValue(entityId, out var player))
        {
            player = new WeaponPlayerSim();
            _weaponSimState[entityId] = player;
        }

        return player;
    }

    private WeaponModeState GetOrCreateState(CharacterEntity entity, CharacterEntity.ActiveWeaponDetails activeWeaponDetails, uint time)
    {
        var player = GetOrCreatePlayerSim(entity.EntityId);

        uint weaponId = activeWeaponDetails.WeaponId;
        byte activeMode = entity.GetActiveFireModeIndex();

        bool weaponChanged = player.WeaponId != weaponId;
        if (weaponChanged)
        {
            player.WeaponId = weaponId;
            player.Modes[0] = new WeaponModeState { WeaponId = weaponId };
            player.Modes[1] = new WeaponModeState { WeaponId = weaponId };
            player.AmmoInitialized = false;
            player.Charge = 0;
            player.LastChargeTime = time;
            CaptureChargeupCaps(player, entity, activeWeaponDetails);
        }

        bool modeChanged = player.ActiveMode != activeMode;
        if (weaponChanged || modeChanged)
        {
            byte otherMode = (byte)(1 - activeMode);
            var otherDetails = entity.GetWeaponDetails(otherMode);
            bool otherHasWeapon = otherDetails != null && otherDetails.Weapon != null;

            bool newHasScope = activeWeaponDetails.Weapon.ScopeId != 0;
            bool newLinked = (activeWeaponDetails.Weapon.WeaponFlags & 8) != 0;
            bool otherLinked = otherHasWeapon && (otherDetails.Weapon!.WeaponFlags & 8) != 0;

            WeaponSpreadMath.SeedModeStateOnSwitch(
                activeWeaponDetails.WeaponProfile,
                player.Modes[activeMode],
                otherHasWeapon ? otherDetails.WeaponProfile : default,
                otherHasWeapon ? player.Modes[otherMode] : null,
                newHasScope,
                newLinked,
                otherLinked,
                time);

            // Firing/reload state does not carry over a weapon or fire-mode switch
            player.Fire.Reset();

            // Clip caps for the new mode. Linked modes share one clip (the client syncs the
            // clip between linked modes), so a switch never clamps the shared clip down —
            // the caps stay the main mode's. Non-linked modes are different weapon
            // templates; their clip does not carry over, so clamp to the new caps.
            if (modeChanged && !weaponChanged)
            {
                if (newLinked || otherLinked)
                {
                    var mainDetails = activeMode == 0 ? activeWeaponDetails : entity.GetWeaponDetails(0);
                    if (mainDetails != null && mainDetails.Weapon != null)
                    {
                        player.ClipMax = mainDetails.WeaponProfile.BaseClipSize;
                        player.ReserveMax = mainDetails.WeaponProfile.MaxAmmo;
                    }
                }
                else
                {
                    player.ClipMax = activeWeaponDetails.WeaponProfile.BaseClipSize;
                    player.ReserveMax = activeWeaponDetails.WeaponProfile.MaxAmmo;
                    if (player.ClipAmmo > player.ClipMax)
                    {
                        player.ClipAmmo = player.ClipMax;
                    }

                    if (player.ReserveAmmo > player.ReserveMax)
                    {
                        player.ReserveAmmo = player.ReserveMax;
                    }
                }

                // Chargeup: the charge is weapon-wide. Linked modes keep it across the
                // switch (with the switch loss); non-linked modes are a different weapon
                // template, so their charge does not carry over.
                if (player.Charge > 0)
                {
                    player.Charge = (newLinked || otherLinked)
                        ? (uint)(player.Charge * _chargeSwitchKeepFraction)
                        : 0;
                }
            }

            player.ActiveMode = activeMode;
        }

        return player.Modes[activeMode];
    }

    private void ProcessEntity(CharacterEntity entity)
    {
        ProcessWeaponSpread(entity);
        DebugWeaponSpread(entity);
        ProcessFire(entity);
        ProcessPendingAbilityProjectiles(entity);
    }

    private void ProcessWeaponSpread(CharacterEntity entity)
    {
        var activeWeaponDetails = entity.GetActiveWeaponDetails();
        if (activeWeaponDetails == null || activeWeaponDetails.Weapon == null)
        {
            return;
        }

        uint currentTime = _shard.CurrentTime;
        var weaponSimState = GetOrCreateState(entity, activeWeaponDetails, currentTime);

        WeaponSpreadMath.UpdateMovementState(activeWeaponDetails.WeaponProfile, weaponSimState, entity.MovementStateContainer.MovementStateValue, currentTime);
        WeaponSpreadMath.RecoilInterpUpdate(activeWeaponDetails.WeaponProfile, weaponSimState, currentTime, snapshot: false);
    }

    /// <summary>
    ///     Drives the per-weapon fire state machine: drains queued fire events in client-time
    ///     order, fires scheduled shots, completes reloads and regens the clip.
    /// </summary>
    /// <param name="entity">Character with a weapon sim state</param>
    private void ProcessFire(CharacterEntity entity)
    {
        var activeWeaponDetails = entity.GetActiveWeaponDetails();
        if (activeWeaponDetails == null || activeWeaponDetails.Weapon == null)
        {
            return;
        }

        var weapon = activeWeaponDetails.Weapon;
        var weaponProfile = activeWeaponDetails.WeaponProfile;
        uint time = _shard.CurrentTime;
        var modeState = GetOrCreateState(entity, activeWeaponDetails, time);
        var sim = _weaponSimState[entity.EntityId];
        var fire = sim.Fire;

        EnsureAmmoInitialized(sim, weapon, weaponProfile);
        UpdateCharge(sim, weapon, time);

        // Reload completion (before the shot/event loop so a resumed press can fire this tick)
        if (fire.State == WeaponFireStateEnum.Reloading && time >= fire.ReloadCompleteTime)
        {
            CompleteReload(sim);
            fire.State = WeaponFireStateEnum.Idle;
            SyncAmmoView(entity, sim);

            if (fire.TriggerHeld && IsContinuousFireMode(weapon))
            {
                uint readyTime = Math.Max(fire.NextShotTime, fire.TriggerHeldTime);
                readyTime = Math.Max(readyTime, fire.ReloadCompleteTime);
                if (IsM1Chargeup(weapon) && sim.Charge < sim.ChargeMin)
                {
                    fire.NextShotTime = readyTime + (sim.ChargeMin - sim.Charge);
                    _logger.Debug(
                        "[FireSim] Reload ended uncharged (server={ServerTime} clientFireBurst={FireTime} charge={Charge}/{Cap} min={Min} ready={ReadyTime})",
                        _shard.CurrentTime,
                        fire.TriggerHeldTime,
                        sim.Charge,
                        sim.ChargeCap,
                        sim.ChargeMin,
                        fire.NextShotTime);
                }
                else
                {
                    _logger.Debug("[FireSim] Resuming fire after reload (server={ServerTime} clientFireBurst={FireTime} ready={ReadyTime})", _shard.CurrentTime, fire.TriggerHeldTime, readyTime);
                    StartFiring(entity, sim, weapon, readyTime);
                }
            }
        }

        if (fire.State == WeaponFireStateEnum.Idle
            && fire.TriggerHeld
            && IsContinuousFireMode(weapon)
            && fire.TriggerHeldTime < fire.NextShotTime
            && (!sim.HasAmmo || sim.ClipAmmo > 0)
            && fire.NextShotTime <= time)
        {
            _logger.Debug("[FireSim] Resuming fire at ready time (server={ServerTime} ready={ReadyTime} press={PressTime})", _shard.CurrentTime, fire.NextShotTime, fire.TriggerHeldTime);
            StartFiring(entity, sim, weapon, Math.Max(fire.NextShotTime, fire.TriggerHeldTime));
        }

        while (true)
        {
            bool shotDue = fire.State == WeaponFireStateEnum.Firing && fire.NextShotTime <= time;
            bool evtDue = sim.Events.Count > 0 && sim.Events[0].Time <= time;
            if (!shotDue && !evtDue)
            {
                break;
            }

            var evt = evtDue ? sim.Events[0] : default;
            bool eventFirst = evtDue && (!shotDue
                || evt.Time < fire.NextShotTime
                || (evt.Time == fire.NextShotTime && evt.Type == WeaponFireEventType.FireAimSample));

            if (eventFirst)
            {
                sim.Events.RemoveAt(0);
                HandleFireEvent(entity, sim, activeWeaponDetails, evt);
            }
            else if (!TryFireScheduledShot(entity, sim, activeWeaponDetails, modeState, fire))
            {
                OnClipEmpty(entity, sim, weapon, fire.NextShotTime);
                break;
            }
        }

        // Passive clip regen from reserve
        if (sim.HasAmmo && fire.State == WeaponFireStateEnum.Idle && weapon.ClipRegenMs > 0 &&
            sim.ClipAmmo < sim.ClipMax && sim.ReserveAmmo > 0 &&
            time >= sim.LastClipRegenTime + weapon.ClipRegenMs)
        {
            sim.ClipAmmo++;
            sim.ReserveAmmo--;
            sim.LastClipRegenTime = time;
            SyncAmmoView(entity, sim);
        }
    }

    private void UpdateCharge(WeaponPlayerSim sim, WeaponTemplateResult weapon, uint time)
    {
        var fire = sim.Fire;
        uint dt = time - sim.LastChargeTime;
        sim.LastChargeTime = time;
        if (sim.ChargeCap == 0 || fire.State == WeaponFireStateEnum.Reloading)
        {
            return;
        }

        uint was = sim.Charge;
        if (fire.State == WeaponFireStateEnum.Firing)
        {
            sim.Charge = sim.ChargeCap;
        }
        else if (fire.TriggerHeld)
        {
            sim.Charge = was + dt > sim.ChargeCap ? sim.ChargeCap : was + dt;
        }
        else
        {
            sim.Charge = was >= dt ? was - dt : 0;
        }

        if (sim.ChargeMin > 0 && fire.TriggerHeld && was < sim.ChargeMin && sim.Charge >= sim.ChargeMin)
        {
            _logger.Debug("[FireSim] Charge armed weapon={Weapon} charge={Charge}/{Cap} min={Min} server={ServerTime}", weapon.DebugName, sim.Charge, sim.ChargeCap, sim.ChargeMin, _shard.CurrentTime);
        }
    }

    private void HandleFireEvent(CharacterEntity entity, WeaponPlayerSim sim, CharacterEntity.ActiveWeaponDetails details, WeaponFireEvent evt)
    {
        var weapon = details.Weapon;
        var fire = sim.Fire;

        switch (evt.Type)
        {
            case WeaponFireEventType.FireAimSample:
                fire.AimSamples.Add(new FireAimSample { Time = evt.Time, Aim = evt.Aim, Velocity = evt.Velocity });
                TrimAimSamples(fire, evt.Time);
                break;

            case WeaponFireEventType.FireBurst:
                // Remember the press and its client time; a reload in progress resumes from it.
                fire.TriggerHeld = true;
                fire.TriggerHeldTime = evt.Time;

                if (fire.State == WeaponFireStateEnum.Reloading)
                {
                    _logger.Debug("[FireSim] FireBurst held during reload (server={ServerTime} client={ClientTime}); will resume if continuous", _shard.CurrentTime, evt.Time);
                    break; // resume at reload completion
                }

                if (IsChargeOnlyMode(weapon, details.WeaponProfile))
                {
                    _logger.Debug("[FireSim] FireBurst in spin-up mode (server={ServerTime} client={ClientTime} charge={Charge}/{Cap})", _shard.CurrentTime, evt.Time, sim.Charge, sim.ChargeCap);
                    break;
                }

                if (sim.HasAmmo && sim.ClipAmmo <= 0)
                {
                    if (!sim.ClipEmptyTriggered)
                    {
                        OnClipEmpty(entity, sim, weapon, evt.Time);
                    }

                    break;
                }

                if (IsM1Chargeup(weapon) && sim.Charge < sim.ChargeMin)
                {
                    fire.NextShotTime = evt.Time + (sim.ChargeMin - sim.Charge);
                    _logger.Debug("[FireSim] FireBurst while charging (server={ServerTime} client={ClientTime} charge={Charge}/{Cap} min={Min} ready={ReadyTime})", _shard.CurrentTime, evt.Time, sim.Charge, sim.ChargeCap, sim.ChargeMin, fire.NextShotTime);
                    break;
                }

                if (fire.NextShotTime > evt.Time)
                {
                    _logger.Debug("[FireSim] FireBurst during down period (server={ServerTime} client={ClientTime} ready={ReadyTime})", _shard.CurrentTime, evt.Time, fire.NextShotTime);
                    break;
                }

                StartFiring(entity, sim, weapon, evt.Time);
                break;

            case WeaponFireEventType.FireEnd:
            case WeaponFireEventType.FireCancel:
                fire.TriggerHeld = false;
                fire.ShotsRemainingInBurst = 0;
                if (fire.State == WeaponFireStateEnum.Firing)
                {
                    fire.State = WeaponFireStateEnum.Idle; // leave Reloading untouched
                }

                break;

            case WeaponFireEventType.FireInputIgnored:
                if (evt.Value != 0)
                {
                    fire.TriggerHeld = false;
                    fire.ShotsRemainingInBurst = 0;
                    if (fire.State == WeaponFireStateEnum.Firing)
                    {
                        fire.State = WeaponFireStateEnum.Idle; // leave Reloading untouched
                    }
                }

                _logger.Debug("[FireSim] FireInputIgnored (server={ServerTime} client={ClientTime} ignored={Ignored})", _shard.CurrentTime, evt.Time, evt.Value);
                break;

            case WeaponFireEventType.UseScope:
                break;

            case WeaponFireEventType.SelectWeapon:
            case WeaponFireEventType.SelectFireMode:
                fire.Reset();
                break;

            case WeaponFireEventType.ReloadWeapon:
                if (fire.State != WeaponFireStateEnum.Reloading)
                {
                    StartReload(entity, sim, weapon, evt.Time);
                }

                break;

            case WeaponFireEventType.CancelReload:
                if (fire.State == WeaponFireStateEnum.Reloading)
                {
                    fire.State = WeaponFireStateEnum.Idle;
                }

                break;
        }
    }

    /// <summary>
    ///     Fires the next scheduled shot at its client time and advances the press schedule.
    ///     Returns false when there is not enough ammo for a full shot.
    /// </summary>
    /// <param name="entity">The character firing.</param>
    /// <param name="sim">The character's weapon simulation.</param>
    /// <param name="details">The active weapon's details.</param>
    /// <param name="modeState">The active fire-mode state (press schedule).</param>
    /// <param name="fire">The fire state (ammo and shot bookkeeping).</param>
    /// <returns>True when the shot was fired; false when there is not enough ammo for a full shot.</returns>
    private bool TryFireScheduledShot(CharacterEntity entity, WeaponPlayerSim sim, CharacterEntity.ActiveWeaponDetails details, WeaponModeState modeState, WeaponFireState fire)
    {
        if (!FireShot(entity, sim, details, modeState, fire.NextShotTime))
        {
            return false;
        }

        fire.ShotIndexInPress++;

        // Advance the schedule before any exit so NextShotTime always ends up holding the
        // weapon's ready time (shot time + spacing) — even after the last shot of a press or
        // the shot that emptied the clip. A press made during that down period must not
        // masquerade as an immediate shot. Clamp to >= 1 so a zero MsPerBurst cannot stall
        // the loop (client does the same).
        uint spacing = Math.Max(1u, details.WeaponProfile.MsPerBurst);
        fire.NextShotTime += spacing;
        if (fire.ShotsRemainingInBurst > 1)
        {
            fire.ShotsRemainingInBurst--;
        }
        else if (fire.ShotsRemainingInBurst >= 0)
        {
            fire.State = WeaponFireStateEnum.Idle; // press complete (semi/burst)
        }

        if (sim.HasAmmo && sim.ClipAmmo <= 0)
        {
            return false; // caller runs OnClipEmpty
        }

        return true;
    }

    private void StartFiring(CharacterEntity entity, WeaponPlayerSim sim, WeaponTemplateResult weapon, uint time)
    {
        var fire = sim.Fire;
        fire.State = WeaponFireStateEnum.Firing;
        fire.FireStartTime = time;
        fire.NextShotTime = time;
        fire.ShotIndexInPress = 0;

        byte burstMode = (byte)(weapon.WeaponFlags & 7u);
        bool oneShotPerPress = ((WeaponFlags)weapon.WeaponFlags).BoltAction || burstMode == 2 || burstMode == 4;
        int roundsInPress;
        if (oneShotPerPress)
        {
            fire.ShotsRemainingInBurst = 1;
            fire.TotalRoundsInPress = 1;
            roundsInPress = 1;
        }
        else if (weapon.RoundsPerBurst > 1)
        {
            int rounds = Math.Min(weapon.RoundsPerBurst, sim.HasAmmo ? sim.ClipAmmo : int.MaxValue);
            rounds = Math.Max(rounds, 1);
            fire.ShotsRemainingInBurst = rounds;
            fire.TotalRoundsInPress = (uint)rounds;
            roundsInPress = rounds;
        }
        else
        {
            fire.ShotsRemainingInBurst = -1; // full auto: unlimited while held
            fire.TotalRoundsInPress = 0;
            roundsInPress = sim.HasAmmo ? Math.Max(1, sim.ClipAmmo) : 1;
        }

        if (weapon.BurstAbility != 0 && roundsInPress >= Math.Max(1, (int)weapon.MinRoundsPerBurst))
        {
            ActivateWeaponAbility(entity, weapon.BurstAbility, time);
        }
    }

    private void StartReload(CharacterEntity entity, WeaponPlayerSim sim, WeaponTemplateResult weapon, uint time)
    {
        if (!sim.HasAmmo || sim.ReserveAmmo <= 0 || sim.ClipAmmo >= sim.ClipMax)
        {
            return;
        }

        sim.Fire.State = WeaponFireStateEnum.Reloading;
        sim.Fire.ReloadCompleteTime = time + weapon.ReloadTime;
        sim.Fire.ShotsRemainingInBurst = 0;
        sim.Fire.TriggerHeld = false;
        sim.Charge = 0;

        ActivateWeaponAbility(entity, weapon.ReloadAbility, time);
    }

    private void CompleteReload(WeaponPlayerSim sim)
    {
        int rounds = Math.Min(sim.ClipMax - sim.ClipAmmo, sim.ReserveAmmo);
        sim.ClipAmmo += rounds;
        sim.ReserveAmmo -= rounds;
        if (sim.ClipAmmo > 0)
        {
            sim.ClipEmptyTriggered = false;
        }
    }

    private void OnClipEmpty(CharacterEntity entity, WeaponPlayerSim sim, WeaponTemplateResult weapon, uint time)
    {
        sim.ClipEmptyTriggered = true;
        sim.Fire.State = WeaponFireStateEnum.Idle;
        sim.Fire.ShotsRemainingInBurst = 0;

        sim.Fire.TriggerHeld = false;

        _logger.Debug("[FireSim] Clip empty (server={ServerTime} client={ClientTime} reserve={Reserve} weapon={Weapon})", _shard.CurrentTime, time, sim.ReserveAmmo, weapon.DebugName);

        ActivateWeaponAbility(entity, weapon.EmptyAbility, time);

        if (!entity.IsPlayerControlled)
        {
            StartReload(entity, sim, weapon, time);
        }
    }

    private void ProcessPendingAbilityProjectiles(CharacterEntity entity)
    {
        if (entity.IsPlayerControlled)
        {
            return;
        }

        while (entity.TryConsumeNextAbilityProjectile() is { } pending)
        {
            uint time = pending.QueueTime != 0 ? pending.QueueTime : pending.ActivationTime;
            OnFireAbilityProjectile(entity, pending, time, entity.AimDirection, null);
        }
    }

    private void ActivateWeaponAbility(CharacterEntity entity, uint abilityId, uint time)
    {
        if (abilityId == 0 || entity is not IAptitudeTarget initiator)
        {
            return;
        }

        _shard.Abilities.HandleActivateAbility(_shard, initiator, abilityId, time, new AptitudeTargets());
    }

    /// <summary>
    ///     For server-driven NPCs firing gravity-affected ammo, replaces the flat pose aim with the low-arc ballistic
    ///     solution to the NPC's current combat target. Players keep their own aim because their client predicts the shot.
    /// </summary>
    /// <param name="entity">The NPC firing the shot.</param>
    /// <param name="origin">The muzzle world position.</param>
    /// <param name="aim">The flat pose aim direction (fallback).</param>
    /// <param name="ammo">The ammo template (whether it is gravity-affected).</param>
    /// <param name="projectileSpeed">The projectile muzzle speed.</param>
    /// <returns>The low-arc ballistic aim toward the combat target, or the original aim when no solution applies.</returns>
    private Vector3 ResolveBallisticAim(CharacterEntity entity, Vector3 origin, Vector3 aim, Ammo ammo, float projectileSpeed)
    {
        if (entity.IsPlayerControlled)
        {
            return aim;
        }

        AmmoFlags flags = ammo.Flags;
        if (flags.Simulation != AmmoFlags.SimulationMode.Parabolic || ammo.Gravity <= 0f || projectileSpeed <= 0f)
        {
            return aim;
        }

        if (!TryGetNpcBallisticTarget(entity, out var target))
        {
            return aim;
        }

        return TrySolveBallisticAim(origin, target, projectileSpeed, ammo.Gravity, out var solved) ? solved : aim;
    }

    private bool TryGetNpcBallisticTarget(CharacterEntity entity, out Vector3 target)
    {
        target = Vector3.Zero;
        var targetId = entity.NpcMovement?.CombatTargetId ?? 0;
        if (targetId == 0 || !_shard.Entities.TryGetValue(targetId, out var entity2) || entity2 is not CharacterEntity { IsAlive: true } targetChar)
        {
            return false;
        }

        // Match NpcMovementSystem's combat aim height: muzzle-to-chest, with the target's chest as the impact point.
        target = new Vector3(targetChar.Position.X, targetChar.Position.Y, targetChar.Position.Z + 1.0f);
        return true;
    }

    /// <summary>
    ///     Fires one simulated shot at the given client time: consumes ammo, resolves aim/origin activates the per-shot attack ability
    /// </summary>
    /// <param name="entity">Character that fires</param>
    /// <param name="sim">Per-player weapon sim state</param>
    /// <param name="details">Active weapon details</param>
    /// <param name="modeState">Spread state of the active fire mode</param>
    /// <param name="time">Client time of the shot</param>
    /// <returns>True when the shot was fired, false when there is not enough ammo for a full shot</returns>
    private bool FireShot(CharacterEntity entity, WeaponPlayerSim sim, CharacterEntity.ActiveWeaponDetails details, WeaponModeState modeState, uint time)
    {
        var weapon = details.Weapon;

        if (sim.HasAmmo)
        {
            int perShot = Math.Max(1, (int)weapon.AmmoPerBurst);
            if (sim.ClipAmmo < perShot)
            {
                return false;
            }

            sim.ClipAmmo -= perShot;
        }

        // Aim resolution: 1) the client sample for this exact shot time, 2) the aim interpolated
        // from the movement sample buffer at the shot time, 3) the last known pose.
        Vector3 aim;
        Vector3? velocity = null;
        string aimSource;
        if (TryGetAimSample(sim.Fire, time, out var sample))
        {
            aim = sample.Aim;
            velocity = sample.Velocity;
            aimSource = "sample";
        }
        else
        {
            aim = entity.AimDirection;
            aimSource = "pose";
        }

        Vector3 origin = entity.GetProjectileOrigin(time, aim, velocity);

        WeaponSpreadMath.UpdateMovementState(details.WeaponProfile, modeState, entity.MovementStateContainer.MovementStateValue, time);
        WeaponSpreadMath.RecoilInterpUpdate(details.WeaponProfile, modeState, time, snapshot: true);
        float spreadPct = WeaponSpreadMath.GetCurrentSpreadPct(details.WeaponProfile, modeState, time);

        bool firedViaAbility = false;
        if (weapon.AttackAbility != 0 && entity is IAptitudeTarget initiator)
        {
            Guid executionId = Guid.NewGuid();
            _shard.Abilities.HandleActivateAbility(_shard, initiator, weapon.AttackAbility, time, new AptitudeTargets(), executionId: executionId, initPosition: origin);

            var pending = entity.TryConsumeAbilityProjectileByExecId(executionId);
            if (pending.HasValue)
            {
                OnFireAbilityProjectile(entity, pending.Value, time, aim, velocity);
                firedViaAbility = true;
            }
        }

        // Monster attack weapons
        if (!firedViaAbility && !entity.IsPlayerControlled && weapon.AttackAbility == 0 && weapon.BurstAbility != 0 && weapon.RoundsPerBurst <= 1)
        {
            firedViaAbility = true;
        }

        if (!firedViaAbility)
        {
            FireWeaponDirect(entity, sim, details, modeState, time, aim, velocity, origin, spreadPct, aimSource);
        }

        SyncAmmoView(entity, sim);
        return true;
    }

    private void EnsureAmmoInitialized(WeaponPlayerSim sim, WeaponTemplateResult weapon, WeaponProfile weaponProfile)
    {
        if (sim.AmmoInitialized)
        {
            return;
        }

        sim.AmmoInitialized = true;
        sim.HasAmmo = weaponProfile.BaseClipSize > 0 && weaponProfile.MaxAmmo > 0;
        sim.ClipMax = weaponProfile.BaseClipSize;
        sim.ReserveMax = weaponProfile.MaxAmmo;
        sim.ClipAmmo = weaponProfile.BaseClipSize;
        sim.ReserveAmmo = weaponProfile.MaxAmmo;
        sim.ClipEmptyTriggered = false;
        sim.LastClipRegenTime = _shard.CurrentTime;

        _logger.Debug("[FireSim] Ammo initialized weapon={Weapon} clip={Clip}/{ClipMax} reserve={Reserve}/{ReserveMax} msPerBurst={MsPerBurst} clipRegenMs={ClipRegenMs}", weapon.DebugName, sim.ClipAmmo, sim.ClipMax, sim.ReserveAmmo, sim.ReserveMax, weaponProfile.MsPerBurst, weapon.ClipRegenMs);
    }

    private void SyncAmmoView(CharacterEntity entity, WeaponPlayerSim sim)
    {
        var view = entity.Character_CombatView;
        if (view == null || !sim.HasAmmo)
        {
            return;
        }

        int slot = entity.WeaponIndex.Index >= 2 ? 1 : 0;
        ushort clip = (ushort)Math.Max(0, sim.ClipAmmo);
        ushort reserve = (ushort)Math.Max(0, sim.ReserveAmmo);
        if (slot == 0)
        {
            view.Ammo_0Prop = clip;
            view.AltAmmo_0Prop = reserve;
        }
        else
        {
            view.Ammo_1Prop = clip;
            view.AltAmmo_1Prop = reserve;
        }
    }

    private void DebugWeaponSpread(CharacterEntity entity)
    {
        var client = entity.Player;

        // NPCs have no player/client to send debug spread to.
        if (client == null)
        {
            return;
        }

        if (client.Preferences.DebugWeapon == 0)
        {
            return;
        }

        if (!client.CanReceiveGSS)
        {
            return;
        }

        var activeWeaponDetails = entity.GetActiveWeaponDetails();
        if (activeWeaponDetails == null || activeWeaponDetails.Weapon == null)
        {
            return;
        }

        var weapon = activeWeaponDetails.Weapon;
        var weaponId = activeWeaponDetails.WeaponId;
        uint time = _shard.CurrentTime;
        var weaponSimState = GetOrCreateState(entity, activeWeaponDetails, time);

        float spreadPct = WeaponSpreadMath.GetCurrentSpreadPct(activeWeaponDetails.WeaponProfile, weaponSimState, time);

        var ammo = SDBInterface.GetAmmo(weapon.AmmoId);
        var eventData = new DebugWeaponSimEventData()
        {
            WeaponName = weapon.DebugName,
            SpreadPct = spreadPct,
            WeaponId = weaponId,
            AccumulatedSpread = weaponSimState.AccumulatedSpread,
            SpreadHeat = weaponSimState.SpreadHeat,
            MovementStateValue = entity.MovementStateContainer.MovementStateValue,
            SpreadMovementState = weaponSimState.SpreadMovementState,
            AgilityFactor = weaponSimState.AgilityCurrent,
            BaseSpreadPct = activeWeaponDetails.WeaponProfile.BaseSpreadPct,
            OtherSpreadPct = activeWeaponDetails.WeaponProfile.OtherSpreadPct,
            Heat = weaponSimState.SpreadHeat,
            SpreadHeatAfterExponent = WeaponSpreadMath.ApplyRampExponent(weaponSimState.SpreadHeat, activeWeaponDetails.WeaponProfile.SpreadRampExponent),
            SpreadFactor = (WeaponSpreadMath.ApplyRampExponent(weaponSimState.SpreadHeat, activeWeaponDetails.WeaponProfile.SpreadRampExponent) * (1f - activeWeaponDetails.WeaponProfile.MinSpreadFrac)) + activeWeaponDetails.WeaponProfile.MinSpreadFrac,
            LastRecoilUpdate = weaponSimState.LastRecoilUpdate,
            AccumulatedWhenReturnStarted = weaponSimState.AccumulatedSpreadWhenReturnStarted,
            StartingSpread = activeWeaponDetails.WeaponProfile.StartingSpread,
            MinSpreadFrac = activeWeaponDetails.WeaponProfile.MinSpreadFrac,
            MsPerBurst = activeWeaponDetails.WeaponProfile.MsPerBurst,
            SpreadRampTime = activeWeaponDetails.WeaponProfile.SpreadRampTime,
            MsSpreadReturn = activeWeaponDetails.WeaponProfile.MsSpreadReturn,
            MsSpreadReturnDelay = activeWeaponDetails.WeaponProfile.MsSpreadReturnDelay,
            WeaponFlags = ((WeaponFlags)weapon.WeaponFlags).ToString(),
            AmmoFlags = ammo != null ? new AmmoFlags(ammo.Flags).ToString() : "[N/A]",
            AmmoName = ammo?.Name ?? "[N/A]",
            AmmoId = ammo?.Id ?? 0
        };

        try
        {
            var json = JsonSerializer.Serialize(eventData);

            var message = new TempConsoleMessage()
            {
                ConsoleNoticeMessage = string.Empty,
                ConsoleCommand = string.Empty,
                ChatNotification = string.Empty,
                DebugReportArgType = "WeaponSim.Spread",
                DebugReportArgData = json,
            };

            client.NetChannels[ChannelType.ReliableGss].SendMessage(message);
        }
        catch (Exception e)
        {
            _logger.Error(e, "Failed DebugWeaponSpread");
        }
    }

    private IEnumerable<IEntity> GetWeaponSimPlayersEntities()
    {
        return _shard.Entities.Values.Where((entity) =>
            entity is CharacterEntity character &&
            (character.IsPlayerControlled ||
                (character.IsAlive &&
                    (_weaponSimState.ContainsKey(character.EntityId) || character.HasPendingAbilityProjectiles))));
    }

    // Per-player weapon sim state
    private sealed class WeaponPlayerSim
    {
        public uint WeaponId;
        public byte ActiveMode = 255;
        public WeaponModeState[] Modes = new WeaponModeState[2];
        public List<WeaponFireEvent> Events = [];
        public WeaponFireState Fire = new();
        public bool AmmoInitialized;
        public bool HasAmmo;
        public bool ClipEmptyTriggered;
        public int ClipAmmo;
        public int ReserveAmmo;
        public int ClipMax;
        public int ReserveMax;
        public uint LastClipRegenTime;

        // Chargeup: weapon-wide (shared across the fire modes of this weapon), in ms
        public uint Charge;
        public uint LastChargeTime;
        public uint ChargeCap;
        public uint ChargeMin;
    }

    private record class DebugWeaponSimEventData
    {
        public string WeaponName { get; set; }
        public float SpreadPct { get; set; }
        public uint WeaponId { get; set; }
        public float AccumulatedSpread { get; set; }
        public float SpreadHeat { get; set; }
        public ushort MovementStateValue { get; set; }
        public byte SpreadMovementState { get; set; }
        public float AgilityFactor { get; set; }
        public float BaseSpreadPct { get; set; }
        public float OtherSpreadPct { get; set; }
        public float Heat { get; set; }
        public float SpreadHeatAfterExponent { get; set; }
        public float SpreadFactor { get; set; }
        public uint LastRecoilUpdate { get; set; }
        public float AccumulatedWhenReturnStarted { get; set; }
        public float StartingSpread { get; set; }
        public float MinSpreadFrac { get; set; }
        public uint MsPerBurst { get; set; }
        public uint SpreadRampTime { get; set; }
        public uint MsSpreadReturn { get; set; }
        public uint MsSpreadReturnDelay { get; set; }
        public string WeaponFlags { get; set; }
        public string AmmoFlags { get; set; }
        public string AmmoName { get; set; }
        public uint AmmoId { get; set; }
    }
}