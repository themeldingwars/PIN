#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using DotRecast.Detour;
using DotRecast.Detour.Crowd;
using Serilog;
using Shared.Collision.Navigation;

namespace GameServer.Navigation;

/// <summary>
///     One Detour crowd simulation per shard. The crowd is a *shared* simulation: it plans every agent
///     in one pass, so each agent's steering accounts for the others and nearby NPCs stop pathing through
///     one another. It replaces only the steering step of NPC movement — physics stays authoritative, the
///     crowd supplies the horizontal velocity the existing motor already consumes, and the crowd's own
///     position integration is overwritten from the real body pose every tick.
///     Driven from the shard tick thread only: the crowd owns a <see cref="DtNavMeshQuery" />, which is
///     not thread safe.
/// </summary>
public sealed class CrowdNavigationService
{
    // Neighbour sampling distance, as a multiple of the agent radius. Recast's demo uses ~12x; below that
    // the avoidance sees no neighbours, above it PlanVelocity dominates the tick cost.
    private const float _collisionQueryRangeFactor = 12f;

    // How far ahead the visibility-optimised corridor shortcut may reach, as a multiple of the radius.
    private const float _pathOptimizationRangeFactor = 30f;

    // Velocity-sampling quality. 3 is the best tier and affordable.
    private const int _obstacleAvoidanceType = 3;

    // How far a goal is snapped onto the mesh. A goal off the mesh has no poly to route to.
    private const float _goalSnapRadius = 5f;

    // Below this distance the crowd's own integration is already tracking the body, so re-syncing would only
    // fight the snap the crowd just performed.
    private const float _poseResyncTolerance = 0.05f;

    // The crowd sizes its agent-placement search from this
    private const float _maxAgentRadiusFactor = 2f;

    // Crowd steps between Debug-level cadence logs
    private const int _logCadenceTicks = 100;

    private readonly NavMeshService _navMesh;
    private readonly GameServerSettings _settings;
    private readonly ILogger _logger;
    private readonly DtQueryDefaultFilter _queryFilter = new();
    private readonly Dictionary<ulong, DtCrowdAgent> _agents = [];
    private readonly List<ulong> _staleScratch = [];
    private readonly HashSet<ulong> _refusedNotice = [];

    private Vector3[] _preUpdatePos = [];
    private Vector3[] _deoverlap = [];

    private DtCrowd? _crowd;
    private float _agentRadius;
    private float _agentHeight;
    private int _updateCount;
    private double _cadenceMs;

    public CrowdNavigationService(NavMeshService navMesh, GameServerSettings settings)
    {
        _navMesh = navMesh;
        _settings = settings;
        _logger = Log.Logger.ForContext<CrowdNavigationService>();
    }

    /// <summary>True when the crowd is switched on and the zone has a navmesh to run it on.</summary>
    public bool IsEnabled => _settings.CrowdNavigation && _navMesh.IsLoaded;

    public int AgentCount => _agents.Count;

    /// <summary>
    ///     Returns the NPC's crowd agent, creating it on first use.
    /// </summary>
    /// <param name="entityId">The NPC's entity id, the agent's identity in this service.</param>
    /// <param name="position">The NPC's current world position (the agent's start).</param>
    /// <param name="maxSpeed">The NPC's current target speed.</param>
    /// <param name="maxAcceleration">The NPC's maximum acceleration.</param>
    /// <returns>The agent, or null when the crowd is unavailable and the caller should steer itself.</returns>
    public DtCrowdAgent? AcquireAgent(ulong entityId, Vector3 position, float maxSpeed, float maxAcceleration)
    {
        if (!IsEnabled)
        {
            return null;
        }

        if (_agents.TryGetValue(entityId, out var existing))
        {
            return existing;
        }

        if (!TryEnsureCrowd())
        {
            return null;
        }

        // Only take on NPCs that are actually standing on the mesh, we can't steer it otherwise.
        if (!_navMesh.FindNearestPoly(position, _settings.CrowdMaxMeshGap, out _, out var onMesh))
        {
            if (_refusedNotice.Add(entityId))
            {
                _logger.Information(
                    "NPC {EntityId} at {Position} is {Gap:F1}u from the walkable navmesh (limit {Limit:F1}), so it keeps the path strategy instead of the crowd",
                    entityId,
                    position,
                    _navMesh.NearestMeshDistance(position),
                    _settings.CrowdMaxMeshGap);
            }

            return null;
        }

        var agent = _crowd!.AddAgent(NavMeshService.ToRc(onMesh), BuildParams(maxSpeed, maxAcceleration));
        if (agent == null)
        {
            return null;
        }

        _agents[entityId] = agent;
        return agent;
    }

