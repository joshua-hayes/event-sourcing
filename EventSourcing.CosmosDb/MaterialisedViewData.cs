using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace EventSourcing.CosmosDb
{
    public class MaterialisedViewData
    {
        [JsonProperty("id", Order = 1)]
        public string Id { get; set; }

        [JsonProperty("changeset", Order = 2)]
        public IList<string> Changeset { get; internal set; }

        [JsonProperty("view", Order = 3)]
        public JObject View { get; set; }

    }
}