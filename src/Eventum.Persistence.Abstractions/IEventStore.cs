using Eventum.EventSourcing;
using System.Threading.Tasks;

namespace Eventum.Persistence
{
    public interface IEventStore
    {
        /// <summary>
        /// Loads and rehydrates a series of events into an event stream by the event
        /// streams stream identifier.
        /// </summary>
        /// <typeparam name="T">The generic type of the event stream being loaded.</typeparam>
        /// <param name="streamId">The identifier of the stream to load.</param>
        /// <returns>The hydrated event stream.</returns>
        Task<T> LoadStreamAsync<T>(string streamId) where T : EventStream;

        /// <summary>
        /// Saves changes to an event stream.
        /// </summary>
        /// <param name="stream">The updated event stream to save changes.</param>
        /// <param name="expectedVersion">The expected version of the event stream used during
        /// optimisitic concurrency control.</param>
        /// <returns>True if the stream was succeffully saved.</returns>
        Task<bool> SaveStreamAsync(EventStream stream, int expectedVersion);
    }
}
