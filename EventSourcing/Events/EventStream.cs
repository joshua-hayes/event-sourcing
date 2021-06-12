using System.Collections.Generic;

namespace EventSourcing.Events
{
    public abstract class EventStream
    {
        private readonly List<IEventStreamEvent> _events;

        public EventStream()
        {
            _events = new List<IEventStreamEvent>();
        }

        public string StreamId { get; private set; }

        public int Version { get; private set; }

        /// <summary>
        ///     Gets the uncommitted changes.
        /// </summary>
        /// <returns>A list of uncommitted changes.</returns>
        public IEnumerable<IEventStreamEvent> UncommittedChanges => _events;

        /// <summary>
        ///     Loads the event stream from history.
        /// </summary>
        /// <param name="history">The history of events to load.</param>
        public void LoadFromHistory(IList<IEventStreamEvent> history)
        {
            foreach (var e in history)
                ApplyChange(e, false);
        }

        /// <summary>
        ///     Applies the event change to the event stream.
        /// </summary>
        /// <param name="event">The event.</param>
        /// <param name="isNewEvent">True if the event is a new event being added to the event stream.</param>
        protected void ApplyChange(IEventStreamEvent @event, bool isNewEvent = true)
        {
            try
            {
                ((dynamic)this).Handle((dynamic)@event);
                if (isNewEvent)
                    _events.Add(@event);
                else
                    Version = @event.Version;
            } catch
            {
                throw new EventStreamHandlerException(@event);
            }
        }
    }
}