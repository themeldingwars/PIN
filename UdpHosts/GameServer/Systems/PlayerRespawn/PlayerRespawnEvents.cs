using GameServer.Entities.Character;

namespace GameServer.Systems.PlayerRespawn;

public readonly record struct PlayerRespawnedEvent(
    CharacterEntity Target);
