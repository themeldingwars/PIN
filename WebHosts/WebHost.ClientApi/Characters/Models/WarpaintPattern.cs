using WebHost.ClientApi.Models.Base;

namespace WebHost.ClientApi.Characters.Models;

public abstract class WarpaintPattern : TransformableSdbItem
{
    public int Usage { get; set; }
}