    /// <summary>Drops an NPC's crowd agent. Safe to call for an id that has none.</summary>
    /// <param name="entityId">The NPC whose agent is removed.</param>
    public void RemoveAgent(ulong entityId)
    {
        if (_crowd == null || !_agents.Remove(entityId, out var agent))
        {
            _refusedNotice.Remove(entityId);
            return;
        }

        _crowd.RemoveAgent(agent);
    }

    /// <summary>
    ///     Drops every agent whose NPC is no longer in the shard.
    /// </summary>
    /// <param name="liveEntityIds">The entity ids seen by the caller this tick.</param>
    public void RemoveStaleAgents(IReadOnlySet<ulong> liveEntityIds)
    {
        if (_agents.Count == 0)
        {
            return;
        }

        _staleScratch.Clear();
        foreach (var (entityId, _) in _agents)
        {
            if (!liveEntityIds.Contains(entityId))
            {
                _staleScratch.Add(entityId);
            }
        }

        foreach (var entityId in _staleScratch)
        {
            RemoveAgent(entityId);
        }
    }

    /// <summary>
    ///     Points an agent at a world-space goal.
    /// </summary>
    /// <param name="agent">The agent to re-target.</param>
    /// <param name="goal">The world-space goal.</param>
    /// <returns>True when a goal was submitted; false when the goal is not on the mesh at all.</returns>
    public bool RequestGoal(DtCrowdAgent agent, Vector3 goal)
    {
        if (_crowd == null)
        {
            return false;
        }

        if (!_navMesh.FindNearestPoly(goal, _goalSnapRadius, out var polyRef, out _))
        {
            _crowd.ResetMoveTarget(agent);
            return false;
        }

        return _crowd.RequestMoveTarget(agent, polyRef, NavMeshService.ToRc(goal));
    }

    /// <summary>Clears an agent's goal so it stands still but stays registered as an obstacle for the others.</summary>
    /// <param name="agent">The agent to clear.</param>
    public void ResetGoal(DtCrowdAgent agent)
    {
        _crowd?.ResetMoveTarget(agent);
    }

    /// <summary>
    ///     Keeps the agent near the real body without detaching it from the mesh.
    ///
    ///     <c>npos</c> is not a mirror of the body: it is a position attached to a navmesh polygon. The crowd
    ///     snaps agents onto the mesh when it places them and re-snaps them whenever they drift off, and an
    ///     agent it cannot put back on the mesh is marked invalid and stops pathing at all. Writing the raw
    ///     body pose over it every tick undoes that snap — a character pivot sits above the walkable surface,
    ///     so every agent detaches on the first tick and never recovers. So: only re-sync when the body has
    ///     actually diverged, snap onto the mesh first, and leave the agent where it is when the body is off
    ///     the mesh entirely rather than pushing it off too.
    /// </summary>
    /// <param name="agent">The agent to re-sync.</param>
    /// <param name="position">The NPC's world position as physics last synced it.</param>
    public void SyncPose(DtCrowdAgent agent, Vector3 position)
    {
        var current = NavMeshService.FromRc(agent.npos);
        if ((current - position).LengthSquared() <= _poseResyncTolerance * _poseResyncTolerance)
        {
            // Close enough that the crowd's own integration is already tracking the body; rewriting it here
            // would only fight the snap the crowd just did.
            return;
        }

        if (!_navMesh.FindNearestPoly(position, _settings.CrowdMaxMeshGap, out _, out var onMesh))
        {
            // The body has left the mesh (walked off a ledge, knocked into the void). Keep the agent on its
            // last valid position so it can path back instead of going invalid and steering from nowhere.
            return;
        }

        agent.npos = NavMeshService.ToRc(onMesh);
    }

