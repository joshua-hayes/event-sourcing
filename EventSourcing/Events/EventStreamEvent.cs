using System;
using System.Text.Json.Serialization;

namespace EventSourcing.Events
{
    public class EventStreamEvent : IEventStreamEvent
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("streamId")]
        public string StreamId { get; set; }

        [JsonPropertyName("eventTime")]
        public DateTime EventTime { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("eventType")]
        public string EventType { get; set; }

        [JsonPropertyName("version")]
        public int Version { get; set; }
    }

    public class EventStreamEvent<T> : EventStreamEvent
    {
        [JsonPropertyName("data")]
        public T Data { get; set; }
    }
}