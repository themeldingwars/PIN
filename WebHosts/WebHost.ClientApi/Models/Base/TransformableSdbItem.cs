using System.Collections.Generic;

namespace WebHost.ClientApi.Models.Base
{
    public class TransformableSdbItem : SdbItem
    {
        public IEnumerable<decimal> Transform { get; set; }
    }
}