#nullable enable
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using GameServer.Entities.Character;
using GameServer.Physics;
using GameServer.Systems.Ai.Behavior;
using GameServer.Systems.Aptitude;
using GameServer.Systems.Combat;
using GameServer.Systems.WeaponSim;
using Serilog;

namespace GameServer.Systems.Ai;

/// <summary>
///     Outer behavior system: decides each NPC's high-level state (which behavior is active, aggro) and sets movement
///     goals. Runs before <see cref="NpcMovementSystem" /> (steering) and the WeaponSim (which reads the aim the
///     movement system leaves on the NPC).
///     - Aggro: a combat-capable NPC acquires the nearest live player it is hostile to within its perception/aggro
///       radius and enters the Offensive behavior (or Base if it has none), setting a combat standoff goal
///       (combatDist / minDist) on the component.
    ///     - Ranged fire: while in combat, within weapon range, and with line of sight, drives the WeaponSim fire state
    ///       machine with single-shot 1ms trigger presses spaced by the behavior/weapon cadence. Projectile abilities
    ///       (am1/am2 and weapon BurstAbility) queue pending projectiles that WeaponSim fires for NPCs.
///     - Idle: wanderer behaviors drive the Wander mode; idle/stand behaviors hold in place.
/// </summary>
public sealed class NpcBehaviorSystem
{
    // Aggro radius when a behavior declares no perceptionDist/aggroDistance (world units, horizontal).
    private const float _defaultAggroRadius = 30f;

    // Burst/rest fallbacks when the behavior declares no triggerPullTime (burst length) / fireRestDuration (rest).
    private const uint _defaultBurstMs = 1000;
    private const float _defaultRestSeconds = 1f;
    private const float _defaultAbilityCooldownSeconds = 5f;

    // Fallback gap between discrete shots for bolt-action/semi weapons when the weapon profile declares no MsPerBurst.
    private const float _defaultShotIntervalSeconds = 0.5f;

    // A weapon reads as ranged (worth firing via the projectile sim) at/above this reach; below it is melee (future).
    private const float _rangedWeaponMinRange = 10f;

    // Muzzle/chest heights (Z-up) for the line-of-sight ray, matching NpcMovementSystem's combat aim.
    private const float _losMuzzleHeight = 1.2f;
    private const float _losChestHeight = 1.0f;

    // triggerPullTime / fireRestDuration / am1Cooldown are stored in milliseconds in the SDB.
    private const float _msToSeconds = 0.001f;

    private readonly IShard _shard;
    private readonly ILogger _logger;
    private readonly HostilityResolver _hostility;
    private readonly HashSet<ulong> _logged = [];

    // Per-NPC ranged fire state: countdown to the next single-shot trigger press. NPC attacks are modelled as 1ms
    // press/release bursts, matching the original monster attack shadowfields (WeaponBurstEnded = Fired + 1ms).
    private readonly Dictionary<ulong, float> _fireRest = [];
    private readonly Dictionary<ulong, float[]> _abilityTimers = [];

    // Drives the am1/am2 "chance" roll (probability the NPC actually uses a ready ability this cycle).
    private readonly Random _rng = new();

    // The shard run-loop can spin faster than the physics timestep, so gate the per-tick logic to the physics rate (20 Hz)
    // and advance the combat timers at real time. Mirrors NpcMovementSystem.Tick.
    private double _accumulator;

    public NpcBehaviorSystem(IShard shard)
    {
        _shard = shard;
        _logger = Log.Logger.ForContext<NpcBehaviorSystem>();
        _hostility = shard.Hostility;
    }

