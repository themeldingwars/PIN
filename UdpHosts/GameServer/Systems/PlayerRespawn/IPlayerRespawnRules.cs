using System.Numerics;
using GameServer.Entities.Character;

namespace GameServer.Systems.PlayerRespawn;

public interface IPlayerRespawnRules
{
    bool RespawnEnabled { get; }

    int DeadDurationMs { get; }

    int AllowRespawnAfterMs { get; }

    int RespawnHealthPercent { get; }

    Vector3? GetRespawnLocation(IShard shard, CharacterEntity character);
}
