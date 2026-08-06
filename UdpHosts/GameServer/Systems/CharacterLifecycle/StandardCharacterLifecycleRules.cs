using GameServer.Entities.Character;

namespace GameServer.Systems.CharacterLifecycle;

public class StandardCharacterLifecycleRules : ICharacterLifecycleRules
{
    public bool BleedoutEnabled { get; init; } = true;

    public int BleedoutDurationMs { get; init; } = 15_000;

    public int BleedoutDamageRateMs { get; init; } = 1000;

    public int BleedoutDamageAmount { get; init; } = 100;

    public bool CanBleedout(CharacterEntity character)
    {
        return character.CanBleedout;
    }
}
