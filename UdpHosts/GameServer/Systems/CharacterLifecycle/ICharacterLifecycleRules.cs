using GameServer.Entities.Character;

namespace GameServer.Systems.CharacterLifecycle;

public interface ICharacterLifecycleRules
{
    bool BleedoutEnabled { get; }

    int BleedoutDurationMs { get; }

    int BleedoutDamageRateMs { get; }

    int BleedoutDamageAmount { get; }

    bool CanBleedout(CharacterEntity character);
}
