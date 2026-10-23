#nullable enable
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using DotRecast.Detour.Crowd;
using GameServer.Entities;
using GameServer.Entities.Character;
using GameServer.Entities.Deployable;
using GameServer.Entities.Vehicle;
using GameServer.Navigation;
using GameServer.Physics;
using GameServer.Systems.SystemEvents;
using Serilog;
using Serilog.Events;

namespace GameServer.Systems.Ai;

/// <summary>
///     Drives NPC steering each shard tick. Runs after the physics step so it can read the physics-synced
///     pose back from each dynamic body and publish a new desired velocity. The physics engine applies a bounded
///     impulse toward that velocity during the next fixed timestep, so gravity and external knockback impulses
///     are not overwritten.
/// </summary>
public sealed class NpcMovementSystem
{
    // Default leash radius for follow mode (world units, horizontal). Used when a caller omits an explicit radius.
    public const float FollowDefaultRadius = 10f;

    // Default wander radius when a caller omits one (world units, horizontal).
    public const float WanderDefaultRadius = 15f;

    private const float _repathInterval = 1.0f;
    private const float _airControlMultiplier = 0.25f;
    private const float _walkMinSpeed = 0.5f; // horizontal speed below which a grounded NPC reads Standing, at/above reads Walking
    private const float _timeToTopSpeed = 0.25f;
    private const float _brakeAccelerationMultiplier = 1.5f;
    private const float _groundProbeRange = 0.5f;
    private const float _minStepLift = 0.1f;

    // Biped traverse assist: small ledges are climbed; gaps are left to the navmesh, which routes around them.
    // Do not add a Z-velocity "hop" for stalled bipeds: when the ahead-ground probe misses, the hop re-fires as its
    // cooldown cycles and the Z-velocity additions compound, launching the biped ~30ft into the air.
    private const float _bipedMaxStepHeight = 0.8f;
    private const float _bipedStepProbeDistance = 0.7f;
    private const float _bipedStepProbeUp = 1.1f;
    private const float _bipedStepProbeDown = 0.6f;
    private const float _bipedStepClearance = 0.15f;

    // A forward surface must face up at least this much (dot with +Z) to count as a climbable top rather than a wall face.
    private const float _bipedStepMinSurfaceZ = 0.5f;

    // --- Launch watchdog diagnostics ---
    // Flag a biped rising faster than this, or whose per-tick Z-velocity changed by more than this.
    private const float _launchWatchVelZ = 5f;
    private const float _launchWatchVelDelta = 3f;
    private const float _launchLogCooldown = 0.25f;

    private const float _stuckDuration = 0.6f;
    private const float _stuckMinDisplacement = 0.15f;
    private const int _maxRepathAttempts = 3;

    // Horizontal distance at which a crowd-driven NPC counts as having reached its destination. Matches
    // NavMeshPathStrategy's arrival radius so both paths stop in the same place.
    private const float _arrivalRadius = 1.0f;

    // A goal already occupied by other agents cannot also hold this one — the crowd parks it just short and
    // stops. Within this distance, an NPC the crowd has settled counts as arrived. Sized to a couple of agent
    // diameters, which is where a queue at a point actually forms.
    private const float _crowdRestRadius = 2.0f;
    private const float _crowdRestSpeed = 0.1f;

    // Movement ticks between steering traces (20 Hz, so 20 ticks is one second per NPC).
    private const int _traceCadence = 20;

    // Local avoidance: how close (horizontal) a solid body (deployable, vehicle, player) must be before the NPC steers
    // around it. Small so the NPC only nudges around the body instead of taking a wide detour (the navmesh can't carve a
    // small hole around them on flat ground).
    private const float _obstacleAvoidanceRadius = 3f;

    // Patrol: horizontal distance at/under which the current waypoint counts as reached (advance to the next).
    private const float _patrolArriveRadius = 2f;

    // Patrol: how far a waypoint is projected onto the navmesh when the route is assigned. A waypoint off the mesh
    // would otherwise be unreachable (the path ends at the nearest on-mesh point but arrival is judged against the raw
    // coordinate), leaving the NPC pressed against the mesh edge and never advancing.
    private const float _patrolSnapRadius = 25f;

    // Wander: random dwell between legs (seconds), and the retry delay when no roam point could be picked.
    private const float _wanderPauseMin = 1.5f;
    private const float _wanderPauseMax = 4f;
    private const float _wanderRetryDelay = 1f;

    // Combat maneuvering: how far a strafe/juke leg steps sideways, the random dwell between legs, the navmesh snap
    // radius for the leg endpoint, and the minimum leg length below which that side is treated as blocked (hold instead
    // of strafing into a wall).
    private const float _strafeLegDistance = 3.5f;
    private const float _strafePauseMin = 2f;
    private const float _strafePauseMax = 4f;
    private const float _maneuverSnapRadius = 6f;
    private const float _strafeMinLeg = 1f;

    // Standoff band hysteresis: only re-approach once the target is this far beyond CombatDist, so a strafe leg that
    // nudges the gap slightly past CombatDist doesn't immediately trigger a full approach (which would drag the NPC in).
    private const float _combatApproachHysteresis = 1.5f;

    private static readonly float _bipedMinNormalZ = MathF.Cos(75f * MathF.PI / 180f);

    private readonly IShard _shard;
    private readonly ILogger _logger;
    private readonly Random _rand = new();

    private readonly List<MovingStep> _movingSteps = [];
    private readonly HashSet<ulong> _liveNpcIds = [];

    private double _accumulator;
    private int _traceTick;

    public NpcMovementSystem(IShard shard)
    {
        _shard = shard;
        _logger = Log.Logger.ForContext<NpcMovementSystem>();
    }

