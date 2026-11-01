using System.Numerics;
using GameServer.Data;
using GameServer.Protocol;

namespace GameServer.Systems.CharacterCreation;

public readonly record struct CharacterDataFetchedEvent(
    NetworkClient Client,
    CharacterData Data,
    long AccountId,
    Vector3? Arrival = null);