    /// <summary>
    ///     Advances the whole crowd one step. Must be called exactly once per movement tick, after every agent's
    ///     goal has been set and its pose synced, and before any agent's velocity is read.
    /// </summary>
    /// <param name="dt">The timestep, in seconds.</param>
    public void Update(float dt)
    {
        if (_crowd == null || _agents.Count == 0)
        {
            return;
        }

        var agents = _crowd.GetActiveAgents();
        EnsureScratch(agents);

        // Snapshot where the crowd is about to integrate from.
        foreach (var agent in agents)
        {
            _preUpdatePos[agent.idx] = NavMeshService.FromRc(agent.npos);
        }

        var start = Stopwatch.GetTimestamp();
        _crowd.Update(dt, null!);

        foreach (var agent in agents)
        {
            var before = _preUpdatePos[agent.idx];
            var vel = agent.vel;
            var after = NavMeshService.FromRc(agent.npos);
            var predicted = new Vector3(
                before.X + (vel.X * dt),
                before.Z + (vel.Z * dt),
                before.Y + (vel.Y * dt));

            // Horizontal only: vertical placement is gravity and the ground probe's job.
            _deoverlap[agent.idx] = new Vector3(after.X - predicted.X, after.Y - predicted.Y, 0f);
        }

        LogCadence(agents, start, dt);
    }

    /// <summary>
    ///     Reads the velocity the crowd planned for an agent, in world Z-up horizontal form.
    /// </summary>
    /// <param name="agent">The agent to read.</param>
    /// <param name="velocity">Receives the planned horizontal velocity.</param>
    /// <returns>
    ///     False when the crowd has no usable route (no target, or a target it failed to path to), in which case
    ///     the caller should steer directly instead of standing still.
    /// </returns>
    public bool TryReadVelocity(DtCrowdAgent agent, out Vector3 velocity)
    {
        velocity = Vector3.Zero;
        if (agent.state == DtCrowdAgentState.DT_CROWDAGENT_STATE_INVALID)
        {
            return false;
        }

        if (agent.targetState is DtMoveRequestState.DT_CROWDAGENT_TARGET_NONE or DtMoveRequestState.DT_CROWDAGENT_TARGET_FAILED)
        {
            return false;
        }

        var vel = agent.vel;
        velocity = new Vector3(vel.X, vel.Z, 0f);
        return true;
    }

    /// <summary>
    ///     The positional correction the crowd applied to de-overlap this agent this tick, beyond what its own
    ///     velocity explains. Horizontal, world Z-up.
    /// </summary>
    /// <param name="agent">The agent to read.</param>
    /// <param name="delta">Receives the horizontal correction in world units.</param>
    /// <returns>True when a correction was recorded for this agent.</returns>
    public bool TryGetDeoverlap(DtCrowdAgent agent, out Vector3 delta)
    {
        delta = Vector3.Zero;
        if (agent.idx >= _deoverlap.Length)
        {
            return false;
        }

        delta = _deoverlap[agent.idx];
        return delta.LengthSquared() > 1e-6f;
    }

    /// <summary>
    ///     Refreshes the speed an agent plans with, so a gait change (walk / sprint / crouch) takes effect on the
    ///     next crowd step rather than on the next re-path.
    /// </summary>
    /// <param name="agent">The agent to update.</param>
    /// <param name="maxSpeed">The NPC's current target speed.</param>
    /// <param name="maxAcceleration">The NPC's maximum acceleration.</param>
    public void SetSpeed(DtCrowdAgent agent, float maxSpeed, float maxAcceleration)
    {
        agent.option.maxSpeed = maxSpeed;
        agent.option.maxAcceleration = maxAcceleration;
    }

