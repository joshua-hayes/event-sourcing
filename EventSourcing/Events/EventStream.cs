using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace EventSourcing.Events
{
    /// <summary>
    /// Represents a stream of events that exist within the same aggregate root.
    /// </summary>
    public abstract class EventStream : ISnapshotable
    {
        private readonly List<IEventStreamEvent> _events;
        protected JObject _snapshot;

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
        ///     Gets the uncommitted changes.
        /// </summary>
        /// <returns>A list of uncommitted changes.</returns>
        [JsonIgnore]
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
            } catch
            {
                throw new EventStreamHandlerException(@event);
            }
        }

        /// <summary>
        /// <see cref="ISnapshotable.IsSnapshotable"/>
        /// </summary>
        /// <remarks>Override if you plan to support snapshots.</remarks>
        public virtual bool IsSnapshotable => false;

        /// <summary>
        /// <see cref="ISnapshotable.SaveToSnapshot"/>
        /// </summary>
        public SnapshotMemento SaveToSnapshot()
        {
            var state = JObject.FromObject(this);
            var memento = new SnapshotMemento(state);

            return memento;
        }

        /// <summary>
        /// <see cref="ISnapshotable.LoadFromSnapshot(SnapshotMemento)"/>
        /// </summary>
        /// <remarks>Override if you plan to support snapshots.</remarks>
        public virtual void LoadFromSnapshot(SnapshotMemento memento)
        {
            _snapshot = memento.State;
            StreamId = _snapshot.GetValue("streamId")?.Value<string>();
            Version = _snapshot.GetValue("version")?.Value<int>() ?? 0;
        }
    }
}