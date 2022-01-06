using WebHost.ClientApi.Characters.Models;

namespace WebHost.ClientApi.Characters;

public interface ICharactersRepository
{
    CharactersList GetCharacters();
}