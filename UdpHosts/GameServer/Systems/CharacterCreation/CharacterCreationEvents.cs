using GameServer.Data;
using GameServer.Protocol;

namespace GameServer.Systems.CharacterCreation;

public readonly record struct CharacterDataFetchedEvent(
    NetworkClient Client,
    CharacterData Data);