    public void Tick(double deltaTimeMs, ulong currentTime, CancellationToken ct)
    {
        // Gate the per-tick movement logic to the physics timestep rate.
        _accumulator += deltaTimeMs;
        if (_accumulator < PhysicsEngine.TimeStepMs)
        {
            return;
        }

        // Run one step; keep the remainder but cap it so a long stall can't cause a catch-up burst of steps.
        _accumulator = Math.Min(_accumulator - PhysicsEngine.TimeStepMs, PhysicsEngine.TimeStepMs);

        const float dt = PhysicsEngine.TimeStepSeconds;

        _traceTick++;
        _movingSteps.Clear();
        _liveNpcIds.Clear();

        // Phase 1: behaviour goals, ground probes, arrival, and (for non-crowd NPCs) steering. Crowd agents are
        // synced from the real body poses here so the shared simulation plans from where the NPCs actually are.
        foreach (var entity in _shard.Entities.Values)
        {
            if (entity is not CharacterEntity npc || npc.NpcMovement is not { } comp)
            {
                continue;
            }

            // A dead NPC (corpse lingering until removal) must stop moving.
            if (!npc.IsAlive)
            {
                if (comp.IsMoving || comp.HasCombat || comp.HasTarget || comp.HasPatrol || comp.HasWander)
                {
                    comp.ClearCombat();
                    comp.ClearTarget();
                    comp.ClearPatrol();
                    comp.ClearWander();
                    npc.Velocity = Vector3.Zero;
                    StopNpc(npc, comp);
                }

                ReleaseCrowdAgent(npc, comp);
                TickStoppedNpc(npc, comp, dt);
                continue;
            }

            _liveNpcIds.Add(npc.EntityId);
            comp.DecrementLaunchLogCooldown(dt);
            comp.AgeAssist();

            // Launch-investigation: sample the real body Z-velocity (before this tick's assist runs) and flag upward spikes.
            LogNpcLaunch(npc, comp);

            // Idle-behavior pre-pass (priority: combat > follow > patrol > wander). Each re-points/starts the destination or
            // brakes before the move/stop dispatch below. Higher-priority modes override lower ones, and because the
            // patrol route / wander area are retained on the component, clearing a higher mode resumes the idle one.
            // The gait follows the active behavior: patrol/wander walk, everything else uses the default (sprint) gait.
            if (comp.HasCombat)
            {
                comp.Walk = false;
                TickCombat(npc, comp, dt);
            }
            else if (comp.HasTarget)
            {
                comp.Walk = false;
                TickTargetFollow(npc, comp, dt);
            }
            else if (comp.HasPatrol)
            {
                comp.Walk = true;
                TickPatrol(npc, comp, dt);
            }
            else if (comp.HasWander)
            {
                comp.Walk = true;
                TickWander(npc, comp, dt);
            }
            else
            {
                comp.Walk = false;
            }

            SyncCrowdAgent(npc, comp);

            if (comp.IsMoving)
            {
                if (BeginMovingNpc(npc, comp, dt) is { } step)
                {
                    _movingSteps.Add(step);
                }
            }
            else
            {
                TickStoppedNpc(npc, comp, dt);
            }
        }

        // Phase 2: one shared crowd step for the whole shard. This is the whole point of the crowd — planning
        // every agent in one pass is what lets each NPC account for the others.
        _shard.Crowd.RemoveStaleAgents(_liveNpcIds);
        _shard.Crowd.Update(dt);

        // Phase 3: read back the planned velocities and run the motor, assists and broadcast state.
        foreach (var step in _movingSteps)
        {
            FinishMovingNpc(step, dt);
        }
    }

    public void MoveTo(CharacterEntity npc, Vector3 destination)
    {
        if (npc.NpcMovement is { } comp)
        {
            comp.SetDestination(destination);
        }
    }

    public void Stop(CharacterEntity npc)
    {
        if (npc.NpcMovement is { } comp)
        {
            StopNpc(npc, comp);
        }
    }

    /// <summary>
    ///     Puts an NPC into follow mode: it continuously trails <paramref name="targetId" />, closing the horizontal
    ///     gap until within <paramref name="leashRadius" /> and holding there until the target moves out of range.
    /// </summary>
    /// <param name="npc">The NPC to put into follow mode.</param>
    /// <param name="targetId">The entity id to trail.</param>
    /// <param name="leashRadius">The horizontal gap at which the NPC stops and holds.</param>
    public void Follow(CharacterEntity npc, ulong targetId, float leashRadius = FollowDefaultRadius)
    {
        if (npc.NpcMovement is { } comp)
        {
            comp.SetTarget(targetId, leashRadius);
        }
    }

    /// <summary>Drops an NPC's follow target and brakes it to a stop.</summary>
    /// <param name="npc">The NPC whose follow target is cleared.</param>
    public void ClearTarget(CharacterEntity npc)
    {
        if (npc.NpcMovement is { } comp)
        {
            comp.ClearTarget();
            StopNpc(npc, comp);
        }
    }

    /// <summary>
    ///     Puts an NPC into patrol mode: it walks <paramref name="route" /> in sequence (cyclically), starting from the
    ///     waypoint nearest its current position. The route is retained, so clearing a follow target or a manual move
    ///     resumes patrol.
    /// </summary>
    /// <param name="npc">The NPC to put into patrol mode.</param>
    /// <param name="route">The waypoints to walk cyclically.</param>
    public void Patrol(CharacterEntity npc, IReadOnlyList<Vector3> route)
    {
        if (npc.NpcMovement is { } comp && route is { Count: > 0 })
        {
            comp.SetPatrol(SnapRouteToNavmesh(route));
        }
    }

    /// <summary>Drops an NPC's patrol route and brakes it to a stop.</summary>
    /// <param name="npc">The NPC whose patrol route is cleared.</param>
    public void ClearPatrol(CharacterEntity npc)
    {
        if (npc.NpcMovement is { } comp)
        {
            comp.ClearPatrol();
            StopNpc(npc, comp);
        }
    }

    /// <summary>
    ///     Puts an NPC into wander mode: it roams within <paramref name="radius" /> of <paramref name="center" />,
    ///     picking a new navmesh-accessible point after each idle pause.
    /// </summary>
    /// <param name="npc">The NPC to put into wander mode.</param>
    /// <param name="center">The wander area center.</param>
    /// <param name="radius">The wander area radius.</param>
    public void Wander(CharacterEntity npc, Vector3 center, float radius = WanderDefaultRadius)
    {
        if (npc.NpcMovement is { } comp)
        {
            comp.SetWander(center, radius);
        }
    }

    /// <summary>Disables an NPC's wander mode and brakes it to a stop.</summary>
    /// <param name="npc">The NPC whose wander mode is disabled.</param>
    public void ClearWander(CharacterEntity npc)
    {
        if (npc.NpcMovement is { } comp)
        {
            comp.ClearWander();
            StopNpc(npc, comp);
        }
    }

    private static bool IsSlow(CharacterEntity npc)
    {
        return npc.Velocity.LengthSquared() < 0.04f;
    }

    private static float HorizontalDistance(Vector3 a, Vector3 b)
    {
        var dx = a.X - b.X;
        var dy = a.Y - b.Y;
        return MathF.Sqrt((dx * dx) + (dy * dy));
    }

    // Index of the waypoint nearest `from` (horizontal), used to (re)join a patrol route at the closest point.
    private static int NearestWaypointIndex(IReadOnlyList<Vector3> route, Vector3 from)
    {
        var best = 0;
        var bestDist = float.MaxValue;
        for (var i = 0; i < route.Count; i++)
        {
            var d = HorizontalDistance(from, route[i]);
            if (d < bestDist)
            {
                bestDist = d;
                best = i;
            }
        }

        return best;
    }

