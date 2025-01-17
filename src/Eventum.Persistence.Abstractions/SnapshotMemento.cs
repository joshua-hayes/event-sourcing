
using System.Runtime.CompilerServices;
using System.Text.Json;

[assembly: InternalsVisibleTo("Eventum.Test")]
namespace Eventum.Persistence.Abstractions
{
    /// <summary>
    /// Provides the ability for an originator to restore its internal state to a previous state.
    /// </summary>
    public class SnapshotMemento
    {
        private JsonDocument _state;

        public SnapshotMemento(JsonDocument state)
        {
            _state = state;
        }

        /// <summary>
        /// The scoping here ensures the private state stays well encapsulated and cannot
        /// be accessed outside of this framework namespace.
        /// </summary>
        internal JsonDocument State => _state;

        // Add a method to get the JsonElement
        public JsonElement GetState() {
            return _state.RootElement;
        }
    }
}