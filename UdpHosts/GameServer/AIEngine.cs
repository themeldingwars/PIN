using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Threading;
using AeroMessages.GSS.Character;
using AeroMessages.GSS.Character.Event;
using GameServer.Entities;
using GameServer.Entities.Character;
using GameServer.Entities.Thumper;
using GameServer.StaticDB;
using GameServer.StaticDB.Records.dbcharacter;
using Serilog;

namespace GameServer;

public readonly record struct AiBrainDiagnostic(
    ulong EntityId,
    ulong TargetEntityId,
    string Mode,
    Vector3 Home,
    bool Aggressive,
    bool HasWeapon,
    ulong NextFireAt,
    string Order);

// Orders are the interface between content systems (encounters, missions)
// and NPC brains. A brain that has no order uses its default behavior.
// A brain that has an order obeys the order. An order overrides the leash.
// Refer to codex/ai-design.md section 4.
public abstract record AiOrder;
public sealed record AttackEntityOrder(ulong TargetEntityId) : AiOrder;
public sealed record DefendPointOrder(Vector3 Position, float Radius) : AiOrder;
public sealed record MoveToOrder(Vector3 Position) : AiOrder;
public sealed record FollowEntityOrder(ulong TargetEntityId, float Distance) : AiOrder;

/// <summary>
///     The server-side NPC brain system. It controls targets, orders,
///     movement classes, combat styles, and weapon fire. Species behavior
///     is in partial class files (example: AIEngine.Aranha.cs). The
///     Engage() method calls the species hooks by name.
/// </summary>
public partial class AIEngine
{
    /// <summary>
    ///     The position of the NPC muzzle on the rig capsule, as a fraction
    ///     of the capsule height. The "npcfx" admin command can change this
    ///     value at run time. The default values in this group come from
    ///     live tests with aranhas.
    /// </summary>
    public static float NpcMuzzleFraction = 0.9f;

    /// <summary>
    ///     The hardpoint id sent in AbilityProjectileFired. The client uses
    ///     it to attach the visible projectile. Hardpoint 2 is the aranha
    ///     spit muzzle. The ability chain of the Aranha Queen names it.
    /// </summary>
    public static uint NpcFxHardpoint = 2;

    /// <summary>
    ///     The visual projectile start offset, in local meters
    ///     (right, forward, up). Sent in AbilityProjectileFired.
    /// </summary>
    public static Vector3 NpcFxOffset = new(0f, 0.2f, 0.9f);

    /// <summary>
    ///     A debug aid ("npcfx aimpoint" admin command). When this value is
    ///     set, each armed hostile NPC stops and fires at this point. The
    ///     NPC does not attack players in this mode.
    /// </summary>
    public static Vector3? NpcAimPointOverride = null;

    // ---- Think interval ----------------------------------------------------
    // Brains think 4 times each second. Each think writes movement data.
    // The client interpolates between the writes. All times in this class
    // are shard milliseconds.
    private const ulong ThinkIntervalMs = 250;

    // ---- Target selection and leash (values in meters) ---------------------
    private const float DefaultAggroRange = 45f;      // player detection radius, used when the monster row has no value
    private const ulong TargetMemoryMs = 5000;        // an NPC follows a target it cannot see for this many milliseconds
    private const float LeashRange = 80f;             // an NPC farther than this from home stops its attack and returns
    private const float HomeArriveRange = 3f;         // an NPC nearer to home than this has arrived
    private const float OrderArriveRange = 2.5f;      // a MoveToOrder nearer to its goal than this is complete
    private const float ThumperAggroRange = 60f;      // hostile NPCs in this radius attack an active thumper

    // ---- Combat-style hold distances (values in meters) --------------------
    // GetProfile selects a combat style for each monster type. The weapon
    // range and the behavior tokens control the selection. These constants
    // are the standard hold distances for each style.
    private const float MeleeRange = 2.5f;
    private const float RangedHoldRange = 18f;
    private const float SniperHoldRange = 38f;

    // ---- Movement ----------------------------------------------------------
    private const float DefaultNormalSpeed = 4.5f;    // walk speed in m/s, used when the monster row has no value
    private const float DefaultFastSpeed = 7f;        // chase speed in m/s, used when the monster row has no value
    private const float FlyerHoverHeight = 4f;        // a flyer stays this many meters above a target on the ground

    // ---- Weapon fire timing (values in milliseconds) -----------------------
    private const uint DefaultTriggerPullMs = 1500;   // burst length, used when the weapon has no value
    private const uint DefaultFireRestMs = 500;       // pause between bursts

    // ---- Thumper melee damage (PLACEHOLDER) --------------------------------
    // Thumper integrity is a counter in the encounter code. Real weapon
    // damage will replace this. Refer to codex/damage-design.md, phase D1.
    private const uint ThumperDamagePerHit = 4;
    private const ulong ThumperHitCooldownMs = 2500;

    // ---- Movement-state words (replicated client animation states) ---------
    private const short StandingMovementState = 0x1000;
    private const short RunningMovementState = 0x2004;
    private const short FallingMovementState = 0x3004;

    // A monster behavior string can contain tunable values.
    // Example: "GiantAranhaMiniBoss(eggSackCooldown=10000)".
    private static readonly Regex BehaviorParamRegex = new(@"(?<key>[A-Za-z0-9_]+)\s*=\s*(?<value>-?[0-9.]+)", RegexOptions.Compiled);

    private readonly IShard _shard;
    private readonly ILogger _logger;

    // Each living NPC has one BrainState, with the entity id as the key.
    // Each character type has one AiProfile, derived from SDB data one time
    // and then cached. Orders and passive overrides exist only for the
    // entities that have them.
    private readonly ConcurrentDictionary<ulong, BrainState> _brains = new();
    private readonly ConcurrentDictionary<ulong, AiOrder> _orders = new();
    private readonly ConcurrentDictionary<uint, AiProfile> _profiles = new();
    private readonly ConcurrentDictionary<ulong, bool> _passiveOverrides = new();
    private Dictionary<(uint A, uint B), sbyte> _factionStance;
    private Dictionary<uint, sbyte> _factionDefaultStance;
    private ulong _nextThinkAt;

    public AIEngine(IShard shard)
    {
        _shard = shard;
        _logger = shard.Logger.ForContext<AIEngine>();
    }

    /// <summary>Idle = ambient behavior at home; Return = walking back after a leash or lost target.</summary>
    private enum BrainMode
    {
        Idle,
        Return,
    }

    /// <summary>Returns a diagnostics record for each live brain. The /api/snapshot endpoint serves this data.</summary>
    public AiBrainDiagnostic[] SnapshotBrains()
    {
        return _brains.Select(pair =>
                                  {
                                      var brain = pair.Value;
                                      _orders.TryGetValue(pair.Key, out var order);
                                      return new AiBrainDiagnostic(
                                          pair.Key,
                                          brain.TargetEntityId,
                                          brain.Mode.ToString(),
                                          brain.Home,
                                          brain.Profile.Aggressive && !IsPassive(pair.Key),
                                          brain.Profile.HasWeapon,
                                          brain.NextFireAt,
                                          order?.ToString() ?? string.Empty);
                                  })
                      .ToArray();
    }

    /// <summary>Gives an order to an NPC brain. The new order replaces the current order.</summary>
    public void IssueOrder(ulong entityId, AiOrder order)
    {
        _orders[entityId] = order;
    }