    /// <summary>
    ///     Composes the broadcast movement state from the live NPC condition:
    ///     - Movestate: Falling when not grounded (falling via gravity); otherwise Running at/above the run speed, Walking at/above
    ///       <see cref="_walkMinSpeed" />, else Standing.
    ///     - Movement flag: raised only while we are actually steering (giving input).
    ///     - Sprint/Crouch flags: from the behavior-driven toggles, only while steering.
    /// </summary>
    /// <param name="comp">The NPC's movement component (behavior toggles and speed thresholds).</param>
    /// <param name="grounded">Whether the NPC is currently grounded.</param>
    /// <param name="effectiveSpeed">The NPC's current horizontal speed.</param>
    /// <param name="isSteering">Whether the NPC is currently being given steering input.</param>
    /// <returns>The packed broadcast movement-state value.</returns>
    private static short BuildMovementState(NpcMovementComponent comp, bool grounded, float effectiveSpeed, bool isSteering)
    {
        var flags = (MovementFlags)0;
        if (isSteering)
        {
            flags |= MovementFlags.Movement;
            if (comp.Crouch)
            {
                flags |= MovementFlags.Crouch;
            }
            else if (comp.Sprint && !comp.Walk && !comp.HasCombat)
            {
                // Suppress the Sprint flag while in combat: the NPC still moves at the fast (sprint) speed (TargetSpeed
                // is unaffected), but broadcasting the Sprint flag during combat maneuvering causes odd client animations.
                flags |= MovementFlags.Sprint;
            }
        }

        Movestate movestate;
        if (!grounded)
        {
            movestate = Movestate.Falling;
        }
        else if (effectiveSpeed < _walkMinSpeed)
        {
            movestate = Movestate.Standing;
        }
        else if (comp.Walk || effectiveSpeed < comp.Speeds.Run)
        {
            // Walk gait forces the Walking animation even at Normal speed; below run speed also reads Walking.
            movestate = Movestate.Walking;
        }
        else
        {
            movestate = Movestate.Running;
        }

        return (short)((byte)movestate << 8 | (byte)flags);
    }

    private static float GetMaxAcceleration(NpcMovementComponent comp)
    {
        return MathF.Max(1f, comp.Speeds.Sprint / _timeToTopSpeed);
    }

    private static float GetBrakeAcceleration(NpcMovementComponent comp)
    {
        return GetMaxAcceleration(comp) * _brakeAccelerationMultiplier;
    }

    private static Quaternion CreateBipedOrientation(CharacterEntity npc, Vector3 desiredVelocity)
    {
        if (new Vector3(desiredVelocity.X, desiredVelocity.Y, 0f).LengthSquared() <= 0.01f)
        {
            return npc.Orientation;
        }

        return Quaternion.CreateFromAxisAngle(Vector3.UnitZ, MathF.Atan2(desiredVelocity.X, desiredVelocity.Y));
    }

    /// <summary>
    ///     True for solid bodies an NPC should steer around: deployables, vehicles, and live players. Pathing NPCs are
    ///     excluded (they share the navmesh/physics and steering around each other is a separate concern).
    /// </summary>
    /// <param name="entity">The entity to classify.</param>
    /// <returns>True when the entity is a solid body the NPC should steer around.</returns>
    private static bool IsAvoidableBody(IEntity entity)
    {
        return entity switch
        {
            DeployableEntity => true,
            VehicleEntity => true,
            CharacterEntity character => !character.IsNpc && character.IsAlive,
            _ => false,
        };
    }

    private static float AngleOf(Vector3 horizontal)
    {
        return MathF.Atan2(horizontal.Y, horizontal.X) * (180f / MathF.PI);
    }

    private static float AngleDifference(float a, float b)
    {
        var d = MathF.Abs(a - b) % 360f;
        return d > 180f ? 360f - d : d;
    }

    /// <summary>
    ///     Horizontal arrival check for the crowd path. Judged against the goal navigation was last given, not the
    ///     live stored destination, matching what <see cref="NavMeshPathStrategy" /> does with its own copy. Also
    ///     accepts an NPC the crowd has parked just short of a goal other agents already occupy — otherwise a group
    ///     sent to one point never finishes, because only one agent fits inside the strict radius.
    /// </summary>
    private static bool HasReachedDestination(CharacterEntity npc, NpcMovementComponent comp, DtCrowdAgent agent)
    {
        if (!comp.HasRequestedGoal)
        {
            return false;
        }

        var toDest = new Vector3(comp.RequestedGoal.X - npc.Position.X, comp.RequestedGoal.Y - npc.Position.Y, 0f);
        var distance = toDest.Length();
        if (distance <= _arrivalRadius)
        {
            return true;
        }

        if (distance > _crowdRestRadius || agent.targetState != DtMoveRequestState.DT_CROWDAGENT_TARGET_VALID)
        {
            return false;
        }

        // Requiring a resolved target matters: an agent whose path request has not been answered yet also has
        // no velocity, and would otherwise be treated as parked the instant it was given a nearby goal.
        var vel = agent.vel;
        var speed = MathF.Sqrt((vel.X * vel.X) + (vel.Z * vel.Z));
        return speed <= _crowdRestSpeed;
    }

    /// <summary>
    ///     Launch watchdog: reads the real integrated body Z-velocity (not the entity's reported velocity, which is
    ///     overwritten with the desired steering each tick) and logs a detailed snapshot when a biped is rising fast
    ///     or its Z-velocity just jumped. The "lastAssist" field attributes the spike to the most recent traverse
    ///     assist; the step-up nudge is the only assist that records one.
    /// </summary>
    /// <param name="npc">The NPC whose integrated body velocity is sampled.</param>
    /// <param name="comp">The NPC's movement component (holds the last body Z-velocity and log cooldown).</param>
    private void LogNpcLaunch(CharacterEntity npc, NpcMovementComponent comp)
    {
        var bodyVel = _shard.Physics.GetNpcVelocity(npc);
        var velZ = bodyVel.Z;
        var velDelta = velZ - comp.LastBodyVelZ;
        comp.NoteBodyVelZ(velZ);

        if ((velZ > _launchWatchVelZ || MathF.Abs(velDelta) > _launchWatchVelDelta) && comp.LaunchLogCooldown <= 0f)
        {
            comp.TriggerLaunchLogCooldown(_launchLogCooldown);
            _logger.Warning(
                "[NpcLaunch] entity={EntityId} pos=({PX:F1},{PY:F1},{PZ:F1}) bodyVel=({VX:F2},{VY:F2},{VZ:F2}) dZ={DZ:F2} lastAssist={Assist}(val={AV:F2}, {Age}t ago) grounded={G} slow={S} stepAcc={SA:F2}",
                npc.EntityId,
                npc.Position.X,
                npc.Position.Y,
                npc.Position.Z,
                bodyVel.X,
                bodyVel.Y,
                bodyVel.Z,
                velDelta,
                comp.LastAssist,
                comp.LastAssistValue,
                comp.LastAssistTicksAgo == int.MaxValue ? "inf" : comp.LastAssistTicksAgo.ToString(),
                comp.Grounded,
                IsSlow(npc),
                comp.StepUpAccumulator);
        }
    }

