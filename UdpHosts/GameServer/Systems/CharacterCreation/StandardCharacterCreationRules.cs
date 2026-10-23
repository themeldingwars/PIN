using GameServer.Entities.Character;

namespace GameServer.Systems.CharacterCreation;

public class StandardCharacterCreationRules : ICharacterCreationRules
{
    public uint FallbackChassisId { get; init; } = 76331; // Mammoth

    public void OnCharacterCreated(INetworkPlayer player, CharacterEntity character)
    {
    }
}
