using GameServer.Entities;
using GameServer.Entities.Character;

namespace GameServer.Systems.CharacterLifecycle;

public enum CharacterLifecycleState
{
    Living = 0,
    Bleedout = 1,
    Dead = 2
}

public readonly record struct CharacterEnteredBleedoutEvent(
    CharacterEntity Target,
    ulong BleedoutExpiryAt);

public readonly record struct CharacterRevivedEvent(
    CharacterEntity Target,
    IEntity Reviver);

public readonly record struct CharacterDiedEvent(
    CharacterEntity Target);