    /// <summary>Removes the order of an NPC. The brain then uses its default behavior.</summary>
    public void ClearOrder(ulong entityId)
    {
        _orders.TryRemove(entityId, out _);
    }

    /// <summary>Sets the passive override for one entity. A passive NPC does not select targets. The faction has no effect on this.</summary>
    public void SetPassive(ulong entityId, bool passive)
    {
        if (passive)
        {
            _passiveOverrides[entityId] = true;
        }
        else
        {
            _passiveOverrides.TryRemove(entityId, out _);
        }
    }

    /// <summary>Returns true when the entity has a passive override. Refer to <see cref="SetPassive"/>.</summary>
    public bool IsPassive(ulong entityId)
    {
        return _passiveOverrides.TryGetValue(entityId, out var passive) && passive;
    }

    /// <summary>Removes all cached profiles and brains. New brains then use the current GetProfile logic.</summary>
    public int ClearProfiles()
    {
        var count = _profiles.Count;
        _profiles.Clear();
        _brains.Clear();
        return count;
    }

    /// <summary>Sets the passive override on each live brain. This is a debug aid.</summary>
    public int SetAllPassive(bool passive)
    {
        var count = 0;
        foreach (var entityId in _brains.Keys.ToArray())
        {
            SetPassive(entityId, passive);
            count++;
        }

        return count;
    }

    /// <summary>Returns true when the faction is hostile to the player faction. Players spawn as faction 1 (Accord).</summary>
    public bool IsHostileTowardPlayers(uint npcFactionId)
    {
        return IsHostileTowardFaction(npcFactionId, 1);
    }

    /// <summary>
    ///     The shard-tick entry point. The method runs one time for each
    ///     think interval. It ticks the brain of each living NPC. It then
    ///     applies creature separation. It removes the state of entities
    ///     that no longer exist.
    /// </summary>
    public void Tick(double deltaTime, ulong currentTime, CancellationToken ct)
    {
        if (currentTime < _nextThinkAt)
        {
            return;
        }

        _nextThinkAt = currentTime + ThinkIntervalMs;
        var deltaSeconds = Math.Max((float)deltaTime / 1000f, ThinkIntervalMs / 1000f);

        var seen = new HashSet<ulong>();
        foreach (var character in _shard.Entities.Values.OfType<CharacterEntity>().ToArray())
        {
            if (ct.IsCancellationRequested)
            {
                return;
            }

            if (character.IsPlayerControlled || !IsLiving(character))
            {
                _brains.TryRemove(character.EntityId, out _);
                _orders.TryRemove(character.EntityId, out _);
                continue;
            }

            seen.Add(character.EntityId);
            TickNpc(character, deltaSeconds, currentTime);
            ApplySeparation(character, currentTime);
        }

        // Remove the brains and the orders of entities that no longer exist
        foreach (var staleId in _brains.Keys.Where(id => !seen.Contains(id)).ToArray())
        {
            _brains.TryRemove(staleId, out _);
            _orders.TryRemove(staleId, out _);
            _passiveOverrides.TryRemove(staleId, out _);
        }

        foreach (var staleId in _orders.Keys.Where(id => !seen.Contains(id)).ToArray())
        {
            _orders.TryRemove(staleId, out _);
        }
    }

    /// <summary>
    ///     Returns a well-mixed hash value between 0 and 1. Entity ids
    ///     increase in steps of 256. Because of this, simple bit selection
    ///     gives only a small set of values. Always use this hash for
    ///     per-entity random values.
    /// </summary>
    private static float Hash01(ulong id, uint salt)
    {
        var h = (uint)(id >> 8) ^ salt;
        h *= 2654435761u;
        h ^= h >> 16;
        return (h & 0xFFFF) / 65536f;
    }

    /// <summary>Returns the body radius for creature separation. It is the pose physics radius multiplied by the visual scale. It covers the body only, not the legs.</summary>
    private static float ComputeBodyRadius(CharacterEntity npc)
    {
        if (npc.Collision is CharacterCollisionComponent collision)
        {
            var baseRadius = collision.PoseTypeRecord?.PhysicsRadius ?? 0.5f;
            if (baseRadius <= 0.05f)
            {
                baseRadius = 0.5f;
            }

            return Math.Clamp(baseRadius * Math.Max(collision.Scale, 0.1f), 0.3f, 2f);
        }

        return 0.5f;
    }

