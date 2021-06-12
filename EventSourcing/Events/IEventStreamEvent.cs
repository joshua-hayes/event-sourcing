using Newtonsoft.Json;
using System;

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
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// The type of event.
        /// </summary>
        [JsonProperty("eventType")]
        public string EventType { get; set; }

        /// <summary>
        /// The time the event was created.
        /// </summary>
        [JsonProperty("eventTime")]
        public DateTime EventTime { get; set; }

        /// <summary>
        /// The version associated with the event within the event stream.
        /// </summary>
        [JsonProperty("version")]
        public int Version { get; set; }
    }

    public interface IEventStreamEvent<T> : IEventStreamEvent
    {
        /// <summary>
        /// The main event data / payload.
        /// </summary>
        [JsonProperty("data")]
        public T Data { get; set; }
    }
}