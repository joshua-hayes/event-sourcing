using Eventum.EventSourcing;

namespace Eventum.Persistence.Abstractions
{
    /// <summary>
    /// Used to snapshot and restore the internal state of an object to a previous state. 
    /// </summary>
    public interface ISnapshotStore
    {
        /// <summary>
        /// Saves the <see cref="SnapshotMemento"> to the store.
        /// </summary>
        /// <returns>The encapsulated <see cref="SnapshotMemento"/>.</returns>
        Task SaveSnapshotAsync();

        /// <summary>
        /// Loads a previously saved snapshot for the specified stream.
        /// </summary>
        /// <param name="memento">The <see cref="SnapshotMemento"/>, if it exists.</param>
        Task<SnapshotMemento> LoadSnapshotAsync(string streamId);
    }
}