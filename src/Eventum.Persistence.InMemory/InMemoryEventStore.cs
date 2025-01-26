using Eventum.EventSourcing;
using Eventum.Persistence;
using Eventum.Telemetry;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;

namespace Eventum.Persistence.InMemory
{
    /// <summary>
    /// A basic in memory store for use in unit tests and testing.
    /// </summary>
    public class InMemoryStore : IEventStore
    {
        private readonly BlockingCollection<IEventStreamEvent> _events;
        private readonly ITelemetryProvider _telemetryProvider;


        public InMemoryStore(ITelemetryProvider telemetryProvider) :this(new BlockingCollection<IEventStreamEvent>(),
                                                                         telemetryProvider)
        {
        }

        public InMemoryStore(BlockingCollection<IEventStreamEvent> events, ITelemetryProvider telemetryProvider)
        {
            _events = events;
            _telemetryProvider = telemetryProvider;
        }

        /// <summary>
        /// <see cref="IEventStore.LoadStreamAsync{T}(string)"/>
        /// </summary>
        public async Task<T> LoadStreamAsync<T>(string streamId) where T : EventStream, new()
        {
            var stopwatch = Stopwatch.StartNew();
            try
            { 
                var events = _events.Where(e => e.StreamId == streamId)
                                    .OrderBy(e => e.Version)
                                    .ToArray();

                var stream = Activator.CreateInstance<T>();
                stream.LoadFromHistory(events);

                _telemetryProvider.TrackMetric("InMemoryStore.LoadStreamAsync.Time", stopwatch.ElapsedMilliseconds);
                return stream;
            }
            catch(Exception ex)
            {
                _telemetryProvider.TrackException(ex, new Dictionary<string, string>()
                {
                    { "Operation", "LoadStreamAsync" },
                    { "StreamId", streamId },
                    { "ErrorMessage", ex.Message},
                    { "StackTrace", ex.StackTrace},
                });
                throw;
            }
        }

        /// <summary>
        /// <see cref="IEventStore.SaveStreamAsync(EventStream, int)"/>
        /// </summary>
        public async Task<bool> SaveStreamAsync(EventStream stream, int expectedVersion)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var existingEvents = _events.Where(e => e.StreamId == stream.StreamId)
                                            .OrderBy(e => e.Version)
                                            .ToArray();

                if (existingEvents.Length != expectedVersion)
                {
                    _telemetryProvider.TrackEvent("InMemoryStore.SaveStreamAsync.VersionMismatch", new Dictionary<string, string>
                    {
                        { "StreamId", stream.StreamId },
                        { "ExpectedVersion", expectedVersion.ToString() },
                        { "ActualVersion", existingEvents.Length.ToString() }
                    }, TelemetryVerbosity.Warning);
                    return false;
                }

                foreach (var @event in stream.UncommittedChanges)
                {
                    @event.EventType = @event.GetType().Name;
                    @event.Version = ++expectedVersion;

                    _events.Add(@event);
                }

                _telemetryProvider.TrackMetric("InMemoryStore.SaveStreamAsync.Time", stopwatch.ElapsedMilliseconds);
                return true;
            }
            catch (Exception ex)
            {
                _telemetryProvider.TrackException(ex, new Dictionary<string, string>()
                {
                    { "Operation", "LoadStreamAsync" },
                    { "StreamId", stream.StreamId },
                    { "ErrorMessage", ex.Message},
                    { "StackTrace", ex.StackTrace},
                });
                throw;
            }
        }
    }
}
