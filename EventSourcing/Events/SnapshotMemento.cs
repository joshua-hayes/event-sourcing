using Newtonsoft.Json.Linq;

namespace EventSourcing.Events
{
    /// <summary>
    /// Provides the ability for an originator to restore its internal state to a previous state.
    /// </summary>
    public class SnapshotMemento
    {
        private JObject _state;

        public SnapshotMemento(JObject state)
        {
            _state = state;
        }

        /// <summary>
        /// The scoping here ensures the private state stays well encapsulated and cannot
        /// be accessed outside of this framework namespace.
        /// </summary>
        internal JObject State => _state;
    }
}