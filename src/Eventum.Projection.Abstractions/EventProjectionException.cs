using Eventum.EventSourcing;
using Eventum.Persistence.Abstractions;
using System;

namespace Eventum.Projection.Abstractions
{
    public class EventProjectionException : Exception
    {
        public EventProjectionException(string projectionName, IEventStreamEvent @event, Exception innerException)
            : base($"A materialised view error occurred whilst projecting {@event.GetType().Name} change through {projectionName}.", innerException)
        {

        }

        public EventProjectionException(string projectionName, MaterialisedView view, IEventStreamEvent @event)
                : base($"{projectionName}: Event handling issue or no event handler found when trying to project {@event.GetType().Name} change to {view.GetType().Name}.")
        {

        }
    }
}