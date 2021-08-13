using EventSourcing.Projections;
using Newtonsoft.Json;
using System;

namespace EventSourcing.Test.Data
{
    public class TestView : MaterialisedView
    {
        [JsonProperty("name")]
        public string Name { get; internal set; }

        [JsonProperty("registered")]
        public DateTime Registered { get; internal set; }
    }
}
