#nullable enable
using System;
using System.Collections.Generic;
using System.Numerics;
using DotRecast.Detour.Crowd;
using GameServer.Entities.Character;
using GameServer.Systems.Ai.Behavior;

namespace GameServer.Systems.Ai;

/// <summary>Precomputed NPC movement speeds (see <see cref="NpcSpeedCalculator" />).</summary>
public readonly record struct NpcSpeeds(float Run, float Sprint, float Crouch);

/// <summary>
///     Per-NPC movement state, owned by a <see cref="CharacterEntity" />.
///     Created on spawn for NPCs; the active <see cref="Strategy" /> is swapped in Phase 2 (Crowd) with no other changes.
/// </summary>
public sealed class NpcMovementComponent
{
    private float _stuckTime;
    private Vector3 _stuckReferencePosition;
    private bool _hasStuckReference;

    public NpcMovementComponent(CharacterEntity owner, INavigationStrategy strategy, NpcBehaviorConfig behavior, NpcSpeeds speeds)
    {
        Owner = owner;
        Strategy = strategy;
        Behavior = behavior;
        Speeds = speeds;
        Mode = NpcMovementMode.Idle;
        GroundNormal = Vector3.UnitZ;
    }

    public CharacterEntity Owner { get; }
    public INavigationStrategy Strategy { get; }

    /// <summary>
    ///     The NPC's agent in the shard's shared crowd simulation, assigned by <see cref="NpcMovementSystem" /> on
    ///     the tick it is created. Null while the NPC is steered by <see cref="Strategy" /> instead — no navmesh,
    ///     crowd disabled, or the goal could not be placed on the mesh.
    /// </summary>
    public DtCrowdAgent? CrowdAgent { get; set; }

    public NpcBehaviorConfig Behavior { get; }
    public NpcSpeeds Speeds { get; }
    public Vector3 Destination { get; private set; }

    /// <summary>
    ///     The goal the navigation layer was last actually given, as opposed to <see cref="Destination" /> which
    ///     behaviors keep re-anchoring. Arrival must be judged against this: a destination recomputed relative to
    ///     the NPC's own position each tick — combat's back-off always points <c>CombatDist</c> away from where the
    ///     NPC now stands — is by construction unreachable, so checking against it means the NPC never arrives and
    ///     retreats forever. Navigation refreshes this at repath cadence: the fixed point the route was built to.
    /// </summary>
    public Vector3 RequestedGoal { get; private set; }

    public bool HasRequestedGoal { get; private set; }

    public bool IsMoving { get; private set; }
    public NpcMovementMode Mode { get; private set; }
    public float RepathTimer { get; private set; }
    public bool Grounded { get; private set; } = true;
    public Vector3 GroundNormal { get; private set; }
    public float BrakeTimer { get; private set; }

    // Follow mode: the NPC continuously trails a target entity, closing the gap until it is within
    // LeashRadius and holding there until the target moves out of range again. TargetId == 0 means no target.
    public ulong TargetId { get; private set; }
    public float LeashRadius { get; private set; }
    public bool HasTarget => TargetId != 0;

    // Patrol mode: the NPC walks a fixed route of waypoints in sequence (cyclically). The route is retained across
    // higher-priority modes (follow / manual move), so clearing those resumes patrol from PatrolIndex. PatrolIndex
    // is the waypoint the NPC is currently heading to; the movement system advances it on arrival and re-picks the
    // nearest waypoint when the NPC is far off the route (e.g. after an aggro interruption).
    public IReadOnlyList<Vector3>? PatrolRoute { get; private set; }
    public int PatrolIndex { get; private set; }
    public bool HasPatrol => PatrolRoute is { Count: > 0 };

    // Wander mode: the NPC roams within a circle (WanderCenter / WanderRadius), picking a new navmesh-accessible
    // point after each idle pause. WanderPauseTimer counts down only while the NPC is stopped, giving a random
    // dwell between legs. WanderRadius <= 0 means wander is off.
    public Vector3 WanderCenter { get; private set; }
    public float WanderRadius { get; private set; }
    public float WanderPauseTimer { get; private set; }
    public bool HasWander => WanderRadius > 0f;

