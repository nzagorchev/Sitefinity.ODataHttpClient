using Newtonsoft.Json;
using System.Collections.Generic;

namespace ConsumingODataWebServices
{
    public class ODataResult<T>
    {
        [JsonProperty("@odata.context")]
        public string Context { get; set; }

        public List<T> Value { get; set; }
    }
}