    private bool TryEnsureCrowd()
    {
        if (_crowd != null)
        {
            return true;
        }

        var mesh = _navMesh.NavMesh;
        if (mesh == null)
        {
            return false;
        }

        var parameters = new NavMeshBuildParams();
        _agentRadius = parameters.AgentRadius;
        _agentHeight = parameters.AgentHeight;

        // The filter factory is indexed by queryFilterType; every agent uses type 0, and PIN has no custom
        // filtering, so the crowd pathfinds with the same stock filter NavMeshService uses.
        _crowd = new DtCrowd(
            new DtCrowdConfig(_agentRadius * _maxAgentRadiusFactor),
            mesh,
            _ => _queryFilter);

        _logger.Information(
            "Crowd navigation enabled: agentRadius={Radius} agentHeight={Height} separationWeight={Separation}",
            _agentRadius,
            _agentHeight,
            _settings.CrowdSeparationWeight);
        return true;
    }

    private DtCrowdAgentParams BuildParams(float maxSpeed, float maxAcceleration)
    {
        // Obstacle avoidance and separation are independent: avoidance is "do not run into each other",
        // separation is "push apart even when not on a collision course".
        var config = new DtCrowdAgentConfig
        {
            anticipateTurns = true,
            optimizeVis = true,
            optimizeTopo = true,
            obstacleAvoidance = true,
            obstacleAvoidanceType = _obstacleAvoidanceType,
            separation = _settings.CrowdSeparationWeight > 0f,
            separationWeight = _settings.CrowdSeparationWeight,
        };

        return new DtCrowdAgentParams
        {
            radius = _agentRadius,
            height = _agentHeight,
            maxAcceleration = maxAcceleration,
            maxSpeed = maxSpeed,
            collisionQueryRange = _agentRadius * _collisionQueryRangeFactor,
            pathOptimizationRange = _agentRadius * _pathOptimizationRangeFactor,
            separationWeight = _settings.CrowdSeparationWeight,
            updateFlags = config.GetUpdateFlags(),
            obstacleAvoidanceType = _obstacleAvoidanceType,
            queryFilterType = 0,
        };
    }

    private void EnsureScratch(IList<DtCrowdAgent> agents)
    {
        int maxIndex = 0;
        foreach (var agent in agents)
        {
            maxIndex = Math.Max(maxIndex, agent.idx);
        }

        if (maxIndex >= _preUpdatePos.Length)
        {
            var size = maxIndex + 64;
            Array.Resize(ref _preUpdatePos, size);
            Array.Resize(ref _deoverlap, size);
        }
    }

    /// <summary>
    ///     Periodic Debug-level snapshot of what the crowd is doing. Only runs when
    ///     <see cref="GameServerSettings.CrowdDebugLogging" /> is on; the counters exist purely to feed it.
    /// </summary>
    private void LogCadence(IList<DtCrowdAgent> agents, long startTimestamp, float dt)
    {
        if (!_settings.CrowdDebugLogging)
        {
            return;
        }

        _updateCount++;

        long elapsedMs = (Stopwatch.GetTimestamp() - startTimestamp) * 1000 / Stopwatch.Frequency;
        _cadenceMs += elapsedMs;

        if (_updateCount % _logCadenceTicks != 0)
        {
            return;
        }

        int moving = 0;
        int offMesh = 0;
        int noPath = 0;
        float fastest = 0f;
        foreach (var agent in agents)
        {
            if (agent.state == DtCrowdAgentState.DT_CROWDAGENT_STATE_INVALID)
            {
                offMesh++;
            }

            if (agent.targetState is DtMoveRequestState.DT_CROWDAGENT_TARGET_NONE
                or DtMoveRequestState.DT_CROWDAGENT_TARGET_FAILED)
            {
                noPath++;
            }

            var speed = MathF.Sqrt((agent.vel.X * agent.vel.X) + (agent.vel.Z * agent.vel.Z));
            if (speed > 0.01f)
            {
                moving++;
            }

            fastest = Math.Max(fastest, speed);
        }

        _logger.Debug(
            "Crowd step: agents={Agents} moving={Moving} offMesh={OffMesh} noPath={NoPath} fastest={Fastest:F1}u/s dt={Dt:F1}ms mean={Mean:F2}ms",
            agents.Count,
            moving,
            offMesh,
            noPath,
            fastest,
            dt,
            _cadenceMs / (float)_logCadenceTicks);

        _cadenceMs = 0;
    }
}