    // Combat mode: the NPC holds a standoff distance from a combat target. The outer behavior system decides aggro and
    // sets this goal; the movement system steers to keep the horizontal gap at CombatDist (approach if farther, back off
    // under CombatMinDist, hold otherwise). CombatTargetId == 0 means not in combat.
    public ulong CombatTargetId { get; private set; }
    public float CombatDist { get; private set; }
    public float CombatMinDist { get; private set; }
    public bool HasCombat => CombatTargetId != 0;

    // Combat maneuvering. CombatHasLos is set by the behavior system each tick (whether the target is visible) and drives
    // strafe-vs-reposition. CombatFacing is the horizontal body-yaw direction the NPC faces while fighting; the movement
    // system re-points it at the target's live position every tick, so the body keeps turning to track the target even
    // when it moves out of line of sight. CombatStrafeSide / CombatManeuverTimer drive the strafe/juke leg cadence.
    public bool CombatHasLos { get; private set; }
    public Vector3 CombatFacing { get; private set; }
    public bool HasCombatFacing { get; private set; }
    public int CombatStrafeSide { get; private set; } = 1;
    public float CombatManeuverTimer { get; private set; }

    public int RepathAttempts { get; private set; }
    public float StepUpAccumulator { get; private set; }

    // Sprint/crouch are behavior-driven movement-mode toggles (the Phase 4 behavior state machine sets these; they default
    // to sprint so the NPC keeps its current fast-speed behavior until the behavior opts out). They drive both the target
    // speed and the Sprint/Crouch movement flags broadcast to the client.
    public bool Sprint { get; set; } = true;
    public bool Crouch { get; set; }

    // Walk gait toggle: when set, the NPC moves at the base Normal speed and broadcasts the Walking gait (no Sprint
    // flag). Patrol/wander set this; follow/combat and manual moves leave it off.
    public bool Walk { get; set; }

    /// <summary>Speed the NPC should move at, given its current gait (walk uses the base Normal speed).</summary>
    public float TargetSpeed => Crouch ? Speeds.Crouch : (Sprint && !Walk) ? Speeds.Sprint : Speeds.Run;

    // --- Launch-investigation diagnostics (temporary) ---
    // Tracks the last assist action and the real body Z-velocity so a launch can be attributed to a cause.
    public float LastBodyVelZ { get; private set; }
    public string LastAssist { get; private set; } = "none";
    public float LastAssistValue { get; private set; }
    public int LastAssistTicksAgo { get; private set; } = int.MaxValue;
    public float LaunchLogCooldown { get; private set; }

    public void AccumulateStepUp(float amount)
    {
        StepUpAccumulator += amount;
    }

    public void ResetStepUp()
    {
        StepUpAccumulator = 0f;
    }

    public void RecordAssist(string kind, float value)
    {
        LastAssist = kind;
        LastAssistValue = value;
        LastAssistTicksAgo = 0;
    }

    public void AgeAssist()
    {
        if (LastAssistTicksAgo < int.MaxValue)
        {
            LastAssistTicksAgo++;
        }
    }

    public void NoteBodyVelZ(float z)
    {
        LastBodyVelZ = z;
    }

    public void DecrementLaunchLogCooldown(float dt)
    {
        LaunchLogCooldown = MathF.Max(0f, LaunchLogCooldown - dt);
    }

    public void TriggerLaunchLogCooldown(float cooldown)
    {
        LaunchLogCooldown = cooldown;
    }

    public void SetDestination(Vector3 destination)
    {
        Destination = destination;
        Strategy.SetDestination(destination);
        Mode = NpcMovementMode.NavMesh;
        IsMoving = true;
        RepathTimer = 0f;
        RepathAttempts = 0;
        ResetStuckTracking();
    }

