using System.Collections.Generic;

namespace WebHost.ClientApi.Models
{
    public class TransformableSdbItem : SdbItem
    {
        public IEnumerable<decimal> Transform { get; set; }
    }
}