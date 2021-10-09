using System.Collections.Generic;

namespace WebHost.ClientApi.Characters.Models
{
    public class CharactersList
    {
        public IEnumerable<Character> Characters { get; set; }
        public bool IsDev { get; set; }
        public int RbBalance { get; set; }
        public int NameChangeCost { get; set; }
    }
}