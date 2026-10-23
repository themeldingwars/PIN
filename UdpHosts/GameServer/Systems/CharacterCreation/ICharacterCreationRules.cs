using GameServer.Entities.Character;

namespace GameServer.Systems.CharacterCreation;

public interface ICharacterCreationRules
{
    uint FallbackChassisId { get; }
    void OnCharacterCreated(INetworkPlayer player, CharacterEntity character);
}
