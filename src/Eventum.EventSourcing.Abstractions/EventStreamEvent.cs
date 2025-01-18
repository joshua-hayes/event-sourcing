using System;

namespace Eventum.EventSourcing
{
    public class EventStreamEvent : IEventStreamEvent
    {
        public string Id { get; set; }

        public string StreamId { get; set; }

        public DateTime EventTime { get; set; } = DateTime.UtcNow;

        public string EventType { get; set; }

        public int Version { get; set; }
    }

    public class EventStreamEvent<T> : EventStreamEvent
    {
        public T Data { get; set; }
    }
}