using Eventum.EventSourcing;
using Eventum.Persistence;
using System.Collections.Concurrent;

namespace Eventum.Persistence.InMemory
{
    /// <summary>
    /// A basic in memory store for use in unit tests and testing.
    /// </summary>
    public class InMemoryStore : IEventStore
    {
        private readonly BlockingCollection<IEventStreamEvent> _events;

        public InMemoryStore() :this(new BlockingCollection<IEventStreamEvent>())
        {
        }

        public InMemoryStore(BlockingCollection<IEventStreamEvent> events)
        {
            _events = events;
        }

        /// <summary>
        /// <see cref="IEventStore.LoadStreamAsync{T}(string)"/>
        /// </summary>
        public async Task<T> LoadStreamAsync<T>(string streamId) where T : EventStream
        {
            var events = _events.Where(e => e.StreamId == streamId)
                                .OrderBy(e => e.Version)
                                .ToArray();

            var stream = Activator.CreateInstance<T>();
            stream.LoadFromHistory(events);
            
            return stream;
        }

        /// <summary>
        /// <see cref="IEventStore.SaveStreamAsync(EventStream, int)"/>
        /// </summary>
        public async Task<bool> SaveStreamAsync(EventStream stream, int expectedVersion)
        {
            var existingEvents = _events.Where(e => e.StreamId == stream.StreamId)
                                        .OrderBy(e => e.Version)
                                        .ToArray();
            
            if (existingEvents.Length != expectedVersion) 
                return false;

            foreach (var @event in stream.UncommittedChanges)
            {
                @event.EventType = @event.GetType().Name;
                @event.Version = ++expectedVersion;

                _events.Add(@event);
            }

            return true;
        }
    }
}
