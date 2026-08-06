using System.Numerics;
using GameServer.Entities.Character;

namespace GameServer.Systems.PlayerRespawn;

public class StandardPlayerRespawnRules : IPlayerRespawnRules
{
    public bool RespawnEnabled { get; init; } = true;

    public int DeadDurationMs { get; init; } = 2_500;

    public int AllowRespawnAfterMs { get; init; } = 5_000;

    public int RespawnHealthPercent { get; init; } = 100;

    public Vector3? GetRespawnLocation(IShard shard, CharacterEntity character)
    {
        return null;
    }
}
