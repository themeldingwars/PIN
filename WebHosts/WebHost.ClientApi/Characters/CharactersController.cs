using Microsoft.AspNetCore.Mvc;
using WebHost.ClientApi.Characters.Models;

namespace WebHost.ClientApi.Characters;

[ApiController]
public class CharactersController : ControllerBase
{
    private readonly ICharactersRepository _charactersRepository;

    public CharactersController(ICharactersRepository charactersRepository)
    {
        _charactersRepository = charactersRepository;
    }

    [Route("api/v2/characters/list")]
    [HttpGet]
    public CharactersList GetCharactersList()
    {
        return _charactersRepository.GetCharacters();
    }
}