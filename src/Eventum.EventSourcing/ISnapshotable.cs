using System.Text.Json.Serialization;

namespace Eventum.EventSourcing
{
    /// <summary>
    /// Used to snapshot and restore the internal state of an object to a previous state. 
    /// </summary>
    public interface ISnapshotable
    {
        /// <summary>
        /// True if this event stream can save and restore state from a snapshot.
        /// </summary>
        [JsonIgnore]
        bool IsSnapshotable { get; }

        /// <summary>
        /// Saves the current internal state as a memento.
        /// </summary>
        /// <returns>The encapsulated <see cref="SnapshotMemento"/>.</returns>
        SnapshotMemento SaveToSnapshot();

        /// <summary>
        /// Loads the current internal state from a previously saved memento.
        /// </summary>
        /// <param name="memento">The memento to load state from.</param>
        void LoadFromSnapshot(SnapshotMemento memento);
    }
}