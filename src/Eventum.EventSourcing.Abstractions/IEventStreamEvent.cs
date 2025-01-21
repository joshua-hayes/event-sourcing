using System;

namespace Eventum.EventSourcing
{
    /// <summary>
    /// Metadata definition for an event added to an event stream.
    /// </summary>
    public interface IEventStreamEvent
    {
        /// <summary>
        /// The event stream identifier.
        /// </summary>
        public string StreamId { get; set; }

        /// <summary>
        /// The unique event identifier.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The type of event.
        /// </summary>
        public string EventType { get; set; }

        /// <summary>
        /// The time the event was created.
        /// </summary>
        public DateTime EventTime { get; set; }

        /// <summary>
        /// The version associated with the event within the event stream.
        /// </summary>
        public int Version { get; set; }
    }

    public interface IEventStreamEvent<T> : IEventStreamEvent
    {
        /// <summary>
        /// The main event data / payload.
        /// </summary>
        public T Data { get; set; }
    }
}