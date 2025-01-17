using Eventum.Events;
using System.Text.Json.Serialization;

namespace Eventum.Projection.Tests.Data
{
    public class TestEvent1 : EventStreamEvent
    {
        public TestEvent1(string streamId, string field1, int field2)
        {
            StreamId = streamId;
            Field1 = field1;
            Field2 = field2;
        }

        [JsonPropertyName("field1")]
        public string Field1 { get; set; }

        [JsonPropertyName("field2")]
        public int Field2 { get; set; }
    }

    public class TestEvent2 : EventStreamEvent
    {
        public TestEvent2(string streamId, string field3, DateTime field4)
        {
            StreamId = streamId;
            Field3 = field3;
            Field4 = field4;
        }

        [JsonPropertyName("field4")]
        public string Field3 { get; set; }

        [JsonPropertyName("field4")]
        public DateTime Field4 { get; set; }
    }
}
