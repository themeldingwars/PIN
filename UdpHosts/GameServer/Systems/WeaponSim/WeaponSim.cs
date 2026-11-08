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

public partial class WeaponSim
{
    private readonly Dictionary<ulong, CharacterWeapons> _weaponSimState;
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

    internal ILogger Logger => _logger;

    internal uint CurrentTime => _shard.CurrentTime;

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
        InsertEventSorted(GetOrCreateWeapons(entity).Events, evt);
    }

    public void OnFireWeaponProjectile(CharacterEntity entity, uint time, Vector3 localAimDir, Vector3? shooterVelocity = null)
    {
        EnqueueFireEvent(entity, new WeaponFireEvent
        {
            Time = time,
            Type = WeaponFireEventType.Shot,
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
        var activeWeaponDetails = SimWeapon(entity);
        var attrsDict = activeWeaponDetails?.Attributes;

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

    public void RestockAmmo(CharacterEntity entity, uint percent) => GetOrCreateWeapons(entity).Ammo.Restock(entity, percent);

    /// <summary>
    ///     Takes the weapon slot, 1 or 2. False for a character without weapon state.
    /// </summary>
    public bool NeedsAmmo(CharacterEntity entity, byte weaponSlot) => _weaponSimState.TryGetValue(entity.EntityId, out var weapons) && weapons.Ammo.NeedsAmmo(weaponSlot);

    public void ResetAmmoState(CharacterEntity entity)
    {
        _weaponSimState.Remove(entity.EntityId);
        GetOrCreateWeapons(entity).Ammo.Reset(entity);
    }

    /// <summary>
    ///     The caller has already taken the ammo for the shot.
    /// </summary>
    /// <param name="ctx">The firing weapon</param>
    /// <param name="time">Client time of the shot, it also seeds the spread</param>
    /// <param name="aim">Aim direction of the shot</param>
    /// <param name="velocity">Shooter velocity, when the client sent one</param>
    /// <param name="aimSource">Where the aim came from, for the log</param>
    /// <param name="roundsInPress">Rounds of the current press for the spread heat, 0 for full auto</param>
    internal void FireShot(in WeaponContext ctx, uint time, Vector3 aim, Vector3? velocity, string aimSource, uint roundsInPress)
    {
        var entity = ctx.Entity;
        var details = ctx.Details;
        var modeState = ctx.Mode;
        var weapon = details.Weapon;
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

        // Monster weapons attack through their burst ability, which StartPress already ran
        if (!firedViaAbility && !entity.IsPlayerControlled && weapon.AttackAbility == 0 && weapon.BurstAbility != 0 && weapon.RoundsPerBurst <= 1)
        {
            firedViaAbility = true;
        }

        if (!firedViaAbility)
        {
            FireWeaponDirect(entity, details, modeState, time, aim, velocity, origin, spreadPct, aimSource, roundsInPress);
        }
    }

    internal void ActivateWeaponAbility(CharacterEntity entity, uint abilityId, uint time)
    {
        if (abilityId == 0 || entity is not IAptitudeTarget initiator)
        {
            return;
        }

        _shard.Abilities.HandleActivateAbility(_shard, initiator, abilityId, time, new AptitudeTargets());
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

    private static void InsertEventSorted(List<WeaponFireEvent> events, WeaponFireEvent evt)
    {
        int lo = 0;
        int hi = events.Count - 1;
        while (lo <= hi)
        {
            int mid = (lo + hi) / 2;

            // Equal times keep their arrival order, a press comes before the shot fired at the same time
            if (events[mid].Time <= evt.Time)
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

    /// <summary>
    ///     Spawns a single weapon projectile directly from the weapon/ammo templates.
    /// </summary>
    /// <param name="entity">Character that fires</param>
    /// <param name="details">Active weapon details (mode template + attributes)</param>
    /// <param name="modeState">Spread state of the active fire mode</param>
    /// <param name="time">Client time of the shot</param>
    /// <param name="aim">Aim direction at shot time</param>
    /// <param name="velocity">Shooter velocity at shot time</param>
    /// <param name="origin">Projectile origin at shot time</param>
    /// <param name="spreadPct">Spread percent at shot time</param>
    /// <param name="aimSource">Only for the log, client or pose</param>
    /// <param name="roundsInPress">Rounds of the current press for the spread heat, 0 for full auto</param>
    private void FireWeaponDirect(CharacterEntity entity, CharacterEntity.ActiveWeaponDetails details, WeaponModeState modeState, uint time, Vector3 aim, Vector3? velocity, Vector3 origin, float spreadPct, string aimSource, uint roundsInPress)
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
        WeaponSpreadMath.ApplyBurstSpreadUpdate(spreadProfile, modeState, 1, Math.Max(1u, roundsInPress));

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

    private WeaponFire NewFire(CharacterEntity entity) => entity.IsPlayerControlled ? new PlayerFire(this) : new NpcFire(this);

    private CharacterWeapons GetOrCreateWeapons(CharacterEntity entity)
    {
        if (!_weaponSimState.TryGetValue(entity.EntityId, out var weapons))
        {
            weapons = new CharacterWeapons(NewFire(entity));
            weapons.Slot = entity.WeaponIndex.Index;
            weapons.SlotModes[1] = entity.GetFireMode(1);
            weapons.SlotModes[2] = entity.GetFireMode(2);
            _weaponSimState[entity.EntityId] = weapons;
        }
        else if (weapons.Fire is PlayerFire != entity.IsPlayerControlled)
        {
            // The state can exist before a player takes control of the character
            weapons.Fire = NewFire(entity);
        }

        return weapons;
    }

    // The weapon the processed events have reached, the entity's fields already show the newest one
    private CharacterEntity.ActiveWeaponDetails SimWeapon(CharacterEntity entity)
    {
        return _weaponSimState.TryGetValue(entity.EntityId, out var weapons) && weapons.ActiveDetails != null ? weapons.ActiveDetails : entity.GetActiveWeaponDetails();
    }

    /// <summary>
    ///     Switches to the slot and mode the processed events have reached. False without a weapon there.
    /// </summary>
    private bool TrySelect(CharacterEntity entity, CharacterWeapons player, uint time, out WeaponContext ctx)
    {
        byte slot = player.Slot;
        byte activeMode = (byte)(slot < player.SlotModes.Length && entity.HasAltMode(slot) && player.SlotModes[slot] == 1 ? 1 : 0);
        var activeWeaponDetails = entity.GetWeaponDetails(slot, activeMode);
        if (activeWeaponDetails?.Weapon == null)
        {
            player.ActiveDetails = null;
            ctx = default;
            return false;
        }

        // Like the client's weapon objects, a slot keeps its state while holstered until a different weapon goes in
        uint weaponId = activeWeaponDetails.WeaponId;
        bool replaced = player.Modes[slot, 0]?.WeaponId != weaponId;
        if (replaced)
        {
            player.Modes[slot, 0] = new WeaponModeState { WeaponId = weaponId };
            player.Modes[slot, 1] = new WeaponModeState { WeaponId = weaponId };
        }

        bool weaponChanged = replaced || player.ActiveSlot != slot;
        ctx = new WeaponContext(entity, slot, activeWeaponDetails, player.Modes[slot, activeMode], player.Modes[slot, 0], player.Modes[slot, 1], player.Ammo, player.SwapReady);
        bool modeChanged = player.ActiveMode != activeMode;
        if (weaponChanged || modeChanged)
        {
            byte otherMode = (byte)(1 - activeMode);
            var otherDetails = entity.GetWeaponDetails(slot, otherMode);

            // Without an alt the other mode is the main itself, the client carries nothing over then
            bool otherHasWeapon = entity.HasAltMode(slot) && otherDetails != null && otherDetails.Weapon != null;

            bool newHasScope = activeWeaponDetails.HasZoom;
            bool newLinked = activeWeaponDetails.Weapon.Flags.LinkedModes;
            bool otherLinked = otherHasWeapon && otherDetails.Weapon!.Flags.LinkedModes;

            WeaponSpreadMath.SeedModeStateOnSwitch(
                activeWeaponDetails.WeaponProfile,
                player.Modes[slot, activeMode],
                otherHasWeapon ? otherDetails.WeaponProfile : default,
                otherHasWeapon ? player.Modes[slot, otherMode] : null,
                newHasScope,
                newLinked,
                otherLinked,
                time);

            player.Fire.Switch(ctx, weaponChanged, newLinked || otherLinked, time);
            player.ActiveSlot = slot;
            player.ActiveMode = activeMode;
        }

        player.ActiveDetails = activeWeaponDetails;
        player.Ammo.Select(entity, slot, activeWeaponDetails, activeMode, time, _logger);
        return true;
    }

    private void ProcessEntity(CharacterEntity entity)
    {
        ProcessFire(entity);
        ProcessWeaponSpread(entity);
        DebugWeaponSpread(entity);
        ProcessPendingAbilityProjectiles(entity);
    }

    // After ProcessFire, it only follows the weapon the events have switched to
    private void ProcessWeaponSpread(CharacterEntity entity)
    {
        if (!_weaponSimState.TryGetValue(entity.EntityId, out var weapons) || weapons.ActiveDetails is not { Weapon: not null } activeWeaponDetails)
        {
            return;
        }

        uint currentTime = _shard.CurrentTime;
        var weaponSimState = weapons.Modes[weapons.ActiveSlot, weapons.ActiveMode];

        WeaponSpreadMath.UpdateMovementState(activeWeaponDetails.WeaponProfile, weaponSimState, entity.MovementStateContainer.MovementStateValue, currentTime);
        WeaponSpreadMath.RecoilInterpUpdate(activeWeaponDetails.WeaponProfile, weaponSimState, currentTime, snapshot: false);
    }

    /// <summary>
    ///     Works through the due events in the client's time order. A weapon or mode switch takes effect at its event, so
    ///     the shots queued before it still use the old weapon.
    /// </summary>
    private void ProcessFire(CharacterEntity entity)
    {
        uint now = _shard.CurrentTime;
        var weapons = GetOrCreateWeapons(entity);
        var events = weapons.Events;
        bool armed = TrySelect(entity, weapons, now, out var ctx);
        while (events.Count > 0 && events[0].Time <= now)
        {
            var evt = events[0];
            events.RemoveAt(0);
            if (armed)
            {
                weapons.Fire.AdvanceTo(ctx, evt.Time);
            }

            switch (evt.Type)
            {
                case WeaponFireEventType.SelectWeapon:
                    if (evt.Value != weapons.Slot)
                    {
                        if (armed)
                        {
                            WeaponFire.Holster(ctx, evt.Time);
                        }

                        uint exit = entity.GetWeaponDetails(weapons.Slot, 0)?.Weapon?.EquipExitMs ?? 0;
                        uint enter = entity.GetWeaponDetails(evt.Value, 0)?.Weapon?.EquipEnterMs ?? 0;
                        weapons.SwapReady = evt.Time + exit + enter;
                    }

                    weapons.Slot = evt.Value;
                    armed = TrySelect(entity, weapons, evt.Time, out ctx);
                    break;

                case WeaponFireEventType.SelectFireMode:
                    if (evt.Slot < weapons.SlotModes.Length)
                    {
                        weapons.SlotModes[evt.Slot] = evt.Value;
                    }

                    armed = TrySelect(entity, weapons, evt.Time, out ctx);
                    break;

                default:
                    // Without a weapon there's nothing to check the event against
                    if (armed)
                    {
                        weapons.Fire.Handle(ctx, evt);
                    }

                    break;
            }
        }

        if (armed)
        {
            weapons.Fire.AdvanceTo(ctx, now);
        }

        weapons.Ammo.Regen(entity, now);
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

        if (!_weaponSimState.TryGetValue(entity.EntityId, out var weapons) || weapons.ActiveDetails is not { Weapon: not null } activeWeaponDetails)
        {
            return;
        }

        var weapon = activeWeaponDetails.Weapon;
        var weaponId = activeWeaponDetails.WeaponId;
        uint time = _shard.CurrentTime;
        var weaponSimState = weapons.Modes[weapons.ActiveSlot, weapons.ActiveMode];

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

    private sealed class CharacterWeapons(WeaponFire fire)
    {
        public byte ActiveSlot = 255;
        public byte ActiveMode = 255;
        public CharacterEntity.ActiveWeaponDetails ActiveDetails;

        // The weapon slot as far as the processed events got
        public byte Slot;

        // The scope status effect on the character, 0 when unscoped
        public uint ScopeEffect;

        // The end of the last weapon swap
        public uint SwapReady;

        // By weapon slot and mode
        public WeaponModeState[,] Modes = new WeaponModeState[3, 2];

        // By weapon slot, as far as the processed events got
        public byte[] SlotModes = new byte[3];
        public List<WeaponFireEvent> Events = [];
        public WeaponAmmo Ammo = new();
        public WeaponFire Fire = fire;
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