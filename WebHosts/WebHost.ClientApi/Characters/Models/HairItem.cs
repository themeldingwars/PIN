using WebHost.ClientApi.Models.Base;

namespace WebHost.ClientApi.Characters.Models
{
    public class HairItem : Item
    {
        public ColorItem Color { get; set; }
    }
}