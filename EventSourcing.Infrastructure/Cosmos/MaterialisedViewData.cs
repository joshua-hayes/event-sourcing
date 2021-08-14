using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EventSourcing.Cosmos
{
    public class MaterialisedViewData
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("view")]
        public JObject View { get; set; }
    }
}