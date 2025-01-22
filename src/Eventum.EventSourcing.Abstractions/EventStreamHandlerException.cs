using System;

namespace Eventum.EventSourcing
{
    public class EventStreamHandlerException : Exception
    {
        public EventStreamHandlerException(IEventStreamEvent @event) :base($"No event handler found for event {@event?.GetType()?.Name}.")
        {

        }
    }
}