    /// <summary>
    ///     Assigns a follow target. The movement system re-evaluates the target each tick: it chases while the
    ///     target is farther than <paramref name="leashRadius" /> (horizontal) and holds once inside it.
    /// </summary>
    /// <param name="targetId">The entity id to follow.</param>
    /// <param name="leashRadius">The horizontal distance at which the NPC stops and holds.</param>
    public void SetTarget(ulong targetId, float leashRadius)
    {
        TargetId = targetId;
        LeashRadius = leashRadius;
        Mode = NpcMovementMode.Follow;
    }

    /// <summary>Drops the follow target. The caller is responsible for braking the body.</summary>
    public void ClearTarget()
    {
        TargetId = 0;
        LeashRadius = 0f;
        Mode = NpcMovementMode.Idle;
    }

    /// <summary>
    ///     Assigns a patrol route. The NPC walks the waypoints in sequence (cyclically); the movement system picks the
    ///     starting waypoint by proximity and advances on arrival. The route is retained until <see cref="ClearPatrol" />,
    ///     so a higher-priority mode that interrupts patrol (follow / manual move) resumes it afterwards.
    /// </summary>
    /// <param name="route">The waypoints to walk cyclically.</param>
    public void SetPatrol(IReadOnlyList<Vector3> route)
    {
        PatrolRoute = route;
        PatrolIndex = 0;
        Mode = NpcMovementMode.Patrol;
    }

    /// <summary>Drops the patrol route. The caller is responsible for braking the body.</summary>
    public void ClearPatrol()
    {
        PatrolRoute = null;
        PatrolIndex = 0;
    }

    /// <summary>
    ///     Enables wander mode: the NPC roams within <paramref name="radius" /> of <paramref name="center" />,
    ///     periodically picking a new navmesh-accessible point. A radius of 0 disables it.
    /// </summary>
    /// <param name="center">The wander area center.</param>
    /// <param name="radius">The wander area radius (0 disables wander).</param>
    public void SetWander(Vector3 center, float radius)
    {
        WanderCenter = center;
        WanderRadius = radius;
        WanderPauseTimer = 0f;
        Mode = NpcMovementMode.Wander;
    }

    /// <summary>Disables wander mode. The caller is responsible for braking the body.</summary>
    public void ClearWander()
    {
        WanderRadius = 0f;
        WanderPauseTimer = 0f;
    }

    /// <summary>
    ///     Puts the NPC into combat mode against <paramref name="targetId" />, holding a horizontal standoff of
    ///     <paramref name="combatDist" /> (and backing off if it closes under <paramref name="minDist" />). The outer
    ///     behavior system re-asserts this each tick while the target is aggroed.
    /// </summary>
    /// <param name="targetId">The combat target's entity id.</param>
    /// <param name="combatDist">The horizontal standoff distance to hold.</param>
    /// <param name="minDist">The minimum distance; the NPC backs off if it closes under this.</param>
    public void SetCombat(ulong targetId, float combatDist, float minDist)
    {
        CombatTargetId = targetId;
        CombatDist = combatDist;
        CombatMinDist = minDist;
        Mode = NpcMovementMode.Combat;
    }

    /// <summary>Drops the combat target. The caller is responsible for braking the body.</summary>
    public void ClearCombat()
    {
        CombatTargetId = 0;
        CombatDist = 0f;
        CombatMinDist = 0f;
        CombatHasLos = false;
        HasCombatFacing = false;
        CombatFacing = Vector3.Zero;
        CombatStrafeSide = 1;
        CombatManeuverTimer = 0f;
    }

    /// <summary>Records whether the combat target is currently visible (set by the behavior system each tick).</summary>
    /// <param name="hasLos">True when line of sight to the combat target is currently clear.</param>
    public void SetCombatLos(bool hasLos)
    {
        CombatHasLos = hasLos;
    }

    /// <summary>
    ///     Sets the horizontal body-yaw direction the NPC faces while fighting. Re-pointed at the target's live position
    ///     every tick, so the body keeps tracking the target even when it moves out of line of sight.
    /// </summary>
    /// <param name="facing">The horizontal body-yaw direction to face while fighting.</param>
    public void SetCombatFacing(Vector3 facing)
    {
        CombatFacing = facing;
        HasCombatFacing = true;
    }