    private static bool ContainsAny(string value, params string[] tokens)
    {
        foreach (var token in tokens)
        {
            if (value.Contains(token, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    ///     One think for one brain. The priority sequence is:
    ///     1. Continue an active leap. 2. Obey an order. 3. Return home.
    ///     4. For hostile NPCs only: select a target and engage it.
    /// </summary>
    private void TickNpc(CharacterEntity npc, float deltaSeconds, ulong currentTime)
    {
        var brain = _brains.GetOrAdd(npc.EntityId, _ => new BrainState
        {
            Home = npc.Position,
            Profile = GetProfile(npc),
            BodyRadius = ComputeBodyRadius(npc),
        });

        if (!brain.WeaponAligned)
        {
            brain.WeaponAligned = true;
            if (brain.Profile.PreferredWeaponIndex != 0 && npc.WeaponIndex.Index != brain.Profile.PreferredWeaponIndex)
            {
                npc.SetWeaponIndex(new WeaponIndexData
                {
                    Index = brain.Profile.PreferredWeaponIndex, Unk1 = 1, Unk2 = 0, Time = _shard.CurrentTime
                });
            }
        }

        // While a leap is active, only the leap controls the NPC
        if (brain.LeapEndsAt > currentTime)
        {
            TickLeap(npc, brain, currentTime);
            return;
        }

        // An order has priority over all other behavior, the leash included
        if (TryExecuteOrder(npc, brain, deltaSeconds, currentTime))
        {
            return;
        }

        if (brain.Mode == BrainMode.Return)
        {
            TickReturn(npc, brain, deltaSeconds, currentTime);
            return;
        }

        if (!brain.Profile.Aggressive || IsPassive(npc.EntityId))
        {
            // This NPC is not hostile, or a passive override is set.
            // The NPC stands still. Wander behavior is a future feature.
            return;
        }

        // Debug aim-point mode: fire at the fixed point and ignore targets
        if (NpcAimPointOverride is Vector3 aimPoint && brain.Profile.HasWeapon)
        {
            var pointAim = ComputeAttackAim(npc, aimPoint);
            if (pointAim != Vector3.Zero)
            {
                SetMovement(npc, pointAim, StandingMovementState);
                if (currentTime >= brain.NextFireAt)
                {
                    npc.SetAimDirection(pointAim);
                    AnnounceBurst(npc, pointAim);
                    _shard.WeaponSim.OnFireWeaponProjectile(npc, _shard.CurrentTime, pointAim);
                    brain.NextFireAt = currentTime + brain.Profile.FireCooldownMs;
                }
            }

            return;
        }

        var (playerTarget, thumperTarget) = ResolveTarget(npc, brain, currentTime);
        if (playerTarget == null && thumperTarget == null)
        {
            var hadTarget = brain.TargetEntityId != 0;
            brain.TargetEntityId = 0;

            if (Vector3.Distance(npc.Position, brain.Home) > HomeArriveRange * 2)
            {
                brain.Mode = BrainMode.Return;
                return;
            }

            SetMovement(npc, Vector3.Zero, StandingMovementState);
            if (hadTarget)
            {
                _shard.Movement.BroadcastCharacterPose(npc);
            }

            return;
        }

        // Leash: the NPC is too far from home. It stops the attack and returns.
        if (Vector3.Distance(npc.Position, brain.Home) > LeashRange)
        {
            _logger.Debug("NPC {NpcId} leashing, returning home", npc.EntityId);
            brain.TargetEntityId = 0;
            brain.Mode = BrainMode.Return;
            return;
        }

        Engage(npc, brain, playerTarget, thumperTarget, deltaSeconds, currentTime);
    }

    /// <summary>
    ///     Obeys the current order of the entity, if the entity has one.
    ///     Returns true when an order used this think. A complete order
    ///     removes itself. Examples: the NPC arrives, or the target is dead.
    /// </summary>
    private bool TryExecuteOrder(CharacterEntity npc, BrainState brain, float deltaSeconds, ulong currentTime)
    {
        if (!_orders.TryGetValue(npc.EntityId, out var order))
        {
            return false;
        }

        brain.Mode = BrainMode.Idle;
        switch (order)
        {
            case AttackEntityOrder attack:
                _shard.Entities.TryGetValue(attack.TargetEntityId, out var target);
                if (target is ThumperEntity { IsAttackable: true } thumper)
                {
                    brain.TargetEntityId = thumper.EntityId;
                    Engage(npc, brain, null, thumper, deltaSeconds, currentTime);
                    return true;
                }

                if (target is CharacterEntity character && IsLiving(character))
                {
                    brain.TargetEntityId = character.EntityId;
                    brain.TargetLastSeenAt = currentTime;
                    Engage(npc, brain, character, null, deltaSeconds, currentTime);
                    return true;
                }

                // The target is destroyed or removed. The order is complete.
                ClearOrder(npc.EntityId);
                return false;

            case MoveToOrder move:
                if (MoveTowardGoal(npc, brain, move.Position, OrderArriveRange, deltaSeconds, currentTime))
                {
                    ClearOrder(npc.EntityId);
                }

                return true;

            case DefendPointOrder defend:
                var intruder = FindNearestPlayer(npc, defend.Radius, defend.Position);
                if (intruder != null)
                {
                    brain.TargetEntityId = intruder.EntityId;
                    brain.TargetLastSeenAt = currentTime;
                    Engage(npc, brain, intruder, null, deltaSeconds, currentTime);
                    return true;
                }

                brain.TargetEntityId = 0;
                MoveTowardGoal(npc, brain, defend.Position, HomeArriveRange, deltaSeconds, currentTime);
                return true;

            case FollowEntityOrder follow:
                _shard.Entities.TryGetValue(follow.TargetEntityId, out var leaderEntity);
                if (leaderEntity is not CharacterEntity leader || !IsLiving(leader))
                {
                    ClearOrder(npc.EntityId);
                    return false;
                }

                if (Vector3.Distance(npc.Position, leader.Position) > follow.Distance)
                {
                    MoveTowardGoal(npc, brain, leader.Position, follow.Distance, deltaSeconds, currentTime);
                }
                else
                {
                    SetMovement(npc, SafeNormalize(leader.Position - npc.Position), StandingMovementState);
                    BroadcastPoseIfDue(npc, brain, currentTime);
                }

                return true;

            default:
                return false;
        }
    }

    /// <summary>Moves the NPC toward a goal position. The movement class controls the path. Returns true when the NPC arrives.</summary>
    private bool MoveTowardGoal(CharacterEntity npc, BrainState brain, Vector3 goal, float arriveRange, float deltaSeconds, ulong currentTime)
    {
        var toGoal = goal - npc.Position;
        var distance = toGoal.Length();
        if (distance <= arriveRange || brain.Profile.Stationary)
        {
            SetMovement(npc, Vector3.Zero, StandingMovementState);
            BroadcastPoseIfDue(npc, brain, currentTime);
            return distance <= arriveRange;
        }

        Vector3 next;
        if (brain.Profile.Flying)
        {
            var step = Math.Min(brain.Profile.FastSpeed * deltaSeconds, distance);
            next = npc.Position + (SafeNormalize(toGoal) * step);
        }
        else
        {
            var horizontal = new Vector3(toGoal.X, toGoal.Y, 0f);
            var horizontalDistance = horizontal.Length();
            var step = Math.Min(brain.Profile.FastSpeed * deltaSeconds, Math.Max(horizontalDistance, 0.01f));
            next = npc.Position + (SafeNormalize(horizontal) * step);
            var fallbackZ = npc.Position.Z + Math.Clamp(goal.Z - npc.Position.Z, -step, step);
            next.Z = _shard.Physics.GetGroundHeight(next) ?? fallbackZ;
        }

        SetMovement(npc, SafeNormalize(toGoal), RunningMovementState, next);
        BroadcastPoseIfDue(npc, brain, currentTime);
        return false;
    }

    /// <summary>
    ///     The combat core. It moves the NPC and makes attacks against the
    ///     target. The target is a player or a thumper. When both are set,
    ///     the thumper is the target. The method first calls the species
    ///     hooks (refer to AIEngine.Aranha.cs). A species hook can complete
    ///     the think and return. If no hook completes the think, movement
    ///     runs for the movement class of the NPC. An attack attempt follows.
    /// </summary>
    private void Engage(CharacterEntity npc, BrainState brain, CharacterEntity playerTarget, ThumperEntity thumperTarget, float deltaSeconds, ulong currentTime)
    {
        var targetPosition = thumperTarget?.Position ?? playerTarget.Position;

        // Against a thumper, each creature uses its own combat range.
        // The minimum is 2.5 m, so that no creature enters the drill model.
        // Melee types attack the hull. Ranged types fire from their hold
        // distance. An earlier constant 8 m range kept all creatures in one
        // wide circle, out of the reach of their attacks.
        var contactRange = thumperTarget != null
            ? Math.Max(2.5f, brain.Profile.PreferredRange)
            : brain.Profile.PreferredRange;
        if (brain.Profile.Flying)
        {
            // The hover point of a flyer is above the target. The contact
            // range must not be smaller than the hover height, because the
            // flyer can never reach such a range.
            contactRange = Math.Max(contactRange, FlyerHoverHeight + 1f);
        }

        var toTarget = targetPosition - npc.Position;
        var distance = toTarget.Length();
        var aimDirection = SafeNormalize(toTarget);

        // A ranged NPC always keeps its pose aimed along the ballistic
        // firing solution, also while it moves. The client launches the
        // visible tracer along the pose aim. An earlier version changed the
        // aim between the flat chase direction and the pitched arc. That
        // made the models rock, and shots fired during movement hit the
        // ground at the feet of the NPC.
        if (brain.Profile.HasWeapon && brain.Profile.PreferredRange > MeleeRange && playerTarget != null)
        {
            var attackAim = ComputeAttackAim(npc, targetPosition);
            if (attackAim != Vector3.Zero)
            {
                aimDirection = attackAim;
            }
        }

        // Ground units must not move along the full 3D direction. That
        // walked NPCs into the air when a player used the jetpack. A ground
        // unit follows the target in the horizontal plane only. It follows
        // the target height at a limited slope, and only while the target
        // is on the ground. The Z of a target on the ground is equal to the
        // terrain height at the target position.
        var horizontal = new Vector3(toTarget.X, toTarget.Y, 0f);
        var horizontalDistance = horizontal.Length();
        var moveDirection = SafeNormalize(horizontal);
        var targetGrounded = thumperTarget != null || !playerTarget.IsAirborne;

        if (thumperTarget == null && brain.HasPerch)
        {
            brain.HasPerch = false;
        }

        // The aranha species hooks (AIEngine.Aranha.cs): attach to a hull
        // at a perch point, pounce on players, or leap onto a thumper hull
        if (IsAranhaLike(brain.Profile))
        {
            if (thumperTarget != null)
            {
                contactRange = AranhaHullApproachRange(npc);
                if (TryAranhaLatch(npc, brain, thumperTarget, toTarget, horizontalDistance, deltaSeconds, currentTime))
                {
                    return;
                }
            }

            if (playerTarget != null && thumperTarget == null
                && TryAranhaPounce(npc, brain, playerTarget, toTarget, horizontalDistance, currentTime))
            {
                return;
            }

            if (thumperTarget != null
                && TryAranhaHullLeap(npc, brain, thumperTarget, toTarget, horizontalDistance, currentTime))
            {
                return;
            }
        }

        if (brain.Profile.Stationary)
        {
            // Turrets and towers turn to the target. They do not move.
            SetMovement(npc, aimDirection, StandingMovementState);
        }
        else if (brain.Profile.Flying)
        {
            // A flyer moves in full 3D to a hover point. The hover point is
            // above a target on the ground. For a target in the air, the
            // hover point is level with the target.
            var goal = targetGrounded ? targetPosition + new Vector3(0f, 0f, FlyerHoverHeight) : targetPosition;
            var toGoal = goal - npc.Position;
            var goalDistance = toGoal.Length();
            if (distance <= contactRange)
            {
                SetMovement(npc, aimDirection, StandingMovementState);
            }
            else
            {
                var step = Math.Min(brain.Profile.FastSpeed * deltaSeconds, Math.Max(goalDistance - contactRange, 0f));
                SetMovement(npc, aimDirection, RunningMovementState, npc.Position + (SafeNormalize(toGoal) * step));
            }
        }
        else if (playerTarget != null && brain.Profile.PreferredRange > MeleeRange
                 && distance < brain.Profile.PreferredRange * 0.5f
                 && moveDirection != Vector3.Zero)
        {
            // The target is inside the hold distance. The NPC moves back.
            // This check uses the 3D distance. A player high on a cliff is
            // not too near, although the horizontal distance is small. A
            // move back would decrease the height that the weapon can reach.
            var step = brain.Profile.NormalSpeed * deltaSeconds;
            var next = npc.Position - (moveDirection * step);
            next.Z = _shard.Physics.GetGroundHeight(next) ?? npc.Position.Z;
            SetMovement(npc, aimDirection, RunningMovementState, next);
        }
        else if (distance <= contactRange || moveDirection == Vector3.Zero)
        {
            // The target is in reach, or the NPC is directly below a target
            // in the air. The NPC stands and aims.
            SetMovement(npc, aimDirection, StandingMovementState);
        }
        else
        {
            // Melee NPCs hunt as a pack. Some attack directly. Some move
            // around the target. Each NPC has its own speed factor. Refer
            // to ComputeMeleePackGoal in AIEngine.Aranha.cs.
            var moveTo = targetPosition;
            var gait = 1f;
            if (playerTarget != null && brain.Profile.PreferredRange <= 4f)
            {
                moveTo = ComputeMeleePackGoal(npc, targetPosition, contactRange, horizontalDistance, currentTime, out gait);
            }

            var goalDelta = new Vector3(moveTo.X - npc.Position.X, moveTo.Y - npc.Position.Y, 0f);
            var goalDistance = goalDelta.Length();
            var goalDirection = goalDistance > 0.01f ? goalDelta / goalDistance : moveDirection;
            var step = Math.Min(brain.Profile.FastSpeed * gait * deltaSeconds, moveTo == targetPosition ? Math.Max(horizontalDistance - contactRange, 0f) : goalDistance);
            var next = npc.Position + (goalDirection * step);

            // Use the real terrain height when zone collision data is
            // loaded. If not, follow the height of a target on the ground
            // at a limited slope. For a climber that attacks a thumper, the
            // thumper hull is also a walkable surface.
            var walkableId = (thumperTarget != null && brain.Profile.CanClimb) ? thumperTarget.EntityId : 0UL;
            var fallbackZ = targetGrounded
                ? npc.Position.Z + Math.Clamp(targetPosition.Z - npc.Position.Z, -step, step)
                : npc.Position.Z;
            next.Z = _shard.Physics.GetGroundHeight(next, walkableId) ?? fallbackZ;

            SetMovement(npc, aimDirection, RunningMovementState, next);
        }

        if (thumperTarget != null)
        {
            // This check uses the horizontal distance, the same measure as
            // the movement code. The movement code stops an NPC at exactly
            // the contact range, measured horizontally. The feet of the NPC
            // are a small distance above or below the thumper base. Because
            // of this, a 3D distance check against the same constant always
            // failed by a few millimeters. The NPCs then never attacked.
            TryHitThumper(npc, brain, thumperTarget, horizontalDistance, contactRange, currentTime);
        }
        else
        {
            TryFire(npc, brain, playerTarget, distance, currentTime);
        }

        BroadcastPoseIfDue(npc, brain, currentTime);
    }

    /// <summary>
    ///     Keeps creatures apart. Each creature has a horizontal disc with
    ///     the width of its body (physics radius multiplied by scale). Other
    ///     creatures must stay out of this disc. Without this rule, a pack
    ///     moves into one point, and one burst of fire kills all of them.
    /// </summary>
    private void ApplySeparation(CharacterEntity npc, ulong currentTime)
    {
        if (!_brains.TryGetValue(npc.EntityId, out var brain)
            || brain.Profile.Stationary
            || brain.LeapEndsAt > currentTime
            || brain.HasPerch)
        {
            return;
        }

        var myRadius = brain.BodyRadius;
        var push = Vector3.Zero;
        foreach (var entity in _shard.Entities.Values)
        {
            if (entity is not CharacterEntity other
                || other.EntityId == npc.EntityId
                || other.IsPlayerControlled
                || !IsLiving(other))
            {
                continue;
            }

            var otherRadius = _brains.TryGetValue(other.EntityId, out var otherBrain) ? otherBrain.BodyRadius : 0.5f;
            var minDistance = myRadius + otherRadius;
            var dx = npc.Position.X - other.Position.X;
            var dy = npc.Position.Y - other.Position.Y;
            var distanceSq = (dx * dx) + (dy * dy);
            if (distanceSq >= minDistance * minDistance || distanceSq < 0.000001f)
            {
                if (distanceSq < 0.000001f)
                {
                    // The two creatures are at the same point. Push this
                    // creature in a direction set by its entity id.
                    var angle = (npc.EntityId % 360) * (MathF.PI / 180f);
                    push += new Vector3(MathF.Cos(angle), MathF.Sin(angle), 0f) * minDistance;
                }

                continue;
            }

            var distance = MathF.Sqrt(distanceSq);
            var overlap = minDistance - distance;
            push += new Vector3(dx / distance, dy / distance, 0f) * overlap;
        }

        if (push == Vector3.Zero)
        {
            return;
        }

        // Limit the push for each think. The creatures then move apart
        // slowly, which looks natural.
        var magnitude = push.Length();
        if (magnitude > 1.2f)
        {
            push *= 1.2f / magnitude;
        }

        var corrected = npc.Position + push;
        var walkableId = 0UL;
        if (brain.Profile.CanClimb && brain.TargetEntityId != 0
            && _shard.Entities.TryGetValue(brain.TargetEntityId, out var climbTarget)
            && climbTarget is Entities.Thumper.ThumperEntity)
        {
            walkableId = brain.TargetEntityId;
        }

        corrected.Z = _shard.Physics.GetGroundHeight(corrected, walkableId) ?? npc.Position.Z;
        npc.SetPosition(corrected);
    }

    /// <summary>Moves the NPC back to its home point. This runs after a leash stop or a lost target.</summary>
    private void TickReturn(CharacterEntity npc, BrainState brain, float deltaSeconds, ulong currentTime)
    {
        var toHome = brain.Home - npc.Position;
        var distance = toHome.Length();
        if (distance <= HomeArriveRange)
        {
            brain.Mode = BrainMode.Idle;
            SetMovement(npc, Vector3.Zero, StandingMovementState);
            _shard.Movement.BroadcastCharacterPose(npc);
            return;
        }

        var direction = SafeNormalize(toHome);
        var step = Math.Min(brain.Profile.FastSpeed * deltaSeconds, distance);
        SetMovement(npc, direction, RunningMovementState, npc.Position + (direction * step));
        BroadcastPoseIfDue(npc, brain, currentTime);
    }

    /// <summary>
    ///     Default target selection. The brain keeps its current target
    ///     while the target is valid. A lost player target stays in memory
    ///     for TargetMemoryMs. If there is no target, the brain selects the
    ///     nearest player in the detection range. If there is no player,
    ///     the brain selects an attackable thumper in ThumperAggroRange.
    /// </summary>
    private (CharacterEntity Player, ThumperEntity Thumper) ResolveTarget(CharacterEntity npc, BrainState brain, ulong currentTime)
    {
        // Keep the current target while the target is valid
        if (brain.TargetEntityId != 0)
        {
            if (_shard.Entities.TryGetValue(brain.TargetEntityId, out var existing) && existing is ThumperEntity currentThumper)
            {
                if (currentThumper.IsAttackable)
                {
                    return (null, currentThumper);
                }
            }
            else
            {
                // A player target stays in memory for a short time after it
                // leaves the detection range
                var current = FindPlayerById(brain.TargetEntityId);
                if (current != null)
                {
                    var distance = Vector3.Distance(npc.Position, current.Position);
                    if (distance <= brain.Profile.AggroRange)
                    {
                        brain.TargetLastSeenAt = currentTime;
                    }

                    if (currentTime - brain.TargetLastSeenAt <= TargetMemoryMs)
                    {
                        return (current, null);
                    }
                }
            }

            brain.TargetEntityId = 0;
        }

        var thumper = FindNearestAttackableThumper(npc, ThumperAggroRange);
        var player = FindNearestPlayer(npc, brain.Profile.AggroRange);

        // The default behavior prefers a player target. An attack on an
        // objective, for example a thumper, usually comes from an order
        // that the encounter gives.
        if (thumper != null && player == null)
        {
            brain.TargetEntityId = thumper.EntityId;
            _logger.Debug("NPC {NpcId} acquired thumper {TargetId}", npc.EntityId, thumper.EntityId);
            return (null, thumper);
        }

        if (player != null)
        {
            brain.TargetEntityId = player.EntityId;
            brain.TargetLastSeenAt = currentTime;
            _logger.Debug("NPC {NpcId} acquired target {TargetId}", npc.EntityId, player.EntityId);
        }

        return (player, null);
    }

    /// <summary>Returns the nearest attackable thumper in the given range, or null.</summary>
    private ThumperEntity FindNearestAttackableThumper(CharacterEntity npc, float range)
    {
        ThumperEntity nearest = null;
        var nearestDistanceSq = range * range;

        foreach (var entity in _shard.Entities.Values)
        {
            if (entity is not ThumperEntity thumper || !thumper.IsAttackable)
            {
                continue;
            }

            if (!IsHostileTowardFaction(GetNpcFactionId(npc), thumper.HostilityInfo.FactionId))
            {
                continue;
            }

            var distanceSq = Vector3.DistanceSquared(npc.Position, thumper.Position);
            if (distanceSq < nearestDistanceSq)
            {
                nearest = thumper;
                nearestDistanceSq = distanceSq;
            }
        }

        return nearest;
    }

    /// <summary>
    ///     Makes a melee strike against a thumper when the NPC is inside
    ///     the contact range. The distance parameter is horizontal.
    ///     PLACEHOLDER: the damage is a fixed integrity value. The
    ///     DamageService will replace it.
    /// </summary>
    private void TryHitThumper(CharacterEntity npc, BrainState brain, ThumperEntity thumper, float distance, float contactRange, ulong currentTime)
    {
        // The 0.25 m tolerance is necessary. The movement code stops the
        // NPC at the range boundary. An exact comparison can fail because
        // of small float differences.
        if (distance > contactRange + 0.25f || currentTime < brain.NextFireAt)
        {
            return;
        }

        thumper.Damage(ThumperDamagePerHit);
        brain.NextFireAt = currentTime + ThumperHitCooldownMs;

        // Send a visual projectile, so that the player can see the attack
        if (brain.Profile.HasWeapon)
        {
            var aim = ComputeAttackAim(npc, thumper.Position);
            if (aim != Vector3.Zero)
            {
                npc.SetAimDirection(aim);
                AnnounceBurst(npc, aim);
                _shard.WeaponSim.OnFireWeaponProjectile(npc, _shard.CurrentTime, aim);
            }
        }
    }

    /// <summary>
    ///     Fires the equipped weapon at a character target. Conditions: the
    ///     NPC has a weapon, the cooldown is complete, the target is inside
    ///     FireRange, and an aim solution exists. A melee weapon does not
    ///     fire a projectile. It applies its damage directly.
    /// </summary>
    private void TryFire(CharacterEntity npc, BrainState brain, CharacterEntity target, float distance, ulong currentTime)
    {
        if (!brain.Profile.HasWeapon || distance > brain.Profile.FireRange || currentTime < brain.NextFireAt)
        {
            return;
        }

        // Aim at the torso of the target, not at its feet
        var aim = ComputeAttackAim(npc, target.Position);
        if (aim == Vector3.Zero)
        {
            return;
        }

        npc.SetAimDirection(aim);
        AnnounceBurst(npc, aim);

        // A melee strike applies its damage directly. The invisible contact
        // projectile is not reliable at bite range. Its ray can start
        // inside the hitbox of the target and then hits nothing. Ranged
        // weapons use the simulated projectile.
        // PLACEHOLDER: the damage value is fixed. Refer to
        // codex/damage-design.md phase D1 for real weapon damage.
        if (brain.Profile.PreferredRange <= 4f)
        {
            _shard.Damage.ApplyDamage(target, 1337, npc);
        }
        else
        {
            _shard.WeaponSim.OnFireWeaponProjectile(npc, _shard.CurrentTime, aim);
        }

        brain.NextFireAt = currentTime + brain.Profile.FireCooldownMs;
    }

    /// <summary>
    ///     Computes the aim direction from the muzzle of the NPC to the
    ///     torso of the target. For ammunition with gravity, the method
    ///     computes a ballistic launch angle. It uses the low arc. When the
    ///     target is out of ballistic range, it uses 45 degrees. For
    ///     ammunition without gravity, it returns the direct line.
    ///     A return value of zero means: do not fire.
    /// </summary>
    private Vector3 ComputeAttackAim(CharacterEntity npc, Vector3 targetPosition)
    {
        // Start the solution at the true muzzle height (the top of the rig
        // capsule). A different start height makes the arc miss the target.
        var origin = npc.Position + new Vector3(0f, 0f, npc.GetNpcMuzzleHeight());

        // Aim at the torso. When the target stands above the NPC, aim at
        // the head. The line to the torso of a target on a ledge touches
        // the edge of the ledge. The shot then hits the terrain at the
        // feet of the target.
        var aimHeight = targetPosition.Z - npc.Position.Z > 1.5f ? 1.7f : 1f;
        var target = targetPosition + new Vector3(0f, 0f, aimHeight);
        var delta = target - origin;
        var flat = SafeNormalize(delta);

        var details = npc.GetActiveWeaponDetails();
        if (details?.Weapon == null)
        {
            return flat;
        }

        var ammo = SDBInterface.GetAmmo(details.Weapon.AmmoId);
        if (ammo == null || ammo.Gravity <= 0.01f || ammo.ProjectileSpeed <= 0.1f)
        {
            return flat;
        }

        // Only ammunition with the Parabolic simulation mode flies under
        // gravity. Linear ammunition flies straight. Its gravity column has
        // no effect. A ballistic solution for linear ammunition would fire
        // the shot too high.
        if (new Enums.AmmoFlags(ammo.Flags).Simulation != Enums.AmmoFlags.SimulationMode.Parabolic)
        {
            return flat;
        }

        var horizontal = MathF.Sqrt((delta.X * delta.X) + (delta.Y * delta.Y));
        if (horizontal < 0.5f)
        {
            return flat;
        }

        // A test value from live reviews. It adds an up angle to ballistic aims.
        const float AimPitchUpDeg = 0f;

        Vector3 aim;
        var g = ammo.Gravity;
        var v2 = ammo.ProjectileSpeed * ammo.ProjectileSpeed;
        var discriminant = (v2 * v2) - (g * ((g * horizontal * horizontal) + (2f * delta.Z * v2)));
        if (discriminant <= 0f)
        {
            if (delta.Z > 2f)
            {
                // The target is higher than the ballistic reach. A maximum
                // shot would only hit the cliff face. Do not fire. The
                // movement code moves the NPC nearer. A shorter horizontal
                // distance increases the height that the weapon can reach.
                return Vector3.Zero;
            }

            // The target is out of ballistic range. An angle of 45 degrees
            // gives the maximum reach.
            aim = SafeNormalize(new Vector3(delta.X, delta.Y, horizontal));
        }
        else
        {
            // Artillery ammunition (a large splash radius) uses the high
            // arc. The shot goes up and comes down near the target. The
            // high arc is also correct when the target is far above the
            // NPC. The low arc would touch the cliff edge or hit the cliff
            // face. All other ammunition uses the low arc at level targets.
            var isArtillery = ammo.ImpactRadius >= 3f || delta.Z > 4f;
            var tanTheta = isArtillery
                ? (v2 + MathF.Sqrt(discriminant)) / (g * horizontal)
                : (v2 - MathF.Sqrt(discriminant)) / (g * horizontal);
            aim = SafeNormalize(new Vector3(delta.X / horizontal, delta.Y / horizontal, tanTheta));

            // An NPC behind a terrain edge fires the low arc into the
            // ground at its feet. Examine the first meters of the shot
            // line. If the line is blocked, use the high arc.
            if (!isArtillery && IsMuzzleBlocked(npc, origin, aim))
            {
                var highTan = (v2 + MathF.Sqrt(discriminant)) / (g * horizontal);
                aim = SafeNormalize(new Vector3(delta.X / horizontal, delta.Y / horizontal, highTan));
            }
        }

        // Turn the aim up by AimPitchUpDeg. Limit the result to 85 degrees.
        var horizLen = MathF.Sqrt((aim.X * aim.X) + (aim.Y * aim.Y));
        if (horizLen > 0.001f)
        {
            var pitch = MathF.Atan2(aim.Z, horizLen) + (AimPitchUpDeg * MathF.PI / 180f);
            pitch = MathF.Min(pitch, 85f * MathF.PI / 180f);
            aim = SafeNormalize(new Vector3(aim.X / horizLen * MathF.Cos(pitch), aim.Y / horizLen * MathF.Cos(pitch), MathF.Sin(pitch)));
        }

        return aim;
    }

    /// <summary>Casts a short ray along the aim line. Returns true when terrain or an obstacle blocks the muzzle.</summary>
    private bool IsMuzzleBlocked(CharacterEntity npc, Vector3 origin, Vector3 aim)
    {
        var hit = _shard.Physics.SegmentRayCast(origin, origin + (aim * 6f), npc.EntityId);
        return hit.Hit;
    }

    // Sends the shot to the clients in scope. WeaponSim only feeds the
    // server-side projectile simulation, which is not visible. The
    // WeaponProjectileFired message uses the equipped weapon of the client
    // and stays invisible for NPCs. Because of this, send the
    // AbilityProjectileFired message. It names the ammunition directly and
    // does not depend on client equipment data.
    private void AnnounceBurst(CharacterEntity npc, Vector3 aim)
    {
        var time = (uint)_shard.CurrentTime;
        npc.SetFireBurst(time);
        npc.SetFireEnd(time + 1);

        var details = npc.GetActiveWeaponDetails();
        if (details?.Weapon == null)
        {
            return;
        }

        var range = details.Weapon.Range;
        if (details.Attributes.TryGetValue((ushort)Enums.ItemAttributeId.WeaponRange, out var rangeAttr))
        {
            range = rangeAttr;
        }

        // The client applies the offset field of the packet in WORLD space
        // (found in live tests). Turn the local offset (right, forward, up)
        // to the direction of the shooter for each shot. The visual start
        // point then follows the head of the model.
        var flatAim = new Vector3(aim.X, aim.Y, 0f);
        var flatLen = flatAim.Length();
        var fxForward = flatLen > 0.001f ? flatAim / flatLen : Vector3.UnitY;
        var fxRight = new Vector3(fxForward.Y, -fxForward.X, 0f);

        // The offset values come from tests with the base aranha
        // (scale 1.3). Scale the offset with the rig. A large creature then
        // spits from its mouth, not from its legs.
        var fxScale = (npc.Collision != null ? Math.Clamp(npc.Collision.Scale, 0.5f, 5f) : 1.3f) / 1.3f;
        var fxOffset = ((fxRight * NpcFxOffset.X) + (fxForward * NpcFxOffset.Y) + new Vector3(0f, 0f, NpcFxOffset.Z)) * fxScale;

        var fired = new AbilityProjectileFired
        {
            ShortTime = (ushort)time,
            MaybeHalfs = fxOffset,
            Aim = aim,
            AmmoType = (ushort)details.Weapon.AmmoId,
            Range = range,
            Unk1 = 0,
            Unk2 = 1,
            Unk3 = 0,
            Unk4 = 0,
            Unk5 = 0,
            Hardpoint = NpcFxHardpoint,
            UnkFlag = 0,
        };
        _shard.EntityMan.SendToScoped(npc, fired);
    }

    /// <summary>
    ///     Derives the behavior profile for a character type and caches it.
    ///     All data comes from the SDB. There are no configuration files
    ///     for each monster. The monster row gives the speeds and the
    ///     faction. The rig name gives the movement class. The weapon
    ///     ranges give the combat style. The behavior strings give extra
    ///     tunable values.
    /// </summary>
    private AiProfile GetProfile(CharacterEntity npc)
    {
        var typeId = npc.StaticInfo.CharacterTypeId;
        return _profiles.GetOrAdd(typeId, id =>
        {
            var profile = new AiProfile
            {
                Aggressive = false,
                AggroRange = DefaultAggroRange,
                NormalSpeed = DefaultNormalSpeed,
                FastSpeed = DefaultFastSpeed,
                HasWeapon = false,
                FireCooldownMs = DefaultTriggerPullMs + DefaultFireRestMs,
                PreferredRange = RangedHoldRange,
                FireRange = 40f,
            };

            var monster = id != 0 ? SDBInterface.GetMonster(id) : null;
            if (monster == null)
            {
                return profile;
            }

            profile.Aggressive = IsHostileTowardPlayers(monster.FactionId);
            profile.NormalSpeed = ClampSpeed(monster.NormalSpeed, DefaultNormalSpeed);
            profile.FastSpeed = ClampSpeed(monster.FastSpeed, DefaultFastSpeed);
            profile.HasWeapon = monster.Weapon1Id != 0 || monster.Weapon2Id != 0;

            // The rig name in CharInfo gives the movement class. The SDB
            // has no flag column for this. Examples of flyer rig names:
            // "Mosquito", "Drone", "Flying Roach", "Hovering", "UAV".
            // Turret rigs and tower rigs never move.
            var rigName = (monster.CharinfoId != 0 ? SDBInterface.GetCharInfo(monster.CharinfoId)?.Name : null) ?? string.Empty;
            profile.Flying = ContainsAny(rigName, "drone", "mosquito", "flying", "hovering", "uav");
            profile.Stationary = ContainsAny(rigName, "turret", "tower", "thumper", "spawner", "tentacle", "inhibitor");
            profile.CanClimb = ContainsAny(rigName, "climber", "aranha", "spider")
                && !rigName.Contains("Non Climbing", StringComparison.OrdinalIgnoreCase);

            // The combat style sets the hold distance. The reach comes from
            // the real weapon data. Example: the aranha bite is a weapon
            // with a 3.12 m range. An aranha that holds a ranged distance
            // can never attack. The behavior tokens cover monsters that
            // have no weapon rows. 45 monster types have a melee weapon and
            // a ranged weapon. Classify these by the longer weapon. They
            // then fight at range. Weapon change at short range is a
            // Phase B feature. HasMeleeBackup marks these monsters.
            var range1 = monster.Weapon1Id != 0 ? SDBUtils.GetDetailedWeaponInfo(monster.Weapon1Id)?.Main?.Range ?? 0f : 0f;
            var range2 = monster.Weapon2Id != 0 ? SDBUtils.GetDetailedWeaponInfo(monster.Weapon2Id)?.Main?.Range ?? 0f : 0f;
            var weaponRange = Math.Max(range1, range2);
            var shortest = Math.Min(range1 > 0f ? range1 : float.MaxValue, range2 > 0f ? range2 : float.MaxValue);
            profile.HasMeleeBackup = shortest <= 6f && weaponRange > 6f;

            // A new NPC has weapon slot 1 equipped. When slot 2 has the
            // longer weapon, the brain changes to slot 2 on its first think.
            profile.PreferredWeaponIndex = (byte)(range2 > range1 && range2 > 0f ? 2 : 0);

            var archetypes = $"{monster.Behavior} {monster.BehaviorOffensive} {monster.BehaviorDefensive}";
            var isMelee = (weaponRange > 0f && weaponRange <= 6f)
                || ContainsAny(archetypes, "melee", "kamikaze", "charger", "whipper", "brute", "rageclaw", "scorcher")
                || (!profile.HasWeapon && !ContainsAny(archetypes, "ranged", "sniper", "movethenfire"));
            if (isMelee)
            {
                // Stop outside the body of the target, but inside the attack reach
                profile.PreferredRange = weaponRange > 0f
                    ? Math.Clamp(weaponRange - 0.5f, 1.5f, 3f)
                    : MeleeRange;
                profile.FireRange = weaponRange > 0f ? weaponRange + 0.25f : 4f;
            }
            else
            {
                profile.PreferredRange = ContainsAny(archetypes, "sniper") ? SniperHoldRange : RangedHoldRange;
                if (weaponRange > 0f && weaponRange < profile.PreferredRange)
                {
                    profile.PreferredRange = Math.Max(4f, weaponRange * 0.8f);
                }

                profile.FireRange = weaponRange > 0f
                    ? Math.Min(weaponRange, profile.PreferredRange + 22f)
                    : profile.PreferredRange + 22f;

                // An artillery type (ammunition with a large splash radius)
                // holds a position near its maximum weapon range. The
                // default hold distance is too near. At 18 m, the splash
                // damage of the NPC can hit the NPC itself.
                var classifiedWeapon = range2 > range1 ? monster.Weapon2Id : monster.Weapon1Id;
                var mainTemplate = classifiedWeapon != 0 ? SDBUtils.GetDetailedWeaponInfo(classifiedWeapon)?.Main : null;
                var ammoRecord = mainTemplate != null ? SDBInterface.GetAmmo(mainTemplate.AmmoId) : null;
                if (ammoRecord is { ImpactRadius: >= 3f } && weaponRange > 8f)
                {
                    profile.PreferredRange = Math.Max(profile.PreferredRange, weaponRange - 4f);
                    profile.FireRange = weaponRange;
                }
            }

            ApplyBehaviorTunables(profile, monster);
            return profile;
        });
    }

    /// <summary>Applies the key=value tunables from the behavior strings of the monster. Example: fire timing values.</summary>
    private void ApplyBehaviorTunables(AiProfile profile, Monster monster)
    {
        uint triggerPull = DefaultTriggerPullMs;
        uint fireRest = DefaultFireRestMs;

        foreach (var behavior in new[] { monster.Behavior, monster.BehaviorOffensive, monster.BehaviorDefensive })
        {
            if (string.IsNullOrWhiteSpace(behavior))
            {
                continue;
            }

            foreach (Match match in BehaviorParamRegex.Matches(behavior))
            {
                var key = match.Groups["key"].Value;
                if (!float.TryParse(match.Groups["value"].Value, System.Globalization.CultureInfo.InvariantCulture, out var value))
                {
                    continue;
                }

                switch (key)
                {
                    case "targetSelectDist":
                        profile.AggroRange = Math.Clamp(value, 5f, 150f);
                        break;
                    case "triggerPullTime":
                        triggerPull = (uint)Math.Clamp(value, 100f, 10000f);
                        break;
                    case "fireRestDuration":
                        fireRest = (uint)Math.Clamp(value, 0f, 10000f);
                        break;
                }
            }
        }

        profile.FireCooldownMs = triggerPull + fireRest;
    }

    /// <summary>Reads the faction stance from the SDB faction matrix. A negative stance means hostile. The matrix loads on first use.</summary>
    private bool IsHostileTowardFaction(uint factionA, uint factionB)
    {
        EnsureFactionData();

        if (factionA == 0 || factionA == factionB)
        {
            return false;
        }

        if (_factionStance.TryGetValue((factionA, factionB), out var stance))
        {
            return stance < 0;
        }

        return _factionDefaultStance.GetValueOrDefault(factionA, (sbyte)0) < 0;
    }

    private uint GetNpcFactionId(CharacterEntity npc)
    {
        return npc.HostilityInfo.Flags.HasFlag(AeroMessages.GSS.HostilityInfoData.HostilityFlags.Faction)
            ? npc.HostilityInfo.FactionId
            : 0u;
    }

    private void EnsureFactionData()
    {
        if (_factionStance != null)
        {
            return;
        }

        var stance = new Dictionary<(uint, uint), sbyte>();
        foreach (var relation in SDBInterface.GetFactionRelations())
        {
            stance[(relation.FactionA, relation.FactionB)] = relation.HostilityStance;
            if (relation.HostilityBidirectional != 0)
            {
                stance[(relation.FactionB, relation.FactionA)] = relation.HostilityStance;
            }
        }

        _factionDefaultStance = SDBInterface.GetFactions().ToDictionary(f => f.Id, f => f.DefaultStance);
        _factionStance = stance;
    }

    /// <summary>Sends the pose to the clients, at most one time for each think interval. Network broadcasts are costly.</summary>
    private void BroadcastPoseIfDue(CharacterEntity npc, BrainState brain, ulong currentTime)
    {
        if (currentTime >= brain.NextPoseBroadcastAt)
        {
            _shard.Movement.BroadcastCharacterPose(npc);
            brain.NextPoseBroadcastAt = currentTime + ThinkIntervalMs;
        }
    }

    private CharacterEntity FindPlayerById(ulong entityId)
    {
        foreach (var client in _shard.Clients.Values)
        {
            var candidate = client.CharacterEntity;
            if (candidate != null
                && candidate.EntityId == entityId
                && client.Status == IPlayer.PlayerStatus.Playing
                && IsLiving(candidate))
            {
                return candidate;
            }
        }

        return null;
    }

    private CharacterEntity FindNearestPlayer(CharacterEntity npc, float range, Vector3? center = null)
    {
        CharacterEntity nearest = null;
        var nearestDistanceSq = range * range;
        var origin = center ?? npc.Position;

        foreach (var client in _shard.Clients.Values)
        {
            var candidate = client.CharacterEntity;
            if (client.Status != IPlayer.PlayerStatus.Playing || candidate == null || !IsLiving(candidate))
            {
                continue;
            }

            var distanceSq = Vector3.DistanceSquared(origin, candidate.Position);
            if (distanceSq < nearestDistanceSq)
            {
                nearest = candidate;
                nearestDistanceSq = distanceSq;
            }
        }

        return nearest;
    }

    private bool IsLiving(CharacterEntity character)
    {
        return character.CharacterState.State == CharacterStateData.CharacterStatus.Living;
    }

    private Vector3 SafeNormalize(Vector3 value)
    {
        return value.LengthSquared() > 0.0001f ? Vector3.Normalize(value) : Vector3.Zero;
    }

    private float ClampSpeed(float value, float fallback)
    {
        return value > 0.1f ? Math.Clamp(value, 1f, 15f) : fallback;
    }

    /// <summary>Writes the aim, the orientation, the movement state, and the position (optional) to the replicated character.</summary>
    private void SetMovement(CharacterEntity character, Vector3 aimDirection, short movementState, Vector3? position = null)
    {
        if (aimDirection != Vector3.Zero)
        {
            character.SetAimDirection(aimDirection);

            // The replicated orientation is the world-to-local rotation.
            // The local +Y axis is the forward direction. Refer to
            // CalculateProjectileOrigin: local offsets go to world space
            // through the INVERSE quaternion, and the muzzle offset is
            // local (right, forward, up). The yaw angle is measured from
            // +Y. The wire format wants the inverse rotation. These two
            // facts together give Atan2(X, Y). The earlier Atan2(Y, X)
            // turned NPCs to the side or to the back.
            character.SetOrientation(Quaternion.CreateFromAxisAngle(Vector3.UnitZ, MathF.Atan2(aimDirection.X, aimDirection.Y)));
        }

        character.MovementStateContainer.MovementStateValue = (ushort)movementState;
        character.MovementState = movementState;
        character.MovementShortTime = character.Shard.CurrentShortTime;

        if (position.HasValue)
        {
            character.SetPosition(position.Value);
        }
    }

    /// <summary>
    ///     The mutable state of one brain. It is created on the first think
    ///     of the entity. It is removed when the entity dies or despawns.
    ///     The leap and perch fields belong to the leap attacks and the
    ///     hull climbing in AIEngine.Aranha.cs.
    /// </summary>
    private class BrainState
    {
        public Vector3 Home { get; set; }                       // The spawn position. The leash and the return use it.
        public AiProfile Profile { get; set; }                  // The shared profile of the type. Do not change it.
        public BrainMode Mode { get; set; }
        public ulong TargetEntityId { get; set; }
        public ulong TargetLastSeenAt { get; set; }             // The time of the last target contact. TargetMemoryMs uses it.
        public ulong NextFireAt { get; set; }
        public ulong NextPoseBroadcastAt { get; set; }          // The time of the next permitted pose broadcast.
        public bool WeaponAligned { get; set; }                 // True after the one-time weapon-slot change on the first think.
        public float BodyRadius { get; set; } = 0.5f;           // The radius of the separation disc. ApplySeparation uses it.

        // An active leap (a pounce at a player, or a leap onto a hull)
        public Vector3 LeapStart { get; set; }
        public Vector3 LeapTarget { get; set; }
        public ulong LeapWalkableId { get; set; }               // The entity that is the landing surface. Zero means the ground.
        public ulong LeapStartedAt { get; set; }
        public ulong LeapEndsAt { get; set; }                   // While this is in the future, only TickLeap controls the NPC.
        public ulong NextPounceAt { get; set; }                 // The time when the next leap is permitted.

        // The NPC holds a perch point on a climbable hull
        public bool HasPerch { get; set; }
        public Vector3 PerchOffset { get; set; }                // The perch position, relative to the hull entity.
    }

    /// <summary>
    ///     The behavior profile of one character type. GetProfile derives
    ///     it from SDB data one time and caches it. All instances of the
    ///     type share one profile object. Do not change a profile after
    ///     its creation.
    /// </summary>
    private class AiProfile
    {
        public bool Aggressive { get; set; }                    // True when the type is hostile to players.
        public float AggroRange { get; set; }
        public float NormalSpeed { get; set; }                  // The walk speed in m/s.
        public float FastSpeed { get; set; }                    // The chase speed in m/s.
        public bool HasWeapon { get; set; }
        public uint FireCooldownMs { get; set; }
        public bool Flying { get; set; }                        // Movement class: the NPC flies in 3D to a hover point.
        public bool Stationary { get; set; }                    // Movement class: the NPC aims but does not move.
        public bool CanClimb { get; set; }                      // Movement class: hulls are walkable surfaces for the NPC.
        public float PreferredRange { get; set; } = RangedHoldRange;  // The hold distance: melee 2, ranged 18, sniper 38.
        public float FireRange { get; set; } = 40f;             // The maximum firing distance.
        public bool HasMeleeBackup { get; set; }                // True for types with a melee weapon and a ranged weapon.
        public byte PreferredWeaponIndex { get; set; }          // The weapon slot that sets the combat style.
    }
}

