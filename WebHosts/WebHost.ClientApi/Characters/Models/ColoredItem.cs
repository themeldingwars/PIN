using WebHost.ClientApi.Models.Base;

namespace WebHost.ClientApi.Characters.Models
{
    public class ColoredItem : Item
    {
        public ColorValue Value { get; set; }
    }
}