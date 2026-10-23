using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using GameServer.Entities.Character;
using GameServer.Systems.Ai;

namespace GameServer;

public class AIEngine
{
    private readonly NpcBehaviorSystem _behavior;
    private readonly NpcMovementSystem _movement;

    public AIEngine(IShard shard)
    {
        _behavior = new NpcBehaviorSystem(shard);
        _movement = new NpcMovementSystem(shard);
    }

    public void Tick(double deltaTime, ulong currentTime, CancellationToken ct)
    {
        // Behavior runs first: it decides each NPC's state and sets movement goals that the movement system then steers.
        _behavior.Tick(deltaTime, currentTime, ct);
        _movement.Tick(deltaTime, currentTime, ct);
    }

    public void MoveTo(CharacterEntity npc, Vector3 destination)
    {
        _movement.MoveTo(npc, destination);
    }

    public void Stop(CharacterEntity npc)
    {
        _movement.Stop(npc);
    }

    public void Follow(CharacterEntity npc, ulong targetId, float leashRadius = NpcMovementSystem.FollowDefaultRadius)
    {
        _movement.Follow(npc, targetId, leashRadius);
    }

    public void ClearTarget(CharacterEntity npc)
    {
        _movement.ClearTarget(npc);
    }

    public void Patrol(CharacterEntity npc, IReadOnlyList<Vector3> route)
    {
        _movement.Patrol(npc, route);
    }

    public void ClearPatrol(CharacterEntity npc)
    {
        _movement.ClearPatrol(npc);
    }

    public void Wander(CharacterEntity npc, Vector3 center, float radius = NpcMovementSystem.WanderDefaultRadius)
    {
        _movement.Wander(npc, center, radius);
    }

    public void ClearWander(CharacterEntity npc)
    {
        _movement.ClearWander(npc);
    }

    public bool IsMoving(CharacterEntity npc)
    {
        return npc.NpcMovement is { IsMoving: true };
    }

    public bool HasTarget(CharacterEntity npc)
    {
        return npc.NpcMovement is { HasTarget: true };
    }
}
