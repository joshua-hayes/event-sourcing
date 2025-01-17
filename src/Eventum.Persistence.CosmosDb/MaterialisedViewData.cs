using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Eventum.Persistence.CosmosDb
{
    public class MaterialisedViewData
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("changeset")]
        public IList<string> Changeset { get; internal set; }

        [JsonPropertyName("view")]
        public JsonObject View { get; set; }
    }
}