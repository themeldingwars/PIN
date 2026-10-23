using System.Numerics;

namespace GameServer.Systems.Ai;

/// <summary>
///     A navigation strategy steers an NPC toward a destination.
/// </summary>
public interface INavigationStrategy
{
    /// <summary>Current movement speed (world units/second), set by the movement system each tick.</summary>
    float Speed { get; set; }

    void SetDestination(Vector3 destination);

    /// <summary>
    ///     Computes the steering velocity for the current tick. The result is horizontal (Y == 0);
    ///     the dynamic physics body handles vertical motion and ground contact.
    /// </summary>
    /// <param name="dt">The timestep, in seconds.</param>
    /// <param name="currentPos">The NPC's current world position.</param>
    /// <param name="steeringVelocity">Receives the desired horizontal steering velocity (Y == 0).</param>
    void Tick(float dt, Vector3 currentPos, out Vector3 steeringVelocity);

    bool HasArrived(Vector3 currentPos);

    void Reset();
}
