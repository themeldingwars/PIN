using WebHost.ClientApi.Models.Base;
using WebHost.ClientApi.Models.Characters;

namespace WebHost.ClientApi.Models
{
    public abstract class WarpaintPattern : TransformableSdbItem
    {
        public int Usage { get; set; }
    }
}