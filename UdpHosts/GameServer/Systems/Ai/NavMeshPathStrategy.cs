#nullable enable
using System;
using System.Collections.Generic;
using System.Numerics;
using GameServer.Navigation;

namespace GameServer.Systems.Ai;

/// <summary>
///     Velocity steering along a straight navmesh path.
///     Agents path on the navmesh (falling back to direct steering when no path exists).
/// </summary>
public sealed class NavMeshPathStrategy : INavigationStrategy
{
    private const float _waypointArriveDistance = 0.6f;

    // Ease the input speed toward the final destination so the NPC walks into its goal (Running -> Walking -> Standing)
    // instead of running at full speed and abruptly stopping. Kept short, and floored (below), so a slow/walking NPC
    // still closes the last bit and crosses the arrival radius instead of stalling just short of it — the movement
    // system's brake brings it to a full stop on arrival.
    private const float _decelerationRadius = 2.0f;
    private const float _minApproachSpeedFraction = 0.35f;

    private readonly NavMeshService? _nav;
    private List<Vector3>? _waypoints;
    private int _waypointIndex;
    private Vector3 _destination;
    private bool _hasDestination;

    public NavMeshPathStrategy(NavMeshService? nav)
    {
        _nav = nav;
    }

    public float Speed { get; set; }
    public float ArrivalRadius { get; set; } = 1.0f;

    public void SetDestination(Vector3 destination)
    {
        _destination = destination;
        _hasDestination = true;
        _waypoints = null; // force a (re)path on the next tick
        _waypointIndex = 0;
    }

    public void Tick(float dt, Vector3 currentPos, out Vector3 steeringVelocity)
    {
        steeringVelocity = Vector3.Zero;
        if (!_hasDestination)
        {
            return;
        }

        if (_waypoints == null)
        {
            _waypoints = BuildWaypoints(currentPos);
        }

        if (_waypoints.Count == 0)
        {
            return;
        }

        var target = _waypoints[Math.Min(_waypointIndex, _waypoints.Count - 1)];
        var toTarget = target - currentPos;
        toTarget.Z = 0f; // Firefall is Z-up: keep steering in the horizontal X-Y plane.

        var dist = toTarget.Length();

        if (dist < _waypointArriveDistance && _waypointIndex < _waypoints.Count - 1)
        {
            _waypointIndex++;
            target = _waypoints[_waypointIndex];
            toTarget = target - currentPos;
            toTarget.Z = 0f;

            dist = toTarget.Length();
        }

        if (dist > 0.001f)
        {
            var speed = Speed;

            // Near the final destination, ease the input speed down proportionally to the distance remaining, but never
            // below a floor so the NPC always reaches the goal (the brake stops it on arrival).
            if (_waypointIndex >= _waypoints.Count - 1 && dist < _decelerationRadius)
            {
                speed = MathF.Max(speed * (dist / _decelerationRadius), Speed * _minApproachSpeedFraction);
            }

            steeringVelocity = (toTarget / dist) * speed;
        }
    }

    public bool HasArrived(Vector3 currentPos)
    {
        if (!_hasDestination)
        {
            return false;
        }

        var toDest = _destination - currentPos;
        toDest.Z = 0f; // Firefall is Z-up: measure arrival in the horizontal plane only.

        return toDest.Length() <= ArrivalRadius;
    }

    public void Reset()
    {
        _hasDestination = false;
        _waypoints = null;
        _waypointIndex = 0;
    }

    private List<Vector3> BuildWaypoints(Vector3 currentPos)
    {
        // Agents without a usable navmesh steer straight at the destination.
        if (_nav == null || !_nav.IsLoaded)
        {
            return new List<Vector3> { _destination };
        }

        var path = _nav.FindPath(currentPos, _destination);
        if (path != null && path.Count > 1)
        {
            return path;
        }

        return new List<Vector3> { _destination };
    }
}
