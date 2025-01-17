using System.Threading.Tasks;

namespace Eventum.Events
{
    /// <summary>
    /// Provides a backing store for saving and loading event stream snapshots in the form of a memento.
    /// </summary>
    public interface ISnapshotStore
    {
        /// <summary>
        /// Saves a snapshot so that it can later be loaded.
        /// </summary>
        /// <param name="streamId">The unique stream id.</param>
        /// <param name="snapshot">The memento to store.</param>
        /// <returns>The task.</returns>
        Task SaveSnapshotAsync(string streamId, SnapshotMemento snapshot);

        /// <summary>
        /// Loads a snapshot by its corresponding stream identifier.
        /// </summary>
        /// <param name="streamId">The unique stream id.</param>
        /// <returns>The snapshot memento.</returns>
        Task<SnapshotMemento> LoadSnapshotAsync(string streamId);
    }
}