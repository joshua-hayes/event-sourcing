using EventSourcing.Projection;
using System;
using System.Text.Json.Serialization;

namespace EventSourcing.Projection.Tests.Data
{
    public class TestView : MaterialisedView
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("registered")]
        public DateTime Registered { get; set; }
    }
}
