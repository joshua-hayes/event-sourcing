using Newtonsoft.Json;
using System;

namespace EventSourcing.Events
{
    public class EventStreamEvent : IEventStreamEvent
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("streamId")]
        public string StreamId { get; set; }

        [JsonProperty("eventTime")]
        public DateTime EventTime { get; set; } = DateTime.UtcNow;

        [JsonProperty("eventType")]
        public string EventType { get; set; }

        [JsonProperty("version")]
        public int Version { get; set; }
    }

    public class EventStreamEvent<T> : EventStreamEvent
    {
        [JsonProperty("data")]
        public T Data { get; set; }
    }
}