    public void SetCombatStrafeSide(int side)
    {
        CombatStrafeSide = side;
    }

    public void DecrementCombatManeuverTimer(float dt)
    {
        CombatManeuverTimer = MathF.Max(0f, CombatManeuverTimer - dt);
    }

    public void ResetCombatManeuverTimer(float value)
    {
        CombatManeuverTimer = value;
    }

    /// <summary>Sets the current patrol waypoint index (the waypoint the NPC is heading to).</summary>
    /// <param name="index">The index into the patrol route to head toward.</param>
    public void SetPatrolIndex(int index)
    {
        PatrolIndex = index;
    }

    public void DecrementWanderPauseTimer(float dt)
    {
        WanderPauseTimer = MathF.Max(0f, WanderPauseTimer - dt);
    }

    public void ResetWanderPauseTimer(float value)
    {
        WanderPauseTimer = value;
    }

    /// <summary>
    ///     Refreshes the stored destination (used to keep a follow chase pointed at the target's live position)
    ///     without forcing an immediate re-path; the periodic repath timer picks the new value up.
    /// </summary>
    /// <param name="destination">The new destination to store for the next repath.</param>
    public void UpdateDestination(Vector3 destination)
    {
        Destination = destination;
    }

    /// <summary>
    ///     Records the goal navigation was just handed, the point arrival is judged against. See
    ///     <see cref="RequestedGoal" />.
    /// </summary>
    public void NoteRequestedGoal(Vector3 goal)
    {
        RequestedGoal = goal;
        HasRequestedGoal = true;
    }

    public void Stop()
    {
        Strategy.Reset();
        IsMoving = false;
        Mode = NpcMovementMode.Idle;
        BrakeTimer = 1.0f;
        RepathAttempts = 0;
        HasRequestedGoal = false;
        ResetStuckTracking();
    }

    public void DecrementRepathTimer(float dt)
    {
        RepathTimer = MathF.Max(0f, RepathTimer - dt);
    }

    public void ResetRepathTimer(float value)
    {
        RepathTimer = value;
    }

    public void DecrementBrakeTimer(float dt)
    {
        BrakeTimer = MathF.Max(0f, BrakeTimer - dt);
    }

    public void UpdateGrounded(bool grounded, Vector3 normal)
    {
        Grounded = grounded;
        if (grounded && normal.LengthSquared() > 0.01f)
        {
            GroundNormal = Vector3.Normalize(normal);
        }
        else if (!grounded)
        {
            GroundNormal = Vector3.UnitZ;
        }
    }

    public void RegisterRepath()
    {
        RepathAttempts++;
    }

    public void BeginStuckTracking(Vector3 position)
    {
        _hasStuckReference = true;
        _stuckReferencePosition = position;
        _stuckTime = 0f;
    }

    public bool UpdateStuckTracking(Vector3 position, float dt, float minDisplacement, float stuckDuration)
    {
        if (!_hasStuckReference)
        {
            BeginStuckTracking(position);
            return false;
        }

        // Agents are judged on horizontal (X-Y) progress (Firefall is Z-up; vertical motion is gravity/step-up driven).
        var current = new Vector3(position.X, position.Y, 0f);
        var reference = new Vector3(_stuckReferencePosition.X, _stuckReferencePosition.Y, 0f);
        if ((current - reference).Length() > MathF.Max(0.01f, minDisplacement))
        {
            _stuckReferencePosition = position;
            _stuckTime = 0f;
            return false;
        }

        _stuckTime += dt;
        if (_stuckTime < stuckDuration)
        {
            return false;
        }

        _stuckReferencePosition = position;
        _stuckTime = 0f;
        return true;
    }

    public void ResetStuckTracking()
    {
        _hasStuckReference = false;
        _stuckReferencePosition = Vector3.Zero;
        _stuckTime = 0f;
    }
}
