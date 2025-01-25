using Eventum.Serialisation.Attributes;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Eventum.EventSourcing
{
    /// <summary>
    /// Represents a stream of events that exist within the same aggregate root.
    /// </summary>
    public abstract class EventStream : ISnapshotable
    {
        protected readonly List<IEventStreamEvent> _events;

        public EventStream()
        {
            _events = new List<IEventStreamEvent>();
        }

        /// <summary>
        /// The unique stream identifier.
        /// </summary>
        public string StreamId { get; set; }

        /// <summary>
        /// The current event stream version.
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Gets the uncommitted changes.
        /// </summary>
        /// <returns>A list of uncommitted changes.</returns>
        [IgnoreSerialization]
        public IEnumerable<IEventStreamEvent> UncommittedChanges => _events;

        /// <summary>
        /// <see cref="ISnapshotable.IsSnapshotable"/>
        /// </summary>
        /// <remarks>Override if you plan to support snapshots.</remarks>
        [IgnoreSerialization]
        public virtual bool IsSnapshotable => false;

        /// <summary>
        /// Loads the event stream from history.
        /// </summary>
        /// <param name="history">The history of events to load.</param>
        /// <param name="keepEvents">True to keep the events after loading history (useful when unit testing).</param>
        public void LoadFromHistory(IList<IEventStreamEvent> history, bool keepEvents = false)
        {
            foreach (var e in history)
                ApplyChange(e, keepEvents);
        }

        /// <summary>
        /// <see cref="ISnapshotable.LoadFromSnapshot(SnapshotMemento)"/>
        /// </summary>
        public virtual void LoadFromSnapshot(SnapshotMemento memento)
        {
            var props = GetType().GetProperties()
                                 .Where(prop => prop.GetCustomAttribute<IgnoreSerializationAttribute>() == null);

            foreach (var prop in props)
            {
                var propertyState = memento.GetState(prop.Name);
                if (propertyState != null)
                {
                    prop.SetValue(this, propertyState);
                }
            }
        }

        /// <summary>
        /// <see cref="ISnapshotable.SaveToSnapshot"/>
        /// </summary>
        public SnapshotMemento SaveToSnapshot()
        {
            return new SnapshotMemento(this);
        }

        /// <summary>
        ///     Applies the event change to the event stream.
        /// </summary>
        /// <param name="event">The event to apply.</param>
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
            }
            catch
            {
                throw new EventStreamHandlerException(@event);
            }
        }
    }
}