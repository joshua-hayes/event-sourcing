using System;
using System.Text.Json.Serialization;

namespace EventSourcing.Events
{
    /// <summary>
    /// Metadata definition for an event added to an event stream.
    /// </summary>
    public interface IEventStreamEvent
    {
        /// <summary>
        /// The unique event identifier.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }

        /// <summary>
        /// The type of event.
        /// </summary>
        [JsonPropertyName("eventType")]
        public string EventType { get; set; }

        /// <summary>
        /// The time the event was created.
        /// </summary>
        [JsonPropertyName("eventTime")]
        public DateTime EventTime { get; set; }

        /// <summary>
        /// The version associated with the event within the event stream.
        /// </summary>
        [JsonPropertyName("version")]
        public int Version { get; set; }
    }

    public interface IEventStreamEvent<T> : IEventStreamEvent
    {
        /// <summary>
        /// The main event data / payload.
        /// </summary>
        [JsonPropertyName("data")]
        public T Data { get; set; }
    }
}