    public void Tick(double deltaTimeMs, ulong currentTime, CancellationToken ct)
    {
        _accumulator += deltaTimeMs;
        if (_accumulator < PhysicsEngine.TimeStepMs)
        {
            return;
        }

        _accumulator = Math.Min(_accumulator - PhysicsEngine.TimeStepMs, PhysicsEngine.TimeStepMs);
        const float dt = PhysicsEngine.TimeStepSeconds;

        foreach (var entity in _shard.Entities.Values)
        {
            if (entity is not CharacterEntity { IsNpc: true } npc || npc.NpcMovement is not { } comp)
            {
                continue;
            }

            // Cleanup dead npcs
            if (!npc.IsAlive)
            {
                if (comp.HasCombat)
                {
                    comp.ClearCombat();
                    npc.SetNpcTarget(null);
                }

                StopFiring(npc);
                _abilityTimers.Remove(npc.EntityId);
                continue;
            }

            if (_logged.Add(npc.EntityId))
            {
                LogBehavior(npc.EntityId, comp.Behavior);
            }

            var config = comp.Behavior;
            var aggro = IsCombatCapable(config) ? FindAggroTarget(npc, ResolveAggroRadius(config)) : null;

            if (aggro != null)
            {
                config.ActiveMode = config.OffensiveResolved != null ? NpcBehaviorMode.Offensive : NpcBehaviorMode.Base;
                var combat = config.CurrentResolved;
                var combatDist = combat.Spacing.CombatDist ?? DefaultCombatDist(combat.Style);
                var minDist = combat.Spacing.MinDist ?? 0f;

                var engaging = !comp.HasCombat;
                var targetChanged = comp.CombatTargetId != aggro.EntityId;
                comp.SetCombat(aggro.EntityId, combatDist, minDist);

                // Publish the AI's current target on the CombatView (NPCTargetObjIdProp) so clients know who the monster
                // is attacking. Only write it when the target actually changes, to avoid dirtying the shadowfield each tick.
                if (targetChanged)
                {
                    npc.SetNpcTarget(aggro);
                }

                if (engaging)
                {
                    // Force an immediate repath to the target this tick (otherwise the NPC finishes its current wander/patrol
                    // leg first, since the periodic repath only re-reads the destination once per second).
                    comp.ResetRepathTimer(0f);
                }

                // One line-of-sight raycast per engaged NPC per tick: gates firing here, and drives strafe-vs-reposition in
                // the movement system (which runs after this one) through the component.
                var los = HasLineOfSight(npc, aggro);
                comp.SetCombatLos(los);

                RunCombatFire(npc, combat, aggro, combatDist, dt, los);
            }
            else
            {
                if (comp.HasCombat)
                {
                    comp.ClearCombat();
                    npc.SetNpcTarget(null);
                }

                StopFiring(npc);
                config.ActiveMode = NpcBehaviorMode.Base;
                ApplyIdleBehavior(npc, comp, config.BaseResolved);
            }
        }
    }

    private static string Fmt(float? value) => value?.ToString("0.##") ?? "-";

    private static string DescribeAbilities(ResolvedBehavior behavior)
    {
        if (behavior.Abilities.Count == 0)
        {
            return "none";
        }

        var parts = new List<string>(behavior.Abilities.Count);
        foreach (var a in behavior.Abilities)
        {
            parts.Add($"am{a.Slot}(id={a.Id}, cd={Fmt(a.Cooldown)}, maxDist={Fmt(a.MaxDist)}, chance={Fmt(a.Chance)})");
        }

        return string.Join("; ", parts);
    }

    private static float HorizontalDistance(Vector3 a, Vector3 b)
    {
        var dx = a.X - b.X;
        var dy = a.Y - b.Y;
        return MathF.Sqrt((dx * dx) + (dy * dy));
    }

    private static float ResolveAggroRadius(NpcBehaviorConfig config)
    {
        var b = config.BaseResolved.Perception;
        var o = config.OffensiveResolved?.Perception;
        return b.AggroDistance ?? b.PerceptionDist ?? o?.AggroDistance ?? o?.PerceptionDist ?? _defaultAggroRadius;
    }

    private static float DefaultCombatDist(BehaviorStyle style)
    {
        return style switch
        {
            BehaviorStyle.Melee or BehaviorStyle.MeleeAdditive or BehaviorStyle.MeleeFullbody or BehaviorStyle.Charger or BehaviorStyle.Kamikaze => 2f,
            BehaviorStyle.Ranged => 20f,
            _ => 10f,
        };
    }

    // A NPC can fight if it has an offensive behavior, or its calm behavior is a combat/wander archetype. Civilians
    // (idle / interaction styles) never aggro.
    private static bool IsCombatCapable(NpcBehaviorConfig config)
    {
        if (config.OffensiveResolved != null)
        {
            return true;
        }

        return config.BaseResolved.Style switch
        {
            BehaviorStyle.Ranged or BehaviorStyle.Melee or BehaviorStyle.MeleeAdditive or BehaviorStyle.MeleeFullbody or BehaviorStyle.Charger or BehaviorStyle.Kamikaze or BehaviorStyle.Wanderer => true,
            _ => false,
        };
    }

    // The NPC's effective weapon reach (0 when it has no usable weapon). Used to gate ranged firing.
    private static float WeaponRange(CharacterEntity npc)
    {
        var details = npc.GetActiveWeaponDetails();
        if (details?.Weapon == null)
        {
            return 0f;
        }

        return MathF.Max(details.Weapon.Range, details.Weapon.TargetingRange);
    }

    private static uint BurstMs(ResolvedBehavior combat)
    {
        var ms = combat.Cadence.TriggerPullTime ?? _defaultBurstMs;
        return ms < 100f ? 100u : (uint)ms;
    }

