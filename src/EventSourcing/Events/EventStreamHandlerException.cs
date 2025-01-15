using System;

namespace EventSourcing.Events
{
    public class EventStreamHandlerException : Exception
    {
        public EventStreamHandlerException(IEventStreamEvent @event) :base($"No event handler found for event {@event.GetType().Name}.")
        {

        }
    }
}