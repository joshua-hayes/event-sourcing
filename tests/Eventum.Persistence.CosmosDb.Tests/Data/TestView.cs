using Eventum.Projection;
using System.Text.Json.Serialization;

namespace Eventum.Persistence.CosmosDb.Tests.Data
{
    public class TestView : MaterialisedView
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("registered")]
        public DateTime Registered { get; set; }
    }
}