    private static float RestSeconds(ResolvedBehavior combat)
    {
        return combat.Cadence.FireRestDuration is float frd ? frd * _msToSeconds : _defaultRestSeconds;
    }

    // Single-shot NPC attacks use the behavior cadence as the gap between trigger presses. For monster ranged behaviors
    // this is normally triggerPullTime; the weapon's MsPerBurst is kept as a lower bound so the server never asks the
    // WeaponSim to press faster than the weapon can cycle.
    private static float AttackIntervalSeconds(ResolvedBehavior combat, CharacterEntity.ActiveWeaponDetails? details)
    {
        float behaviorBurst = BurstMs(combat) * _msToSeconds;
        float behaviorRest = RestSeconds(combat);
        float weaponShot = details?.WeaponProfile != null && details.WeaponProfile.MsPerBurst > 0
            ? details.WeaponProfile.MsPerBurst * _msToSeconds
            : _defaultShotIntervalSeconds;

        return Math.Max(Math.Max(behaviorBurst, behaviorRest), weaponShot);
    }

    private void LogBehavior(ulong entityId, NpcBehaviorConfig config)
    {
        var b = config.BaseResolved;
        _logger.Debug(
            "[Behavior] npc={EntityId} base={Base}[{Style}] inst={BaseInst}/{OffInst}/{DefInst} combatDist={CombatDist} triggerPullTime={TriggerPullTime} fireRestDuration={FireRestDuration} abilities=[{Abilities}] perception={Perception} aggro={Aggro} wanderDist={WanderDist} | offensive={Offensive} defensive={Defensive}",
            entityId,
            b.Name,
            b.Style,
            config.BaseInstanceId,
            config.OffensiveInstanceId,
            config.DefensiveInstanceId,
            Fmt(b.Spacing.CombatDist),
            Fmt(b.Cadence.TriggerPullTime),
            Fmt(b.Cadence.FireRestDuration),
            DescribeAbilities(b),
            Fmt(b.Perception.PerceptionDist),
            Fmt(b.Perception.AggroDistance),
            Fmt(b.Wander.Distance),
            config.OffensiveResolved?.Name ?? "-",
            config.DefensiveResolved?.Name ?? "-");
    }

    // Nearest live player the NPC is allowed to attack, within the aggro radius.
    private CharacterEntity? FindAggroTarget(CharacterEntity npc, float radius)
    {
        CharacterEntity? best = null;
        var bestDist = float.MaxValue;
        foreach (var entity in _shard.Entities.Values)
        {
            if (entity is not CharacterEntity { IsNpc: false, IsAlive: true } player)
            {
                continue;
            }

            if (!_hostility.CanDamage(npc, player))
            {
                continue;
            }

            var d = HorizontalDistance(npc.Position, player.Position);
            if (d <= radius && d < bestDist)
            {
                bestDist = d;
                best = player;
            }
        }

        return best;
    }

    // Ranged fire control: drives the WeaponSim fire state machine while the target is in weapon range AND visible.
    // NPC attacks are single-shot trigger presses: FireBurst at T, FireEnd at T+1. The next press is gated by the
    // behavior's attack interval (triggerPullTime / fireRestDuration) and the weapon's own MsPerBurst. Losing range or
    // LOS only resets the countdown; the queued 1ms release is already harmless.
    private void RunCombatFire(CharacterEntity npc, ResolvedBehavior combat, CharacterEntity target, float combatDist, float dt, bool los)
    {
        var dist = HorizontalDistance(npc.Position, target.Position);
        var range = WeaponRange(npc);

        // The LOS raycast is done once per tick by the caller and passed in (also consumed by the movement system).
        var inRange = range >= _rangedWeaponMinRange && dist <= range && los;
        var details = npc.GetActiveWeaponDetails();
        var interval = AttackIntervalSeconds(combat, details);

        var restLeft = _fireRest.TryGetValue(npc.EntityId, out var s) ? s : interval;
        if (!inRange)
        {
            _fireRest[npc.EntityId] = interval;
            RunAbilityTimers(npc, combat, target, combatDist, dist, dt, los);
            return;
        }

        restLeft -= dt;
        if (restLeft <= 0f)
        {
            var time = _shard.CurrentTime;
            EnqueueFire(npc, WeaponFireEventType.FireBurst, time);
            EnqueueFire(npc, WeaponFireEventType.FireEnd, time + 1);
            restLeft = interval;
        }

        _fireRest[npc.EntityId] = restLeft;

        RunAbilityTimers(npc, combat, target, combatDist, dist, dt, los);
    }

    // Clears the NPC's fire countdown when it leaves combat. The 1ms presses already enqueue their own FireEnd, so no
    // held trigger needs to be released here.
    private void StopFiring(CharacterEntity npc)
    {
        _fireRest.Remove(npc.EntityId);
    }