    /// <summary>
    ///     Follow-mode pre-pass. While the target is alive and farther than the leash radius (horizontal), keep the
    ///     destination pointed at the target's live position so the biped pipeline chases it; once inside the radius,
    ///     brake to a hold. If the target despawns or dies, stop but keep the id so the NPC resumes if it reappears.
    /// </summary>
    /// <param name="npc">The NPC in follow mode.</param>
    /// <param name="comp">The NPC's movement component.</param>
    /// <param name="dt">The timestep, in seconds.</param>
    private void TickTargetFollow(CharacterEntity npc, NpcMovementComponent comp, float dt)
    {
        if (_shard.Entities.TryGetValue(comp.TargetId, out var entity) &&
            entity is CharacterEntity { IsAlive: true } target)
        {
            var distance = HorizontalDistance(npc.Position, target.Position);
            if (distance > comp.LeashRadius)
            {
                // Out of range: chase. Keep the stored destination fresh for the periodic repath, and start the
                // body moving if it is currently holding.
                comp.UpdateDestination(target.Position);
                if (!comp.IsMoving)
                {
                    comp.SetDestination(target.Position);
                }
            }
            else if (comp.IsMoving)
            {
                // Inside the leash: hold position (the target id is retained, so it re-engages when the target leaves).
                StopNpc(npc, comp);
            }
        }
        else if (comp.IsMoving)
        {
            // Target gone or dead: stop in place, keep the id for automatic re-acquisition on respawn.
            StopNpc(npc, comp);
        }
    }

    /// <summary>
    ///     Combat-mode pre-pass. Holds a horizontal standoff of <see cref="NpcMovementComponent.CombatDist" /> from the
    ///     combat target and keeps the body facing it (via <see cref="NpcMovementComponent.CombatFacing" />, re-pointed at
    ///     the target's live position every tick so the NPC turns to track it even when it moves out of sight). Priority:
    ///     back off under <see cref="NpcMovementComponent.CombatMinDist" />, close in beyond <c>CombatDist</c>, then — when
    ///     in the band — flank toward the target to regain line of sight if it is behind cover, or strafe/juke (short
    ///     alternating side-steps) while it is visible. Aiming/firing is done by the outer behavior system.
    /// </summary>
    /// <param name="npc">The NPC in combat mode.</param>
    /// <param name="comp">The NPC's movement component.</param>
    /// <param name="dt">The timestep, in seconds.</param>
    private void TickCombat(CharacterEntity npc, NpcMovementComponent comp, float dt)
    {
        if (!(_shard.Entities.TryGetValue(comp.CombatTargetId, out var entity) &&
              entity is CharacterEntity { IsAlive: true } target))
        {
            return;
        }

        // Always face the target's live position, whether or not it is visible: the body yaw is driven from this facing
        // (FinishMovingNpc / TickStoppedBiped), so the NPC rotates to track the target as it moves around, including when
        // it breaks line of sight behind cover.
        var toTarget = new Vector3(target.Position.X - npc.Position.X, target.Position.Y - npc.Position.Y, 0f);
        if (toTarget.LengthSquared() > 0.0001f)
        {
            comp.SetCombatFacing(Vector3.Normalize(toTarget));
        }

        var distance = HorizontalDistance(npc.Position, target.Position);
        if (comp.CombatMinDist > 0f && distance < comp.CombatMinDist)
        {
            // Too close: back away along the target->npc line to re-establish the standoff band.
            var away = new Vector3(npc.Position.X - target.Position.X, npc.Position.Y - target.Position.Y, 0f);
            if (away.LengthSquared() > 0.01f)
            {
                var dest = npc.Position + (Vector3.Normalize(away) * comp.CombatDist);
                if (comp.IsMoving)
                {
                    comp.UpdateDestination(dest);
                }
                else
                {
                    comp.SetDestination(dest);
                }
            }
        }
        else if (distance > comp.CombatDist + _combatApproachHysteresis)
        {
            // Out of standoff range (beyond the hysteresis band): close in (keep the stored destination fresh for the
            // periodic repath).
            comp.UpdateDestination(target.Position);
            if (!comp.IsMoving)
            {
                comp.SetDestination(target.Position);
            }
        }
        else if (!comp.CombatHasLos)
        {
            // In range but the target is behind cover: flank toward it (the navmesh routes around the obstruction) to
            // regain line of sight. The body stays facing the target via CombatFacing.
            comp.UpdateDestination(target.Position);
            if (!comp.IsMoving)
            {
                comp.SetDestination(target.Position);
            }
        }
        else if (comp.IsMoving)
        {
            // A strafe leg is already in progress: let it play out (arrival re-arms this branch next tick).
            return;
        }
        else
        {
            // In range, visible, and idle: strafe/juke. Dwell, then take a short leg sideways, alternating sides.
            comp.DecrementCombatManeuverTimer(dt);
            if (comp.CombatManeuverTimer > 0f)
            {
                return;
            }

            if (TryBeginStrafeLeg(npc, comp, target.Position))
            {
                comp.ResetCombatManeuverTimer(_strafePauseMin + ((_strafePauseMax - _strafePauseMin) * _rand.NextSingle()));
            }
            else
            {
                // No room to step this way (blocked side): hold and retry shortly.
                comp.ResetCombatManeuverTimer(_strafePauseMin);
            }
        }
    }

    /// <summary>
    ///     Starts one strafe/juke leg: a short step along the standoff circle around the target (rotating the NPC's
    ///     offset-from-target around Z), snapped to the navmesh so it is walkable. Following the circle keeps the gap to
    ///     the target ~constant — a pure sideways step would grow it past CombatDist and re-trigger the approach, dragging
    ///     the NPC inward over successive legs. Returns false (caller holds and flips side) when the side is blocked: no
    ///     navmesh point, or the snapped leg would close the standoff by more than the step allows.
    /// </summary>
    /// <param name="npc">The NPC performing the strafe.</param>
    /// <param name="comp">The NPC's movement component (holds the strafe side and maneuver timer).</param>
    /// <param name="targetPos">The combat target's world position (the standoff circle center).</param>
    /// <returns>True when a walkable strafe leg was started; false when the side is blocked.</returns>
    private bool TryBeginStrafeLeg(CharacterEntity npc, NpcMovementComponent comp, Vector3 targetPos)
    {
        var offset = new Vector3(npc.Position.X - targetPos.X, npc.Position.Y - targetPos.Y, 0f);
        var radius = offset.Length();
        if (radius <= 0.01f)
        {
            return false;
        }

        var side = comp.CombatStrafeSide >= 0 ? 1f : -1f;
        var angle = (_strafeLegDistance / MathF.Max(radius, 1f)) * side;
        var cos = MathF.Cos(angle);
        var sin = MathF.Sin(angle);
        var rotated = new Vector3((offset.X * cos) - (offset.Y * sin), (offset.X * sin) + (offset.Y * cos), 0f);
        var dest = new Vector3(targetPos.X + rotated.X, targetPos.Y + rotated.Y, npc.Position.Z);

        var nav = _shard.NavMesh;
        if (nav != null && nav.IsLoaded)
        {
            if (!nav.FindNearestPoly(dest, _maneuverSnapRadius, out _, out var snapped))
            {
                return false;
            }

            dest = snapped;
        }

        // Reject a leg that closes the standoff more than the step allows (that side is blocked) so the NPC holds and
        // tries the other side next tick instead of drifting toward the target.
        if (HorizontalDistance(dest, targetPos) < radius - _strafeMinLeg)
        {
            return false;
        }

        comp.SetDestination(dest);
        comp.SetCombatStrafeSide(-comp.CombatStrafeSide);
        return true;
    }

