using System.Linq;
using System.Text.Json;

namespace Shared.Common.Infrastructure
{
    public class SnakeCasePropertyNamingPolicy : JsonNamingPolicy
    {
        public override string ConvertName(string name)
        {
            return string.Concat(name.Select((character, index) =>
                                                 index > 0 && char.IsUpper(character)
                                                     ? "_" + character
                                                     : character.ToString()))
                         .ToLower();
        }
    }
}