    // Drives the WeaponSim fire state machine with a timestamped input. The CombatView burst shadowfields are stamped by
    // WeaponSim at the authoritative shot/ability-fire time, not here, so delayed BurstAbility attacks animate at the
    // time the projectile is actually fired.
    private void EnqueueFire(CharacterEntity npc, WeaponFireEventType type, uint time)
    {
        _shard.WeaponSim.EnqueueFireEvent(npc, new WeaponFireEvent { Time = time, Type = type });
    }

    // True when nothing but the target itself blocks the muzzle->chest ray. Static terrain and other entities (deployables,
    // vehicles) occlude; other NPCs are ignored so a crowd never blinds the shooter. A hit on the target counts as clear.
    private bool HasLineOfSight(CharacterEntity npc, CharacterEntity target)
    {
        var from = new Vector3(npc.Position.X, npc.Position.Y, npc.Position.Z + _losMuzzleHeight);
        var to = new Vector3(target.Position.X, target.Position.Y, target.Position.Z + _losChestHeight);
        var hit = _shard.Physics.SegmentRayCast(from, to, npc.EntityId, ignoreNpcs: true);
        return !hit.Hit || hit.HitEntityId == target.EntityId;
    }

    // Ability (am1/am2) cadence: when a slot's cooldown has elapsed, the target is within the ability's max distance,
    // the NPC can see the target, and the behavior's chance roll passes, activate the ability (see CastAbility). The
    // cooldown is spent whether or not the chance roll succeeds, so a failed roll just skips this cycle.
    private void RunAbilityTimers(CharacterEntity npc, ResolvedBehavior combat, CharacterEntity target, float combatDist, float dist, float dt, bool los)
    {
        if (combat.Abilities.Count == 0)
        {
            return;
        }

        if (!_abilityTimers.TryGetValue(npc.EntityId, out var timers))
        {
            timers = new float[3];
            _abilityTimers[npc.EntityId] = timers;
        }

        foreach (var a in combat.Abilities)
        {
            if (a.Slot < 1 || a.Slot > 2)
            {
                continue;
            }

            var cooldown = a.Cooldown is float c ? c * _msToSeconds : _defaultAbilityCooldownSeconds;
            var maxDist = a.MaxDist ?? combatDist;
            timers[a.Slot] -= dt;
            if (timers[a.Slot] > 0f || dist > maxDist || !los)
            {
                continue;
            }

            timers[a.Slot] = cooldown;

            if (a.Id == 0)
            {
                _logger.Debug("[Behavior] npc={EntityId} ability am{Slot} ready but has no ability id", npc.EntityId, a.Slot);
                continue;
            }

            // am*Chance is a 0..1 probability (null = always). Values >= 1 always fire; a failed roll skips this cycle.
            var chance = a.Chance ?? 1f;
            if (chance < 1f && _rng.NextSingle() > Math.Clamp(chance, 0f, 1f))
            {
                continue;
            }

            CastAbility(npc, target, (uint)a.Id, a.Slot);
        }
    }

    // Activates an NPC ability. Projectile abilities queue a PendingAbilityProjectile on the NPC (possibly after a
    // delayed UpdateWaitAndFireOnce chain); WeaponSim then fires it for NPCs, since they have no client
    // FireWeaponProjectile to consume it. Non-projectile abilities run entirely inside the chain.
    private void CastAbility(CharacterEntity npc, CharacterEntity target, uint abilityId, int slot)
    {
        var time = _shard.CurrentTime;
        _shard.Abilities.HandleActivateAbility(_shard, npc, abilityId, time, new AptitudeTargets(target));

        _logger.Debug(
            "[Behavior] npc={EntityId} cast am{Slot}Id={AbilityId} -> target={TargetId}",
            npc.EntityId,
            slot,
            abilityId,
            target.EntityId);
    }

    // Calm/idle behavior when not in combat: wanderers roam, idle/stand behaviors hold in place.
    private void ApplyIdleBehavior(CharacterEntity npc, NpcMovementComponent comp, ResolvedBehavior baseBehavior)
    {
        switch (baseBehavior.Style)
        {
            case BehaviorStyle.Wanderer:
                if (!comp.HasWander)
                {
                    var radius = baseBehavior.Wander.Distance ?? NpcMovementSystem.WanderDefaultRadius;
                    comp.SetWander(npc.Position, radius);
                }

                break;

            case BehaviorStyle.Idle:
                if (comp.IsMoving && !comp.HasPatrol && !comp.HasWander)
                {
                    comp.Stop();
                }

                break;
        }
    }
}