    /// <summary>
    ///     Points the NPC's aim and body yaw at its combat target. Runs after the move/stop dispatch so it is the final
    ///     word on aim for the tick; the WeaponSim reads <see cref="CharacterEntity.AimDirection" /> (pose fallback) when
    ///     it fires, so shots land on the target whether the NPC is approaching, strafing, or holding.
    /// </summary>
    /// <param name="npc">The NPC whose aim and body yaw are set.</param>
    /// <param name="comp">The NPC's movement component (holds the combat target id).</param>
    private void AimAtCombatTarget(CharacterEntity npc, NpcMovementComponent comp)
    {
        if (!(_shard.Entities.TryGetValue(comp.CombatTargetId, out var entity) &&
              entity is CharacterEntity { IsAlive: true } target))
        {
            return;
        }

        // Aim muzzle-to-chest (Z-up): a small torso offset on both ends keeps the line roughly horizontal.
        var muzzle = new Vector3(npc.Position.X, npc.Position.Y, npc.Position.Z + 1.2f);
        var chest = new Vector3(target.Position.X, target.Position.Y, target.Position.Z + 1.0f);
        var aim = chest - muzzle;
        if (aim.LengthSquared() <= 0.0001f)
        {
            return;
        }

        aim = Vector3.Normalize(aim);
        npc.AimDirection = aim;
        npc.Orientation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, MathF.Atan2(aim.X, aim.Y));
    }

    /// <summary>
    ///     Projects each patrol waypoint onto the nearest walkable navmesh point so every leg is actually reachable.
    ///     Without this, a waypoint that sits off the mesh is unreachable: the path ends at the nearest on-mesh point
    ///     but arrival is judged against the raw coordinate, so the NPC never gets close enough to advance. When no
    ///     navmesh is loaded the route is returned unchanged (direct steering reaches the exact point).
    /// </summary>
    /// <param name="route">The raw patrol waypoints to snap.</param>
    /// <returns>The waypoints snapped to the nearest walkable navmesh points, or the original route when no navmesh is loaded.</returns>
    private IReadOnlyList<Vector3> SnapRouteToNavmesh(IReadOnlyList<Vector3> route)
    {
        var nav = _shard.NavMesh;
        if (nav == null || !nav.IsLoaded)
        {
            return route;
        }

        var snapped = new Vector3[route.Count];
        for (var i = 0; i < route.Count; i++)
        {
            snapped[i] = nav.FindNearestPoly(route[i], _patrolSnapRadius, out _, out var closest) ? closest : route[i];
        }

        return snapped;
    }

    /// <summary>
    ///     Patrol-mode pre-pass. When the NPC is idle it either advances to the next waypoint (it has reached the one it
    ///     was heading to) or rejoins the route at the nearest waypoint (it is far off the route, e.g. after an aggro
    ///     interruption), then paths there. While already moving it does nothing — the moving pipeline drives the leg and
    ///     the arrival StopNpc re-arms this pre-pass on the next tick.
    /// </summary>
    /// <param name="npc">The NPC in patrol mode.</param>
    /// <param name="comp">The NPC's movement component (holds the route and current waypoint index).</param>
    /// <param name="dt">The timestep, in seconds.</param>
    private void TickPatrol(CharacterEntity npc, NpcMovementComponent comp, float dt)
    {
        if (comp.IsMoving || comp.PatrolRoute is not { Count: > 0 } route)
        {
            return;
        }

        var current = route[comp.PatrolIndex];
        if (HorizontalDistance(npc.Position, current) <= _patrolArriveRadius)
        {
            // Reached the current waypoint: advance to the next (a single-point route just holds in place).
            if (route.Count <= 1)
            {
                return;
            }

            comp.SetPatrolIndex((comp.PatrolIndex + 1) % route.Count);
        }
        else
        {
            // Off the route (resuming after another mode took over): rejoin at the nearest waypoint.
            comp.SetPatrolIndex(NearestWaypointIndex(route, npc.Position));
        }

        comp.SetDestination(route[comp.PatrolIndex]);
    }

    /// <summary>
    ///     Wander-mode pre-pass. While idle it counts down a random dwell, then picks a new accessible point within the
    ///     wander circle and paths there (setting the dwell for the next leg). While moving it does nothing; the arrival
    ///     StopNpc re-arms this pre-pass on the next tick.
    /// </summary>
    /// <param name="npc">The NPC in wander mode.</param>
    /// <param name="comp">The NPC's movement component (holds the wander area and pause timer).</param>
    /// <param name="dt">The timestep, in seconds.</param>
    private void TickWander(CharacterEntity npc, NpcMovementComponent comp, float dt)
    {
        if (comp.IsMoving)
        {
            return;
        }

        comp.DecrementWanderPauseTimer(dt);
        if (comp.WanderPauseTimer > 0f)
        {
            return;
        }

        if (!TryPickWanderPoint(npc, comp, out var point))
        {
            comp.ResetWanderPauseTimer(_wanderRetryDelay);
            return;
        }

        comp.SetDestination(point);
        comp.ResetWanderPauseTimer(_wanderPauseMin + ((_wanderPauseMax - _wanderPauseMin) * _rand.NextSingle()));
    }

    /// <summary>
    ///     Picks the next wander target: a random navmesh-accessible point within the wander circle, falling back to a
    ///     uniformly random point in the disk when no navmesh is reachable (the strategy then steers directly).
    /// </summary>
    /// <param name="npc">The NPC in wander mode.</param>
    /// <param name="comp">The NPC's movement component (holds the wander center and radius).</param>
    /// <param name="point">Receives the chosen wander target point.</param>
    /// <returns>True when a point was found; false when the wander area yielded none.</returns>
    private bool TryPickWanderPoint(CharacterEntity npc, NpcMovementComponent comp, out Vector3 point)
    {
        var nav = _shard.NavMesh;
        if (nav != null && nav.TryFindRandomPointWithinCircle(comp.WanderCenter, comp.WanderRadius, out point))
        {
            return true;
        }

        // No navmesh (or none reachable): a uniform random point in the disk keeps wander working via direct steering.
        var angle = _rand.NextSingle() * (2f * MathF.PI);
        var r = comp.WanderRadius * MathF.Sqrt(_rand.NextSingle());
        point = comp.WanderCenter + new Vector3(MathF.Cos(angle) * r, MathF.Sin(angle) * r, 0f);
        return true;
    }

    /// <summary>
    ///     First half of the moving-NPC tick: pushes the goal, probes ground, checks arrival, and computes the
    ///     steering inline only when the NPC has no crowd agent. Crowd NPCs defer their steering to
    ///     <see cref="FinishMovingNpc" />, after the shared simulation has run.
    /// </summary>
    /// <param name="npc">The NPC being stepped.</param>
    /// <param name="comp">The NPC's movement component.</param>
    /// <param name="dt">The timestep, in seconds.</param>
    /// <returns>The state to resume with after the crowd step, or null when the NPC was stopped here.</returns>
    private MovingStep? BeginMovingNpc(CharacterEntity npc, NpcMovementComponent comp, float dt)
    {
        var physics = _shard.Physics;
        var agent = comp.CrowdAgent;

        // 1. Refresh the goal periodically (and on the first tick after a destination change). The crowd keeps its
        //    own re-plan throttle (targetReplanDelay), so this cadence is an upper bound, not a re-path rate.
        comp.DecrementRepathTimer(dt);
        if (comp.RepathTimer <= 0f)
        {
            if (agent != null)
            {
                _shard.Crowd.RequestGoal(agent, comp.Destination);
                comp.NoteRequestedGoal(comp.Destination);
            }
            else
            {
                comp.Strategy.SetDestination(comp.Destination);
            }

            comp.ResetRepathTimer(_repathInterval);
        }

        // 2. Probe for ground. This drives air control.
        var grounded = physics.TryGroundProbe(npc, _groundProbeRange, _bipedMinNormalZ, out _, out var groundNormal);
        comp.UpdateGrounded(grounded, groundNormal);

        // 3. Arrival check. The crowd path judges arrival against the live stored destination; the path strategy
        //    keeps judging it against the destination it was last given, exactly as before.
        if (agent != null ? HasReachedDestination(npc, comp, agent) : comp.Strategy.HasArrived(npc.Position))
        {
            StopNpc(npc, comp);
            return null;
        }

        var desired = Vector3.Zero;
        if (agent == null)
        {
            comp.Strategy.Speed = comp.TargetSpeed;
            comp.Strategy.Tick(dt, npc.Position, out var steering);
            desired = new Vector3(steering.X, steering.Y, 0f);
            if (desired.LengthSquared() > 0.01f)
            {
                desired *= grounded ? 1f : _airControlMultiplier;
            }
        }

        return new MovingStep
        {
            Npc = npc,
            Comp = comp,
            Grounded = grounded,
            Agent = agent,
            Desired = desired,
        };
    }

    /// <summary>
    ///     Second half of the moving-NPC tick, after the crowd has planned every agent: take the planned velocity,
    ///     copy across the crowd's positional de-overlap, then run the assists, motor and broadcast state exactly
    ///     as the single-pass version did.
    /// </summary>
    /// <param name="step">The state carried over from <see cref="BeginMovingNpc" />.</param>
    /// <param name="dt">The timestep, in seconds.</param>
    private void FinishMovingNpc(MovingStep step, float dt)
    {
        var npc = step.Npc;
        var comp = step.Comp;
        var physics = _shard.Physics;
        var grounded = step.Grounded;
        var desired = step.Desired;

        // 3b. Crowd steering. Read the velocity planned for this agent; the crowd's own position integration is
        //     discarded (physics is authoritative) except for the bounded de-overlap share copied below.
        if (step.Agent is { } agent)
        {
            if (_shard.Crowd.TryReadVelocity(agent, out var planned))
            {
                desired = planned;
                if (desired.LengthSquared() > 0.01f)
                {
                    desired *= grounded ? 1f : _airControlMultiplier;
                }
            }
            else
            {
                // No usable route (unreachable or off-mesh goal): steer straight at it, which is what the path
                // strategy did when FindPath returned nothing.
                desired = SteerDirectly(npc, comp, grounded);
            }

            ApplyCrowdDeoverlap(npc, agent);
        }

        // 4b. Local avoidance: nudge the heading around nearby solid bodies (deployables, vehicles, players) so the NPC
        //     slides past them instead of pressing into them and stalling (the navmesh can't carve a small hole around them).
        //     NPC-versus-NPC avoidance is now the crowd's job; this still covers the bodies that are not crowd agents.
        desired = ApplyObstacleAvoidance(npc, comp, desired);

        TraceCrowdDecision(npc, comp, step.Agent, desired);

        // 5. Traverse assist: a collision-based ledge step-up (the navmesh surface-follow and gap hop were removed).
        if (grounded && desired.LengthSquared() > 0.01f)
        {
            if (physics.TryProbeAheadGround(npc, desired, _bipedStepProbeDistance, _bipedStepProbeUp, _bipedStepProbeDown, out _, out var aheadHeight, out var surfaceNormal))
            {
                var walkable = Vector3.Dot(surfaceNormal, Vector3.UnitZ) >= _bipedStepMinSurfaceZ;
                var lift = aheadHeight + _bipedStepClearance;
                if (aheadHeight > _minStepLift && aheadHeight <= _bipedMaxStepHeight && IsSlow(npc) && walkable && comp.StepUpAccumulator + lift <= _bipedMaxStepHeight + _bipedStepClearance)
                {
                    // Stalled against a climbable ledge (or the raised floor of a small hole) ahead: lift just enough to clear it so the
                    // motor carries it over. The nudge is a position teleport (no velocity change), so it's gated on IsSlow and capped by
                    // StepUpAccumulator to keep it from re-firing every tick against a face the biped can't climb.
                    physics.NudgeNpcPosition(npc, new Vector3(0f, 0f, lift));
                    comp.AccumulateStepUp(lift);
                    comp.RecordAssist("step-up", lift);
                    _logger.Information(
                        "[NpcAssist] entity={EntityId} STEP-UP lift={Lift:F2} aheadHeight={AH:F2} normZ={NZ:F2} stepAcc={SA:F2} pos=({PX:F1},{PY:F1},{PZ:F1})",
                        npc.EntityId,
                        lift,
                        aheadHeight,
                        surfaceNormal.Z,
                        comp.StepUpAccumulator,
                        npc.Position.X,
                        npc.Position.Y,
                        npc.Position.Z);
                }
                else if (aheadHeight <= _minStepLift)
                {
                    // The forward surface is now level with (or below) the NPC: it has cleared the ledge, so open the lift budget again.
                    comp.ResetStepUp();
                }
            }
        }
        else
        {
            // Not grounded / not moving: the stall is over, so open the lift budget for the next one.
            comp.ResetStepUp();
        }

        // 6. Stuck / repath handling for grounded NPCs that are trying to move.
        // NOTE: currently disabled with the if (false) guard so the velocity motor runs in isolation; the stuck
        // detector never repaths or stops the NPC. Remove the guard to re-enable it.
        if (false && (grounded && desired.LengthSquared() > 0.01f))
        {
            var stuck = comp.UpdateStuckTracking(npc.Position, dt, _stuckMinDisplacement, _stuckDuration);
            if (stuck)
            {
                if (comp.RepathAttempts < _maxRepathAttempts)
                {
                    comp.RegisterRepath();
                    comp.Strategy.SetDestination(comp.Destination);
                    comp.ResetRepathTimer(0f);
                    comp.ResetStuckTracking();
                }
                else
                {
                    _logger.Warning("NPC {entityId} is stuck at {position} while trying to reach {destination}", npc.EntityId, npc.Position, comp.Destination);
                    _shard.EventBus.Enqueue(new NpcStuckEvent(npc.EntityId, npc.Position, comp.Destination));
                    StopNpc(npc, comp);
                    return;
                }
            }
        }
        else
        {
            comp.ResetStuckTracking();
        }

        // 7. Orientation: bipeds yaw around Z. While in combat the body faces the combat target (CombatFacing) rather
        //    than the movement direction, so it strafes / backs off / flanks while keeping its aim on the target.
        var orientation = comp.HasCombatFacing
            ? CreateBipedOrientation(npc, comp.CombatFacing)
            : CreateBipedOrientation(npc, desired);

        // 8. Publish the desired velocity; physics applies the bounded motor during the next fixed timestep.
        physics.SetNpcSteering(npc, desired, GetMaxAcceleration(comp), orientation);

        // 9. Update the entity's reported state for same-tick systems and the next physics broadcast.
        npc.Velocity = desired;
        if (desired.LengthSquared() > 0.01f)
        {
            npc.Orientation = orientation;
            npc.AimDirection = comp.HasCombatFacing ? Vector3.Normalize(comp.CombatFacing) : Vector3.Normalize(desired);
        }

        // 10. Publish the movement state: Falling when airborne, else Running/Walking/Standing from the input speed; the Movement
        //     flag tracks whether we are steering, and Sprint/Crouch come from the behavior toggles.
        npc.MovementState = BuildMovementState(comp, grounded, desired.Length(), desired.LengthSquared() > 0.01f);

        // Combat: face/aim the target after steering so the WeaponSim (which runs later this tick) shoots where the
        // NPC looks, whether it is approaching, strafing, or holding. Overrides the movement-direction aim set above.
        if (comp.HasCombat)
        {
            AimAtCombatTarget(npc, comp);
        }
    }

    /// <summary>
    ///     Straight-at-the-goal steering, used when the crowd has no usable route. Mirrors what
    ///     <see cref="NavMeshPathStrategy" /> does when <see cref="NavMeshService.FindPath" /> returns nothing,
    ///     including steering at the fixed goal rather than the live destination so a self-anchored destination
    ///     cannot pull the NPC indefinitely.
    /// </summary>
    /// <param name="npc">The NPC being steered.</param>
    /// <param name="comp">The NPC's movement component (holds the goal and gait).</param>
    /// <param name="grounded">Whether the NPC is currently grounded (air control scales the input down).</param>
    /// <returns>The desired horizontal velocity.</returns>
    private Vector3 SteerDirectly(CharacterEntity npc, NpcMovementComponent comp, bool grounded)
    {
        if (!comp.HasRequestedGoal)
        {
            return Vector3.Zero;
        }

        var to = new Vector3(comp.RequestedGoal.X - npc.Position.X, comp.RequestedGoal.Y - npc.Position.Y, 0f);
        var dist = to.Length();
        if (dist <= 0.001f)
        {
            return Vector3.Zero;
        }

        var desired = (to / dist) * comp.TargetSpeed;
        if (desired.LengthSquared() > 0.01f)
        {
            desired *= grounded ? 1f : _airControlMultiplier;
        }

        return desired;
    }

    /// <summary>
    ///     Copies the crowd's positional de-overlap onto the physics body. The crowd resolves overlapping agents by
    ///     moving them apart, but physics owns the bodies, so that correction would otherwise be thrown away — it is
    ///     the part that matters once many NPCs are packed together. Bounded so a crowded pile cannot teleport a body.
    /// </summary>
    /// <param name="npc">The NPC to correct.</param>
    /// <param name="agent">The NPC's crowd agent.</param>
    private void ApplyCrowdDeoverlap(CharacterEntity npc, DtCrowdAgent agent)
    {
        var limit = _shard.Settings.CrowdSeparationNudge;
        if (limit <= 0f || !_shard.Crowd.TryGetDeoverlap(agent, out var delta))
        {
            return;
        }

        var length = delta.Length();
        if (length <= 0.01f)
        {
            return;
        }

        if (length > limit)
        {
            delta *= limit / length;
        }

        _shard.Physics.NudgeNpcPosition(npc, delta);
    }

    /// <summary>
    ///     Deflects the heading sideways around nearby solid bodies (deployables, vehicles, players) that are close and
    ///     ahead, so the NPC slides past them instead of pressing into them and stalling.
    /// </summary>
    /// <param name="npc">The NPC being steered.</param>
    /// <param name="comp">The NPC's movement component (its follow target is skipped).</param>
    /// <param name="desired">The desired steering velocity before avoidance.</param>
    /// <returns>The deflected steering velocity, with the commanded speed preserved.</returns>
    private Vector3 ApplyObstacleAvoidance(CharacterEntity npc, NpcMovementComponent comp, Vector3 desired)
    {
        var baseSpeed = desired.Length();
        if (baseSpeed <= 0.1f)
        {
            return desired;
        }

        var forward = desired / baseSpeed;
        var avoidance = Vector3.Zero;

        foreach (var entity in _shard.Entities.Values)
        {
            if (entity.EntityId == npc.EntityId || entity.EntityId == comp.TargetId || !IsAvoidableBody(entity))
            {
                continue;
            }

            // Horizontal vector from the NPC to the body (Z-up; vertical ignored).
            var toObstacle = new Vector3(entity.Position.X - npc.Position.X, entity.Position.Y - npc.Position.Y, 0f);
            var dist = toObstacle.Length();
            if (dist <= 0.01f || dist >= _obstacleAvoidanceRadius)
            {
                continue;
            }

            // Only avoid bodies ahead of the direction of travel; ones behind need no deflection.
            var along = Vector3.Dot(toObstacle, forward);
            if (along <= 0f)
            {
                continue;
            }

            // Steer to the side opposite the body's lateral offset from the heading line. When it is dead ahead
            // (no lateral offset) pick a consistent side to slip around; the off-center case then guides the arc.
            var away = (forward * along) - toObstacle;
            if (away.LengthSquared() < 0.01f)
            {
                away = new Vector3(-forward.Y, forward.X, 0f);
            }

            var strength = (1f - (dist / _obstacleAvoidanceRadius)) * baseSpeed;
            avoidance += Vector3.Normalize(away) * strength;
        }

        if (avoidance.LengthSquared() <= 0.0001f)
        {
            return desired;
        }

        var result = desired + avoidance;
        var len = result.Length();
        if (len > baseSpeed)
        {
            result *= baseSpeed / len;
        }

        return result;
    }

    private void TickStoppedNpc(CharacterEntity npc, NpcMovementComponent comp, float dt)
    {
        TickStoppedBiped(npc, comp, dt);
    }

    private void TickStoppedBiped(CharacterEntity npc, NpcMovementComponent comp, float dt)
    {
        // Steer the body to a stop while the brake is active, then let the motor sleep. While in combat the body keeps
        // rotating to face the target (CombatFacing) even as it brakes/holds, so it tracks a target that moves off-line.
        if (comp.BrakeTimer > 0f)
        {
            comp.DecrementBrakeTimer(dt);
            var orient = comp.HasCombatFacing ? CreateBipedOrientation(npc, comp.CombatFacing) : npc.Orientation;
            _shard.Physics.StopNpcSteering(npc, GetBrakeAcceleration(comp), orient);
        }
        else if (IsSlow(npc))
        {
            _shard.Physics.DisableNpcSteering(npc);
        }

        // Always recompute the movement state so it keeps tracking the live condition once the brake expires (fixes the state
        // getting "stuck", e.g. in Falling, after arriving). Use the ground probe for Falling and the real (decaying) horizontal
        // velocity for the Running -> Walking -> Standing coast-out. No steering input here, so the Movement flag is off.
        var grounded = _shard.Physics.TryGroundProbe(npc, _groundProbeRange, _bipedMinNormalZ, out _, out var groundNormal);
        comp.UpdateGrounded(grounded, groundNormal);
        var bodyVel = _shard.Physics.GetNpcVelocity(npc);
        var horizontalSpeed = new Vector3(bodyVel.X, bodyVel.Y, 0f).Length();
        npc.MovementState = BuildMovementState(comp, grounded, horizontalSpeed, isSteering: false);
    }

    private void StopNpc(CharacterEntity npc, NpcMovementComponent comp)
    {
        comp.Stop();

        // Drop the crowd goal too: an agent with a live target keeps being planned and would re-accelerate the
        // body against the brake. The agent itself stays registered so the others still walk around a standing NPC.
        if (comp.CrowdAgent is { } agent)
        {
            _shard.Crowd.ResetGoal(agent);
        }

        _shard.Physics.StopNpcSteering(npc, GetBrakeAcceleration(comp), npc.Orientation);
        npc.MovementState = (short)((byte)Movestate.Standing << 8);
    }

    /// <summary>
    ///     One Debug-level line per NPC per second comparing the heading the NPC is actually given against the
    ///     heading to its destination, with the crowd's own state alongside it. The crowd's failure modes are
    ///     invisible from outside: an agent that is off-mesh, or whose target it could not route to, yields a
    ///     velocity unrelated to the destination, and that reads as "the NPCs are moving strangely" rather than
    ///     as a diagnosable state. Heading divergence is the number that separates a steering bug from a
    ///     behaviour bug. Gated behind <see cref="GameServerSettings.CrowdDebugLogging" /> so the trace can be
    ///     silenced without also silencing every other Debug-level message.
    /// </summary>
    private void TraceCrowdDecision(CharacterEntity npc, NpcMovementComponent comp, DtCrowdAgent? agent, Vector3 desired)
    {
        if (!_shard.Settings.CrowdDebugLogging || !_logger.IsEnabled(LogEventLevel.Debug) || _traceTick % _traceCadence != 0)
        {
            return;
        }

        var toDest = new Vector3(comp.Destination.X - npc.Position.X, comp.Destination.Y - npc.Position.Y, 0f);
        var heading = desired.LengthSquared() > 0.0001f ? AngleOf(desired) : float.NaN;
        var destHeading = toDest.LengthSquared() > 0.0001f ? AngleOf(toDest) : float.NaN;

        // How far the body sits from the walkable mesh. PIN's own pathfinding searches with a 200u extent, so an
        // NPC can path fine while standing nowhere near the mesh — and the crowd, which places agents with a
        // reach of about one agent radius, marks such an agent invalid and stops steering it entirely.
        var meshGap = _shard.NavMesh.FindNearestPoly(npc.Position, 200f, out _, out var onMesh)
            ? (onMesh - npc.Position).Length()
            : float.NaN;

        _logger.Debug(
            "NPC {EntityId} steer: mode={Mode} destDist={DestDist:F1} meshGap={MeshGap:F1} state={State} target={Target} speed={Speed:F1} heading={Heading:F0} destHeading={DestHeading:F0} diverge={Diverge:F0}",
            npc.EntityId,
            comp.Mode,
            toDest.Length(),
            meshGap,
            agent?.state.ToString() ?? "none",
            agent?.targetState.ToString() ?? "none",
            desired.Length(),
            heading,
            destHeading,
            float.IsNaN(heading) || float.IsNaN(destHeading) ? float.NaN : AngleDifference(heading, destHeading));
    }

    /// <summary>
    ///     Keeps the NPC's crowd agent present and in step with its real body. Agents are created lazily so the
    ///     order of navmesh load versus NPC spawn does not matter, and idle NPCs keep one because a standing NPC is
    ///     still something the others have to walk around.
    /// </summary>
    /// <param name="npc">The NPC to sync.</param>
    /// <param name="comp">The NPC's movement component (holds the agent and current gait).</param>
    private void SyncCrowdAgent(CharacterEntity npc, NpcMovementComponent comp)
    {
        var crowd = _shard.Crowd;
        if (!crowd.IsEnabled)
        {
            return;
        }

        comp.CrowdAgent ??= crowd.AcquireAgent(npc.EntityId, npc.Position, comp.TargetSpeed, GetMaxAcceleration(comp));
        if (comp.CrowdAgent is not { } agent)
        {
            return;
        }

        // The crowd integrates its own positions; without this it plans from poses nothing occupies.
        crowd.SyncPose(agent, npc.Position);
        crowd.SetSpeed(agent, comp.TargetSpeed, GetMaxAcceleration(comp));
    }

    /// <summary>Drops a dead NPC's crowd agent so the corpse stops occupying space in the simulation.</summary>
    /// <param name="npc">The NPC whose agent is released.</param>
    /// <param name="comp">The NPC's movement component.</param>
    private void ReleaseCrowdAgent(CharacterEntity npc, NpcMovementComponent comp)
    {
        if (comp.CrowdAgent == null)
        {
            return;
        }

        _shard.Crowd.RemoveAgent(npc.EntityId);
        comp.CrowdAgent = null;
    }

    /// <summary>
    ///     Per-NPC state carried from the goal/steering pre-pass to the post-crowd pass, so the shard is walked
    ///     twice per tick without re-probing the ground or re-deriving the movement mode.
    /// </summary>
    private struct MovingStep
    {
        public CharacterEntity Npc;
        public NpcMovementComponent Comp;
        public bool Grounded;
        public DtCrowdAgent? Agent;
        public Vector3 Desired;
    }
}
