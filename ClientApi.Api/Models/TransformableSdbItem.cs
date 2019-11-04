using System.Collections.Generic;

namespace ClientApi.Api.Models
{
    public class TransformableSdbItem : SdbItem
    {
        public IEnumerable<decimal> Transform { get; set; }
    }
}